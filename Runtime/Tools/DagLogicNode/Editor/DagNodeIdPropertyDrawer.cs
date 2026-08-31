using System;
using System.Collections.Generic;
using System.Linq;
using NonsensicalKit.Core.DagLogicNode;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode.Editor
{
    /// <summary>
    /// 项目内 DagGraph 查找结果缓存。Inspector 每帧绘制多个 [DagNodeId] 时复用，避免反复 FindAssets。
    /// </summary>
    internal static class DagNodeIdLookupCache
    {
        private static List<DagGraph> s_assetGraphs;
        private static Dictionary<string, string> s_idToGraphName;
        private static bool s_dirty = true;
        private static bool s_hooked;

        private static Component s_mergedTarget;
        private static List<DagGraph> s_mergedGraphs;
        private static EventType s_cachedEventType;

        public static void Invalidate()
        {
            s_dirty = true;
            s_mergedTarget = null;
            s_mergedGraphs = null;
        }

        public static List<DagGraph> GetGraphs(Component target)
        {
            EnsureAssetCache();

            var eventType = Event.current != null ? Event.current.type : EventType.Ignore;
            if (s_mergedGraphs != null && s_mergedTarget == target && s_cachedEventType == eventType)
                return s_mergedGraphs;

            s_mergedTarget = target;
            s_cachedEventType = eventType;
            s_mergedGraphs = MergeLocalGraphs(target, s_assetGraphs);
            RebuildLookup(s_mergedGraphs);
            return s_mergedGraphs;
        }

        public static string GetDisplayText(Component target, string currentValue, bool allowEmpty)
        {
            GetGraphs(target);

            if (string.IsNullOrEmpty(currentValue))
                return allowEmpty ? DagNodeIdPropertyDrawer.EmptyLabel : "(Select)";

            if (s_idToGraphName != null && s_idToGraphName.TryGetValue(currentValue, out var graphName))
                return string.IsNullOrEmpty(graphName) ? currentValue : $"[{graphName}] {currentValue}";

            return currentValue + DagNodeIdPropertyDrawer.MissingSuffix;
        }

        private static void EnsureHooks()
        {
            if (s_hooked)
                return;
            s_hooked = true;
            EditorApplication.projectChanged += Invalidate;
        }

        private static void EnsureAssetCache()
        {
            EnsureHooks();
            if (!s_dirty && s_assetGraphs != null)
                return;

            s_assetGraphs = new List<DagGraph>();
            var guids = AssetDatabase.FindAssets("t:DagGraphConfig");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<DagGraphConfig>(path);
                var graph = config?.DagGraph;
                if (graph != null && !s_assetGraphs.Contains(graph))
                    s_assetGraphs.Add(graph);
            }

            s_dirty = false;
            s_mergedTarget = null;
            s_mergedGraphs = null;
        }

        private static void RebuildLookup(List<DagGraph> graphs)
        {
            if (s_idToGraphName == null)
                s_idToGraphName = new Dictionary<string, string>();
            else
                s_idToGraphName.Clear();

            if (graphs == null)
                return;

            foreach (var graph in graphs)
            {
                if (graph?.nodes == null)
                    continue;
                var graphName = string.IsNullOrWhiteSpace(graph.GraphName) ? "Unnamed Graph" : graph.GraphName.Trim();
                foreach (var node in graph.nodes)
                {
                    if (node == null || string.IsNullOrWhiteSpace(node.nodeId))
                        continue;
                    var id = node.nodeId.Trim();
                    if (!s_idToGraphName.ContainsKey(id))
                        s_idToGraphName[id] = graphName;
                }
            }
        }

        private static List<DagGraph> MergeLocalGraphs(Component target, List<DagGraph> assets)
        {
            if (target == null)
                return assets;

            List<DagGraph> merged = null;

            void AddLocal(DagGraphConfig config)
            {
                var graph = config?.DagGraph;
                if (graph == null)
                    return;
                if (assets != null && assets.Contains(graph))
                    return;
                if (merged == null)
                    merged = new List<DagGraph>((assets?.Count ?? 0) + 2) { graph };
                else if (!merged.Contains(graph))
                    merged.Insert(0, graph);
            }

            if (target is DagGraphConfigurator selfConfigurator)
                AddLocal(selfConfigurator.GraphConfig);

            var local = target.GetComponent<DagGraphConfigurator>();
            if (local != null)
                AddLocal(local.GraphConfig);

            var parent = target.GetComponentInParent<DagGraphConfigurator>(true);
            if (parent != null)
                AddLocal(parent.GraphConfig);

            if (merged == null)
                return assets ?? new List<DagGraph>();

            if (assets != null)
            {
                foreach (var g in assets)
                {
                    if (!merged.Contains(g))
                        merged.Add(g);
                }
            }

            return merged;
        }
    }

    [CustomPropertyDrawer(typeof(DagNodeIdAttribute))]
    public class DagNodeIdPropertyDrawer : PropertyDrawer
    {
        internal const string EmptyLabel = "(None)";
        internal const string MissingSuffix = " (Missing)";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "DagNodeIdAttribute 仅支持 string 字段");
                return;
            }

            var target = property.serializedObject.targetObject as Component;
            var graphs = DagNodeIdLookupCache.GetGraphs(target);
            if (graphs == null || graphs.Count == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var attr = (DagNodeIdAttribute)attribute;
            string currentValue = property.stringValue ?? string.Empty;
            var display = new GUIContent(DagNodeIdLookupCache.GetDisplayText(target, currentValue, attr.AllowEmpty));

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            bool isPressed = EditorGUI.DropdownButton(fieldRect, display, FocusType.Passive);

            if (isPressed)
            {
                DagNodeIdLookupCache.Invalidate();
                graphs = DagNodeIdLookupCache.GetGraphs(target);
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
                        continue;

                    string graphName = string.IsNullOrWhiteSpace(graph.GraphName) ? "Unnamed Graph" : graph.GraphName.Trim();

                    AdvancedDropdownItem groupItem;
                    if (hasMultipleGraphs)
                    {
                        groupItem = new AdvancedDropdownItem(graphName) { id = itemId++ };
                        root.AddChild(groupItem);
                    }
                    else
                    {
                        groupItem = root;
                    }

                    var sortedNodes = graph.nodes
                        .Where(n => n != null && !string.IsNullOrWhiteSpace(n.nodeId))
                        .OrderBy(n => n.nodeId);

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

                _onSelected?.Invoke(item.name);
            }
        }
    }
}
