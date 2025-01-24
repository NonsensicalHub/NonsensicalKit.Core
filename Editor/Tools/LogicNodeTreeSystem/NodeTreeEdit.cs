using System;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Tools.LogicNodeTreeSystem.Editor
{
    /// <summary>
    /// 参考：https://github.com/Unity-Technologies/UnityCsReference/blob/d0fe81a19ce788fd1d94f826cf797aafc37db8ea/Editor/Mono/GUI/ReorderableList.cs
    /// 
    /// 节点树自定义编辑器，当使用ISerializationCallbackReceiver自定义序列化来避免自循环序列化类报警时，使用这个工具类来绘制节点树
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NodeTreeEdit<T> where T : TreeData<T>
    {
        public delegate string GetHeaderStringDelegate(T node);

        public GetHeaderStringDelegate GetHeadString;
        public Action<Rect, T> OnDrawElement;
        public Action<Rect, T> OnDrawFooter;
        public Action<T> OnAddElement;
        public Action<T> OnRemoveElement;
        public Action<T> OnRemoveSelf;

        public float ElementHeight = 20;
        public float HeaderHeight = 20;
        public float FooterHeight = 20;
        public float LevelIndent = 10; //每级缩进

        private T _root;
        private T _removeBuffer;

        private static Style _defaults;

        private bool _changed;

        public static Style Defaults
        {
            get
            {
                return _defaults ??= new Style();
            }
        }

        public class Style
        {
            public GUIContent IconToolbarPlus = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add to the list");
            public GUIContent IconClose = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove self");
            public GUIContent IconToolbarMinus = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove last from the list");
            public readonly GUIStyle HeaderBackground = "RL Header";
            public readonly GUIStyle FooterBackground = "RL Footer";
            public readonly GUIStyle BoxBackground = "RL Background";
            public readonly GUIStyle ElementBackground = "RL Element";
            public readonly GUIStyle PreButton = "RL FooterButton";

            public readonly GUIStyle FoldoutHeader = EditorStyles.foldout;
        }

        public NodeTreeEdit(T root)
        {
            this._root = root;
        }

        public float GetTotalHeight()
        {
            return GetHeight(_root);
        }

        /// <summary>
        /// 每次绘制都需要多次调用此方法，且同一节点会重复获取多次（先父级后自己），可以将每个节点的高度信息缓存
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private float GetHeight(T node)
        {
            float totalHeiget = 0;
            totalHeiget += HeaderHeight;
            totalHeiget += ElementHeight;
            if (!node.IsFoldout)
            {
                return totalHeiget;
            }

            totalHeiget += FooterHeight;
            var childs = node.GetChildren();
            if (childs.Count > 0)
            {
                foreach (var item in childs)
                {
                    totalHeiget += GetHeight(item);
                }
            }

            return totalHeiget;
        }

        public bool DrawNodeTree(Rect rect)
        {
            _changed = false;
            DrawNode(rect, _root);

            if (_removeBuffer != null)
            {
                OnRemoveSelf?.Invoke(_removeBuffer);
                _removeBuffer = null;
            }

            return _changed;
        }

        private Rect DrawNode(Rect rect, T node)
        {
            Rect headerRect = new Rect(rect.x, rect.y, rect.width, HeaderHeight);
            DrawListHeader(headerRect, node);
            rect.y += HeaderHeight;

            if (node.IsFoldout)
            {
                if (node.GetChildren().Count > 0)
                {
                    float spaceHeight = GetHeight(node) - (HeaderHeight + FooterHeight);


                    Rect elementRect = new Rect(rect.x, rect.y, rect.width, spaceHeight);
                    DrawElement(elementRect, node);

                    Rect listRect = new Rect(rect.x, rect.y + ElementHeight, rect.width, spaceHeight);
                    DrawListElements(listRect, node);

                    rect.y += spaceHeight;
                }
                else
                {
                    Rect elementRect = new Rect(rect.x, rect.y, rect.width, ElementHeight);
                    DrawElement(elementRect, node);
                    rect.y += ElementHeight;
                }

                Rect footerRect = new Rect(rect.x, rect.y, rect.width, FooterHeight);
                DrawListFooter(footerRect, node);
                rect.y += FooterHeight;
            }
            else
            {
                Rect elementRect = new Rect(rect.x, rect.y, rect.width, ElementHeight);
                DrawElement(elementRect, node);
                rect.y += ElementHeight;
            }

            return rect;
        }

        private void DrawListHeader(Rect headerRect, T node)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Defaults.HeaderBackground.Draw(headerRect, false, false, false, false);
            }

            int childCount = node.GetChildren().Count;

            var headerString = GetHeadString != null ? GetHeadString(node) : "Node";
            if (childCount > 0)
            {
                var toggleRect = headerRect;
                toggleRect.xMax -= 20;
                EditorGUI.BeginChangeCheck();

                node.IsFoldout = GUI.Toggle(toggleRect, node.IsFoldout, headerString, Defaults.FoldoutHeader);
                if (EditorGUI.EndChangeCheck())
                {
                    _changed = true;
                }
            }
            else
            {
                GUI.Label(headerRect, headerString);
            }

            headerRect.x = headerRect.xMax - 110;
            GUI.Label(headerRect, "子节点数量：" + childCount);

            headerRect.x += 90;
            headerRect.width = headerRect.height;
            if (GUI.Button(headerRect, Defaults.IconClose, Defaults.PreButton))
            {
                _removeBuffer = node;
                _changed = true;
            }
        }

        private void DrawElement(Rect elementRect, T node)
        {
            // draw the background in repaint
            if (Event.current.type == EventType.Repaint)
                Defaults.BoxBackground.Draw(elementRect, false, true, true, true);

            elementRect.height = ElementHeight;
            elementRect.x += 2;
            elementRect.width -= 4;
            elementRect.y += 2;
            elementRect.height -= 4;
            EditorGUI.BeginChangeCheck();
            OnDrawElement?.Invoke(elementRect, node);
            if (EditorGUI.EndChangeCheck())
            {
                _changed = true;
            }
        }

        private void DrawListElements(Rect listRect, T node)
        {
            listRect.xMin += LevelIndent;
            foreach (var item in node.GetChildren())
            {
                listRect = DrawNode(listRect, item);
            }
        }

        private void DrawListFooter(Rect footerRect, T node)
        {
            // perform callback or the default footer
            if (OnDrawFooter != null)
            {
                EditorGUI.BeginChangeCheck();
                OnDrawFooter(footerRect, node);
                if (EditorGUI.EndChangeCheck())
                {
                    _changed = true;
                }
            }
            else
            {
                float leftEdge = footerRect.xMin + 5f;
                float rightEdge = leftEdge + 33f;
                footerRect = new Rect(leftEdge, footerRect.y, rightEdge - leftEdge, footerRect.height);
                Rect addRect = new Rect(leftEdge + 4, footerRect.y, 25, 16);
                if (Event.current.type == EventType.Repaint)
                {
                    Defaults.FooterBackground.Draw(footerRect, false, false, false, false);
                }

                if (GUI.Button(addRect, Defaults.IconToolbarPlus, Defaults.PreButton))
                {
                    OnAddElement?.Invoke(node);
                    _changed = true;
                }
            }
        }
    }
}
