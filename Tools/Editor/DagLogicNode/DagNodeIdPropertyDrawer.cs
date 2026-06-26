using System;
using System.Collections.Generic;
using System.Linq;
using NonsensicalKit.Core.DagLogicNode;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode.Editor
{
    [CustomPropertyDrawer(typeof(DagNodeIdAttribute))]
    public class DagNodeIdPropertyDrawer : PropertyDrawer
    {
        private static readonly string EmptyLabel = "(None)";
        private static readonly string MissingSuffix = " (Missing)";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "DagNodeIdAttribute 仅支持 string 字段");
                return;
            }

            var graphs = ResolveGraphs(property.serializedObject.targetObject as Component);
            if (graphs.Count == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var attr = (DagNodeIdAttribute)attribute;
            string currentValue = property.stringValue ?? string.Empty;

            // 计算显示文本
            string displayText;
            if (string.IsNullOrEmpty(currentValue))
            {
                displayText = attr.AllowEmpty ? EmptyLabel : "(Select)";
            }
            else
            {
                // 尝试找到当前值对应的图形名称
                string graphName = FindGraphNameForNode(graphs, currentValue);
                displayText = string.IsNullOrEmpty(graphName) ? currentValue : $"[{graphName}] {currentValue}";
                if (string.IsNullOrEmpty(graphName) && !NodeExistsInGraphs(graphs, currentValue))
                {
                    displayText = currentValue + MissingSuffix;
                }
            }

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            bool isPressed = EditorGUI.DropdownButton(fieldRect, new GUIContent(displayText), FocusType.Passive);

            if (isPressed)
            {
                var dropdown = new DagNodeIdAdvancedDropdown(
                    new AdvancedDropdownState(),
                    graphs,
                    attr.AllowEmpty,
                    currentValue,
                    selectedNodeId =>
                    {
                        property.stringValue = selectedNodeId;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                dropdown.Show(fieldRect);
            }
        }

        private static string FindGraphNameForNode(List<DagGraph> graphs, string nodeId)
        {
            foreach (var graph in graphs)
            {
                if (graph?.nodes == null) continue;
                foreach (var node in graph.nodes)
                {
                    if (node != null && node.nodeId == nodeId)
                    {
                        return string.IsNullOrWhiteSpace(graph.GraphName) ? "Unnamed Graph" : graph.GraphName.Trim();
                    }
                }
            }
            return null;
        }

        private static bool NodeExistsInGraphs(List<DagGraph> graphs, string nodeId)
        {
            foreach (var graph in graphs)
            {
                if (graph?.nodes == null) continue;
                foreach (var node in graph.nodes)
                {
                    if (node != null && node.nodeId == nodeId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 分组下拉菜单：按 DagGraphConfig 分组显示节点。
        /// </summary>
        private class DagNodeIdAdvancedDropdown : AdvancedDropdown
        {
            private readonly List<DagGraph> _graphs;
            private readonly bool _allowEmpty;
            private readonly string _currentValue;
            private readonly Action<string> _onSelected;

            public DagNodeIdAdvancedDropdown(
                AdvancedDropdownState state,
                List<DagGraph> graphs,
                bool allowEmpty,
                string currentValue,
                Action<string> onSelected)
                : base(state)
            {
                _graphs = graphs ?? new List<DagGraph>();
                _allowEmpty = allowEmpty;
                _currentValue = currentValue ?? string.Empty;
                _onSelected = onSelected;

                minimumSize = new Vector2(220, 300);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("DAG Nodes");

                int itemId = 0;

                // Empty 选项
                if (_allowEmpty)
                {
                    var emptyItem = new AdvancedDropdownItem(EmptyLabel) { id = itemId++ };
                    if (string.IsNullOrEmpty(_currentValue))
                    {
                        emptyItem.icon = EditorGUIUtility.IconContent("Checkmark").image as Texture2D;
                    }
                    root.AddChild(emptyItem);
                }

                bool hasMultipleGraphs = _graphs.Count > 1;

                foreach (var graph in _graphs)
                {
                    if (graph?.nodes == null || graph.nodes.Count == 0)
                    {
                        continue;
                    }

                    string graphName = string.IsNullOrWhiteSpace(graph.GraphName) ? "Unnamed Graph" : graph.GraphName.Trim();

                    AdvancedDropdownItem groupItem;
                    if (hasMultipleGraphs)
                    {
                        groupItem = new AdvancedDropdownItem(graphName) { id = itemId++ };
                        root.AddChild(groupItem);
                    }
                    else
                    {
                        // 只有一个图时，不额外创建分组，直接挂在 root 下
                        groupItem = root;
                    }

                    // 按 nodeId 排序
                    var sortedNodes = graph.nodes
                        .Where(n => n != null && !string.IsNullOrWhiteSpace(n.nodeId))
                        .OrderBy(n => n.nodeId)
                        .ToList();

                    foreach (var node in sortedNodes)
                    {
                        string nodeId = node.nodeId.Trim();
                        var nodeItem = new AdvancedDropdownItem(nodeId) { id = itemId++ };

                        if (nodeId == _currentValue)
                        {
                            nodeItem.icon = EditorGUIUtility.IconContent("Checkmark").image as Texture2D;
                        }

                        groupItem.AddChild(nodeItem);
                    }
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                base.ItemSelected(item);

                if (item.name == EmptyLabel)
                {
                    _onSelected?.Invoke(string.Empty);
                    return;
                }

                // 找到对应的 nodeId（item.name 即为 nodeId）
                _onSelected?.Invoke(item.name);
            }
        }

        /// <summary>
        /// 从项目 Asset 查找所有 DagGraphConfig 配置文件。
        /// 同时保留对当前组件附近 DagGraphConfigurator 的检查，作为局部上下文的补充。
        /// </summary>
        private static List<DagGraph> ResolveGraphs(Component target)
        {
            if (target == null)
            {
                return new List<DagGraph>();
            }

            var graphs = new List<DagGraph>();

            void AddGraph(DagGraphConfig config)
            {
                var graph = config?.DagGraph;
                if (graph != null && graphs.Contains(graph) == false)
                {
                    graphs.Add(graph);
                }
            }

            // 1. 自身（如果是 DagGraphConfigurator）
            if (target is DagGraphConfigurator selfConfigurator && selfConfigurator.GraphConfig != null)
            {
                AddGraph(selfConfigurator.GraphConfig);
            }

            // 2. 同物体上的 DagGraphConfigurator
            var localConfigurator = target.GetComponent<DagGraphConfigurator>();
            if (localConfigurator != null && localConfigurator.GraphConfig != null)
            {
                AddGraph(localConfigurator.GraphConfig);
            }

            // 3. 父级的 DagGraphConfigurator
            var parentConfigurator = target.GetComponentInParent<DagGraphConfigurator>(true);
            if (parentConfigurator != null && parentConfigurator.GraphConfig != null)
            {
                AddGraph(parentConfigurator.GraphConfig);
            }

            // 4. 查找项目中所有 DagGraphConfig 资产（主要来源）
            var guids = AssetDatabase.FindAssets("t:DagGraphConfig");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<DagGraphConfig>(path);
                AddGraph(config);
            }

            return graphs;
        }
    }
}
