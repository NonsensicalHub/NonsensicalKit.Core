using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace Frame.Editor.SceneToolbar
{
    /// <summary>
    /// ToolbarOverlay 元素基类：把 UI 按钮绑定到 <see cref="ISceneToolbarTool"/>。
    /// 具体工具放在 <c>Tools/</c>：继承此类 + [EditorToolbarElement] +
    /// 把 Id 加进 <see cref="SceneToolbarOverlay"/> 构造函数，并在 Bootstrap 中 Register。
    /// </summary>
    public abstract class SceneToolbarDropdownElement : EditorToolbarDropdown
    {
        /// <summary>对应 <see cref="ISceneToolbarTool.Id"/>。</summary>
        protected abstract string ToolId { get; }

        protected SceneToolbarDropdownElement()
        {
            BindFromTool();
            clicked += OnElementClicked;
            SceneToolbarRegistry.ToolsChanged += BindFromTool;
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                SceneToolbarRegistry.ToolsChanged -= BindFromTool;
            });
        }

        void BindFromTool()
        {
            if (!SceneToolbarRegistry.TryGet(ToolId, out var tool) || tool == null)
            {
                tooltip = $"未注册工具: {ToolId}";
                text = "?";
                icon = null;
                return;
            }

            tooltip = tool.Tooltip;
            text = string.Empty; // 停靠顶栏时只显示图标，避免文字裁切
            icon = tool.Icon as Texture2D;
        }

        void OnElementClicked()
        {
            if (!SceneToolbarRegistry.TryGet(ToolId, out var tool) || tool == null)
            {
                Debug.LogWarning($"[SceneToolbar] 工具未注册: {ToolId}");
                return;
            }

            // EditorToolbar 元素提供稳定的 worldBound，可用于 GenericMenu.DropDown
            tool.OnClick(worldBound);
        }
    }
}
