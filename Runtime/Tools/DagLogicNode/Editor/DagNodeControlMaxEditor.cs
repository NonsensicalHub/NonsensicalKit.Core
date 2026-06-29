using System.Collections.Generic;
using NonsensicalKit.Core.DagLogicNode;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode.Editor
{
    [CustomEditor(typeof(DagNodeControlMax))]
    internal sealed class DagNodeControlMaxEditor : UnityEditor.Editor
    {
        private const float IndentPx = 14f;

        private SerializedProperty _nodesProp;
        private SerializedProperty _rootIndexProp;

        private string _previewCache = string.Empty;

        private void OnEnable()
        {
            _nodesProp = serializedObject.FindProperty("m_logicNodes");
            _rootIndexProp = serializedObject.FindProperty("m_rootNodeIndex");
            _previewCache = string.Empty;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var ctrl = (DagNodeControlMax)target;

            _nodesProp = serializedObject.FindProperty("m_logicNodes");
            _rootIndexProp = serializedObject.FindProperty("m_rootNodeIndex");

            if (!RootSerializedValid(_nodesProp, _rootIndexProp))
            {
                EditorGUILayout.HelpBox("尚无有效逻辑根。请点击下方创建空「或」组根节点。", MessageType.Warning);
                if (GUILayout.Button("创建逻辑根节点（空「或」组）", GUILayout.Height(28)))
                {
                    Undo.RecordObject(ctrl, "创建逻辑根节点");
                    ctrl.CreateEmptyLogicalRoot();
                    EditorUtility.SetDirty(ctrl);
                    serializedObject.Update();
                    _nodesProp = serializedObject.FindProperty("m_logicNodes");
                    _rootIndexProp = serializedObject.FindProperty("m_rootNodeIndex");
                }

                EditorGUILayout.Space(8);
                DrawPropertiesExcluding(serializedObject,
                    "m_logicNodes", "m_rootNodeIndex");
                // 切勿在 Apply 前调用 Update()，否则会覆盖本帧 PropertyField 尚未写回对象的修改。
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.LabelField("激活条件（逻辑树）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "扁平表存储，无序列化深度限制。逻辑块内添加子项；子项关系选 且 / 或；删除会移除整棵子树。",
                MessageType.None);

            EditorGUILayout.Space(4);
            var rootIdx = _rootIndexProp.intValue;
            DrawNode(ctrl, _nodesProp, rootIdx, -1, null, -1, true, 0);

            var guiEvent = Event.current;
            if (guiEvent != null && guiEvent.type == EventType.Repaint)
                _previewCache = BuildPreview(_nodesProp, rootIdx);

            if (!string.IsNullOrEmpty(_previewCache))
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("表达式预览", EditorStyles.miniBoldLabel);
                EditorGUILayout.HelpBox(_previewCache, MessageType.None);
            }

            EditorGUILayout.Space(10);
            DrawPropertiesExcluding(serializedObject,
                "m_logicNodes", "m_rootNodeIndex");

            serializedObject.ApplyModifiedProperties();
        }

        private static bool RootSerializedValid(SerializedProperty nodesProp, SerializedProperty rootIndexProp)
        {
            if (nodesProp == null || !nodesProp.isArray || nodesProp.arraySize <= 0)
                return false;
            if (rootIndexProp == null)
                return false;
            var r = rootIndexProp.intValue;
            return r >= 0 && r < nodesProp.arraySize;
        }

        private void DrawNode(
            DagNodeControlMax ctrl,
            SerializedProperty nodesProp,
            int nodeIndex,
            int parentNodeIndex,
            SerializedProperty parentChildIndicesProp,
            int slotInParent,
            bool isRoot,
            int depth)
        {
            if (nodesProp == null || nodeIndex < 0 || nodeIndex >= nodesProp.arraySize)
                return;

            var nodeProp = nodesProp.GetArrayElementAtIndex(nodeIndex);
            var kindProp = nodeProp.FindPropertyRelative("Kind");
            if (kindProp == null)
                return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(depth * IndentPx);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(kindProp, new GUIContent("节点类型"), GUILayout.MinWidth(180f));

            if (!isRoot && parentChildIndicesProp != null && slotInParent >= 0 && parentNodeIndex >= 0)
            {
                if (GUILayout.Button("删除", GUILayout.Width(44f)))
                {
                    Undo.RecordObject(ctrl, "删除逻辑节点");
                    ctrl.RemoveNodeFromParent(nodeIndex, parentNodeIndex, slotInParent);
                    EditorUtility.SetDirty(ctrl);
                    serializedObject.Update();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    GUIUtility.ExitGUI();
                    return;
                }

                EditorGUI.BeginDisabledGroup(slotInParent <= 0);
                if (GUILayout.Button("↑", GUILayout.Width(22f)))
                {
                    Undo.RecordObject(ctrl, "上移逻辑节点");
                    ctrl.SwapChildrenInGroup(parentNodeIndex, slotInParent, slotInParent - 1);
                    EditorUtility.SetDirty(ctrl);
                    serializedObject.Update();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    GUIUtility.ExitGUI();
                    return;
                }

                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(slotInParent >= parentChildIndicesProp.arraySize - 1);
                if (GUILayout.Button("↓", GUILayout.Width(22f)))
                {
                    Undo.RecordObject(ctrl, "下移逻辑节点");
                    ctrl.SwapChildrenInGroup(parentNodeIndex, slotInParent, slotInParent + 1);
                    EditorUtility.SetDirty(ctrl);
                    serializedObject.Update();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    GUIUtility.ExitGUI();
                    return;
                }

                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndHorizontal();

            var kind = (DagNodeControlMax.LogicExprKind)kindProp.enumValueIndex;

            if (kind == DagNodeControlMax.LogicExprKind.Leaf)
            {
                DrawLeafFields(nodeProp);
            }
            else
            {
                var combineProp = nodeProp.FindPropertyRelative("Combine");
                EditorGUILayout.PropertyField(combineProp, new GUIContent("子项关系"));

                var childrenProp = nodeProp.FindPropertyRelative("ChildIndices");
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+ 条件", GUILayout.Width(72f)))
                    AddChild(ctrl, nodeIndex, ChildKind.Leaf);
                if (GUILayout.Button("+ 且组", GUILayout.Width(72f)))
                    AddChild(ctrl, nodeIndex, ChildKind.GroupAnd);
                if (GUILayout.Button("+ 或组", GUILayout.Width(72f)))
                    AddChild(ctrl, nodeIndex, ChildKind.GroupOr);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(2);
                if (childrenProp != null && childrenProp.isArray)
                {
                    if (childrenProp.arraySize == 0)
                        EditorGUILayout.HelpBox("（尚无子项：请用上方按钮添加）", MessageType.None);
                    else
                    {
                        for (var i = 0; i < childrenProp.arraySize; i++)
                        {
                            var childIdxProp = childrenProp.GetArrayElementAtIndex(i);
                            var childNodeIndex = childIdxProp.intValue;
                            DrawNode(ctrl, nodesProp, childNodeIndex, nodeIndex, childrenProp, i, false, depth + 1);
                        }
                    }
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private enum ChildKind
        {
            Leaf,
            GroupAnd,
            GroupOr
        }

        private void AddChild(DagNodeControlMax ctrl, int parentNodeIndex, ChildKind kind)
        {
            Undo.RecordObject(ctrl, "添加逻辑子节点");
            int childIdx;
            switch (kind)
            {
                case ChildKind.Leaf:
                    childIdx = ctrl.CreateLeafNode(string.Empty, DagNodeCheckType.SelfSelect.ToString());
                    break;
                case ChildKind.GroupAnd:
                    childIdx = ctrl.CreateEmptyGroupNode(DagNodeControlMax.LogicCombineOp.And);
                    break;
                case ChildKind.GroupOr:
                    childIdx = ctrl.CreateEmptyGroupNode(DagNodeControlMax.LogicCombineOp.Or);
                    break;
                default:
                    return;
            }

            ctrl.AppendChildIndexToGroup(parentNodeIndex, childIdx);
            EditorUtility.SetDirty(ctrl);
            serializedObject.Update();
        }

        private static void DrawLeafFields(SerializedProperty nodeProp)
        {
            var cond = nodeProp.FindPropertyRelative("Condition");
            if (cond == null)
                return;

            var nodeId = cond.FindPropertyRelative("m_nodeID");
            var checkType = cond.FindPropertyRelative("m_checkType");

            EditorGUILayout.PropertyField(nodeId, new GUIContent("节点 ID"));
            EditorGUILayout.PropertyField(checkType, new GUIContent("检查类型"));
        }

        private static string BuildPreview(SerializedProperty nodesProp, int nodeIndex)
        {
            if (nodesProp == null || nodeIndex < 0 || nodeIndex >= nodesProp.arraySize)
                return string.Empty;

            var nodeProp = nodesProp.GetArrayElementAtIndex(nodeIndex);
            var kindProp = nodeProp.FindPropertyRelative("Kind");
            if (kindProp == null)
                return string.Empty;

            var kind = (DagNodeControlMax.LogicExprKind)kindProp.enumValueIndex;
            if (kind == DagNodeControlMax.LogicExprKind.Leaf)
            {
                var cond = nodeProp.FindPropertyRelative("Condition");
                var id = cond?.FindPropertyRelative("m_nodeID")?.stringValue ?? string.Empty;
                return string.IsNullOrEmpty(id) ? "（未填节点 ID）" : id;
            }

            var combine = (DagNodeControlMax.LogicCombineOp)nodeProp.FindPropertyRelative("Combine").enumValueIndex;
            var sep = combine == DagNodeControlMax.LogicCombineOp.And ? " ∧ " : " ∨ ";
            var children = nodeProp.FindPropertyRelative("ChildIndices");
            if (children == null || children.arraySize == 0)
                return "(空逻辑块)";

            var parts = new List<string>(children.arraySize);
            for (var i = 0; i < children.arraySize; i++)
            {
                var ci = children.GetArrayElementAtIndex(i).intValue;
                parts.Add(BuildPreview(nodesProp, ci));
            }

            var inner = string.Join(sep, parts);
            return children.arraySize == 1 ? inner : $"({inner})";
        }
    }
}
