using Frame.Editor.SceneToolbar.Tools.SceneSwitcher;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace Frame.Editor.SceneToolbar
{
    /// <summary>
    /// Scene 视图工具栏 Overlay（ToolbarOverlay）。
    /// 水平/垂直停靠时，各工具图标直接显示在工具条上，可一点即用。
    /// </summary>
    /// <remarks>
    /// Unity 限制：ToolbarOverlay 的元素 Id 必须在构造时静态声明。
    /// 新增工具步骤见根目录 HANDOFF.md「扩展新工具」。
    /// 具体工具代码放在 <c>Tools/&lt;ToolName&gt;/</c>，此处只挂元素 Id。
    /// </remarks>
    [Overlay(typeof(SceneView), "项目工具", true)]
    [UnityEngine.Icon("d_SceneAsset Icon")]
    public sealed class SceneToolbarOverlay : ToolbarOverlay
    {
        SceneToolbarOverlay() : base(
            SceneSwitcherElement.Id
            // 新工具 Element.Id 写在这里，例如：
            // , MyToolElement.Id
        )
        {
        }
    }
}
