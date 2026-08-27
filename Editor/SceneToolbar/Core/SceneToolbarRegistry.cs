using System;
using System.Collections.Generic;
using System.Linq;

namespace Frame.Editor.SceneToolbar
{
    /// <summary>
    /// Scene 工具栏工具注册表。外部通过 Register / Unregister 增删工具。
    /// </summary>
    public static class SceneToolbarRegistry
    {
        static readonly List<ISceneToolbarTool> s_Tools = new List<ISceneToolbarTool>();

        /// <summary>工具列表发生变化时触发，Overlay 据此刷新 UI。</summary>
        public static event Action ToolsChanged;

        /// <summary>当前已注册工具（按 Order 排序）。</summary>
        public static IReadOnlyList<ISceneToolbarTool> Tools =>
            s_Tools.OrderBy(t => t.Order).ThenBy(t => t.Id).ToList();

        /// <summary>注册一个工具。相同 Id 会覆盖旧实例。</summary>
        public static void Register(ISceneToolbarTool tool)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));

            if (string.IsNullOrEmpty(tool.Id))
                throw new ArgumentException("Tool Id 不能为空", nameof(tool));

            s_Tools.RemoveAll(t => t.Id == tool.Id);
            s_Tools.Add(tool);
            ToolsChanged?.Invoke();
        }

        /// <summary>按 Id 注销工具。</summary>
        public static bool Unregister(string id)
        {
            var removed = s_Tools.RemoveAll(t => t.Id == id) > 0;
            if (removed)
                ToolsChanged?.Invoke();
            return removed;
        }

        /// <summary>注销指定工具实例。</summary>
        public static bool Unregister(ISceneToolbarTool tool)
        {
            if (tool == null)
                return false;

            return Unregister(tool.Id);
        }

        /// <summary>是否已注册指定 Id。</summary>
        public static bool Contains(string id)
        {
            return s_Tools.Exists(t => t.Id == id);
        }

        /// <summary>按 Id 获取工具实例。</summary>
        public static bool TryGet(string id, out ISceneToolbarTool tool)
        {
            tool = s_Tools.Find(t => t.Id == id);
            return tool != null;
        }

        /// <summary>清空全部工具（一般仅用于测试）。</summary>
        public static void Clear()
        {
            if (s_Tools.Count == 0)
                return;

            s_Tools.Clear();
            ToolsChanged?.Invoke();
        }
    }
}
