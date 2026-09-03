using System;
using System.Collections.Generic;
using System.Text;
using NonsensicalKit.Core.DagLogicNode;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode.Editor
{
    [CustomEditor(typeof(DagNodeControlMax))]
    internal sealed class DagNodeControlMaxEditor : UnityEditor.Editor
    {
        private const float IndentPx = 14f;
        private const int MaxUncappedListItems = 50;

        private static readonly GUIContent GcKind = new GUIContent("节点类型");
        private static readonly GUIContent GcCombine = new GUIContent("子项关系");
        private static readonly GUIContent GcNodeId = new GUIContent("节点 ID");
        private static readonly GUIContent GcCheckType = new GUIContent("检查类型");
        private static readonly GUIContent GcAddCondition = new GUIContent("+ 条件");
        private static readonly GUIContent GcAddAnd = new GUIContent("+ 且组");
        private static readonly GUIContent GcAddOr = new GUIContent("+ 或组");
        private static readonly GUIContent GcDelete = new GUIContent("删除");
        private static readonly GUIContent GcUp = new GUIContent("↑");
        private static readonly GUIContent GcDown = new GUIContent("↓");
        private static readonly GUIContent GcControlGos = new GUIContent("控制物体");
        private static readonly GUIContent GcControlComps = new GUIContent("控制组件");

        private SerializedProperty _scriptProp;
        private SerializedProperty _nodesProp;
        private SerializedProperty _rootIndexProp;
        private SerializedProperty _controlGosProp;
        private SerializedProperty _controlCompsProp;

        private readonly Dictionary<int, bool> _groupExpanded = new Dictionary<int, bool>();
        private bool _showAllControlGos;
        private bool _showAllControlComps;

        private string _previewCache = string.Empty;
        private int _previewStamp = int.MinValue;

        private Action _deferredMutation;

        private void OnEnable()
        {
            CacheProperties();
            CollapseHugeList(_controlGosProp);
            CollapseHugeList(_controlCompsProp);
            _previewCache = string.Empty;
            _previewStamp = int.MinValue;
            _deferredMutation = null;
        }

        private void CacheProperties()
        {
            _scriptProp = serializedObject.FindProperty("m_Script");
            _nodesProp = serializedObject.FindProperty("m_logicNodes");
            _rootIndexProp = serializedObject.FindProperty("m_rootNodeIndex");
            _controlGosProp = serializedObject.FindProperty("m_controlGameObjects");
            _controlCompsProp = serializedObject.FindProperty("m_controlComponents");
        }

        private static void CollapseHugeList(SerializedProperty listProp)
        {
            if (listProp != null && listProp.isArray && listProp.arraySize > MaxUncappedListItems)
                listProp.isExpanded = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var ctrl = (DagNodeControlMax)target;
            CacheProperties();

            using (new EditorGUI.DisabledScope(true))
            {
                if (_scriptProp != null)
                    EditorGUILayout.PropertyField(_scriptProp);
            }

            if (!RootSerializedValid(_nodesProp, _rootIndexProp))
            {
                EditorGUILayout.HelpBox("尚无有效逻辑根。请点击下方创建空「或」组根节点。", MessageType.Warning);
                if (GUILayout.Button("创建逻辑根节点（空「或」组）", GUILayout.Height(28)))
                {
                    QueueMutation(() =>
                    {
                        Undo.RecordObject(ctrl, "创建逻辑根节点");
                        ctrl.CreateEmptyLogicalRoot();
                        EditorUtility.SetDirty(ctrl);
                    });
                }

                EditorGUILayout.Space(8);
                DrawControlLists();
                DrawRemainingProperties();
                FlushMutationOrApply();
                return;
            }

            EditorGUILayout.LabelField("激活条件（逻辑树）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "扁平表存储，无序列化深度限制。逻辑块可折叠。逻辑块内添加子项；子项关系选 且 / 或；删除会移除整棵子树。",
                MessageType.None);

            var rootIdx = _rootIndexProp.intValue;
            DrawNode(ctrl, rootIdx, -1, null, -1, true, 0);

            DrawPreview(rootIdx);

            EditorGUILayout.Space(8);
            DrawControlLists();
            DrawRemainingProperties();

            FlushMutationOrApply();
        }

        private void QueueMutation(Action action)
        {
            _deferredMutation = action;
        }

        private void FlushMutationOrApply()
        {
            if (_deferredMutation != null)
            {
                var action = _deferredMutation;
                _deferredMutation = null;
                action();
                serializedObject.Update();
                CacheProperties();
                _previewStamp = int.MinValue;
                GUIUtility.ExitGUI();
                return;
            }

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
            int nodeIndex,
            int parentNodeIndex,
            SerializedProperty parentChildIndicesProp,
            int slotInParent,
            bool isRoot,
            int depth)
        {
            if (_nodesProp == null || nodeIndex < 0 || nodeIndex >= _nodesProp.arraySize)
                return;

            var nodeProp = _nodesProp.GetArrayElementAtIndex(nodeIndex);
            var kindProp = nodeProp.FindPropertyRelative("Kind");
            if (kindProp == null)
                return;

            var kind = (DagNodeControlMax.LogicExprKind)kindProp.enumValueIndex;
            var isGroup = kind == DagNodeControlMax.LogicExprKind.Group;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(depth * IndentPx);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            bool expanded = true;
            if (isGroup)
            {
                if (!_groupExpanded.TryGetValue(nodeIndex, out expanded))
                    expanded = true;
                expanded = EditorGUILayout.Toggle(expanded, EditorStyles.foldout, GUILayout.Width(16f));
                _groupExpanded[nodeIndex] = expanded;
            }

            EditorGUILayout.PropertyField(kindProp, GcKind, GUILayout.MinWidth(180f));

            if (!isRoot && parentChildIndicesProp != null && slotInParent >= 0 && parentNodeIndex >= 0)
            {
                if (GUILayout.Button(GcDelete, GUILayout.Width(44f)))
                {
                    var capturedNode = nodeIndex;
                    var capturedParent = parentNodeIndex;
                    var capturedSlot = slotInParent;
                    QueueMutation(() =>
                    {
                        Undo.RecordObject(ctrl, "删除逻辑节点");
                        ctrl.RemoveNodeFromParent(capturedNode, capturedParent, capturedSlot);
                        EditorUtility.SetDirty(ctrl);
                    });
                }

                EditorGUI.BeginDisabledGroup(slotInParent <= 0);
                if (GUILayout.Button(GcUp, GUILayout.Width(22f)))
                {
                    var capturedParent = parentNodeIndex;
                    var capturedSlot = slotInParent;
                    QueueMutation(() =>
                    {
                        Undo.RecordObject(ctrl, "上移逻辑节点");
                        ctrl.SwapChildrenInGroup(capturedParent, capturedSlot, capturedSlot - 1);
                        EditorUtility.SetDirty(ctrl);
                    });
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(slotInParent >= parentChildIndicesProp.arraySize - 1);
                if (GUILayout.Button(GcDown, GUILayout.Width(22f)))
                {
                    var capturedParent = parentNodeIndex;
                    var capturedSlot = slotInParent;
                    QueueMutation(() =>
                    {
                        Undo.RecordObject(ctrl, "下移逻辑节点");
                        ctrl.SwapChildrenInGroup(capturedParent, capturedSlot, capturedSlot + 1);
                        EditorUtility.SetDirty(ctrl);
                    });
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndHorizontal();

            if (kind == DagNodeControlMax.LogicExprKind.Leaf)
            {
                DrawLeafFields(nodeProp);
            }
            else
            {
                var combineProp = nodeProp.FindPropertyRelative("Combine");
                EditorGUILayout.PropertyField(combineProp, GcCombine);

                if (expanded)
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GcAddCondition, GUILayout.Width(72f)))
                        QueueAddChild(ctrl, nodeIndex, ChildKind.Leaf);
                    if (GUILayout.Button(GcAddAnd, GUILayout.Width(72f)))
                        QueueAddChild(ctrl, nodeIndex, ChildKind.GroupAnd);
                    if (GUILayout.Button(GcAddOr, GUILayout.Width(72f)))
                        QueueAddChild(ctrl, nodeIndex, ChildKind.GroupOr);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(2);
                    var childrenProp = nodeProp.FindPropertyRelative("ChildIndices");
                    if (childrenProp != null && childrenProp.isArray)
                    {
                        if (childrenProp.arraySize == 0)
                        {
                            EditorGUILayout.HelpBox("（尚无子项：请用上方按钮添加）", MessageType.None);
                        }
                        else
                        {
                            for (var i = 0; i < childrenProp.arraySize; i++)
                            {
                                var childNodeIndex = childrenProp.GetArrayElementAtIndex(i).intValue;
                                DrawNode(ctrl, childNodeIndex, nodeIndex, childrenProp, i, false, depth + 1);
                            }
                        }
                    }
                }
                else
                {
                    var childrenProp = nodeProp.FindPropertyRelative("ChildIndices");
                    var count = childrenProp != null && childrenProp.isArray ? childrenProp.arraySize : 0;
                    EditorGUILayout.LabelField(
                        count == 0 ? "（已折叠）" : $"（已折叠，{count} 个子项）",
                        EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawLeafFields(SerializedProperty nodeProp)
        {
            var cond = nodeProp.FindPropertyRelative("Condition");
            if (cond == null)
                return;

            var nodeId = cond.FindPropertyRelative("m_nodeID");
            var checkType = cond.FindPropertyRelative("m_checkType");
            EditorGUILayout.PropertyField(nodeId, GcNodeId);
            EditorGUILayout.PropertyField(checkType, GcCheckType);
        }

        private enum ChildKind
        {
            Leaf,
            GroupAnd,
            GroupOr
        }

        private void QueueAddChild(DagNodeControlMax ctrl, int parentNodeIndex, ChildKind kind)
        {
            QueueMutation(() =>
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
            });
        }

        private void DrawPreview(int rootIdx)
        {
            if (serializedObject.hasModifiedProperties || _previewStamp == int.MinValue)
            {
                _previewStamp = 0;
                _previewCache = BuildPreview(_nodesProp, rootIdx);
            }

            if (string.IsNullOrEmpty(_previewCache))
                return;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("表达式预览", EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(_previewCache, MessageType.None);
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

            var combineProp = nodeProp.FindPropertyRelative("Combine");
            var combine = combineProp == null
                ? DagNodeControlMax.LogicCombineOp.Or
                : (DagNodeControlMax.LogicCombineOp)combineProp.enumValueIndex;
            var sep = combine == DagNodeControlMax.LogicCombineOp.And ? " ∧ " : " ∨ ";
            var children = nodeProp.FindPropertyRelative("ChildIndices");
            if (children == null || children.arraySize == 0)
                return "(空逻辑块)";

            var sb = new StringBuilder();
            if (children.arraySize > 1)
                sb.Append('(');
            for (var i = 0; i < children.arraySize; i++)
            {
                if (i > 0)
                    sb.Append(sep);
                sb.Append(BuildPreview(nodesProp, children.GetArrayElementAtIndex(i).intValue));
            }

            if (children.arraySize > 1)
                sb.Append(')');
            return sb.ToString();
        }

        private void DrawControlLists()
        {
            DrawCompactObjectList(_controlGosProp, GcControlGos, ref _showAllControlGos);
            DrawCompactObjectList(_controlCompsProp, GcControlComps, ref _showAllControlComps);
        }

        private static void DrawCompactObjectList(SerializedProperty listProp, GUIContent label, ref bool showAll)
        {
            if (listProp == null || !listProp.isArray)
                return;

            var count = listProp.arraySize;
            var foldLabel = count > 0 ? $"{label.text} ({count})" : label.text;
            listProp.isExpanded = EditorGUILayout.Foldout(listProp.isExpanded, foldLabel, true);
            if (!listProp.isExpanded)
                return;

            EditorGUI.indentLevel++;

            var drawCount = count;
            var capped = false;
            if (!showAll && count > MaxUncappedListItems)
            {
                drawCount = MaxUncappedListItems;
                capped = true;
            }

            for (var i = 0; i < drawCount; i++)
            {
                var el = listProp.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(el, GUIContent.none);
                if (GUILayout.Button("×", EditorStyles.miniButton, GUILayout.Width(20f)))
                {
                    el.objectReferenceValue = null;
                    listProp.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (capped)
            {
                if (GUILayout.Button($"还有 {count - MaxUncappedListItems} 项，展开全部"))
                    showAll = true;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("添加", EditorStyles.miniButton))
                listProp.arraySize++;
            EditorGUI.BeginDisabledGroup(listProp.arraySize <= 0);
            if (GUILayout.Button("移除末项", EditorStyles.miniButton))
            {
                var last = listProp.arraySize - 1;
                listProp.GetArrayElementAtIndex(last).objectReferenceValue = null;
                listProp.DeleteArrayElementAtIndex(last);
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        private void DrawRemainingProperties()
        {
            DrawPropertiesExcluding(serializedObject,
                "m_Script",
                "m_logicNodes",
                "m_rootNodeIndex",
                "m_controlGameObjects",
                "m_controlComponents");
        }
    }
}
