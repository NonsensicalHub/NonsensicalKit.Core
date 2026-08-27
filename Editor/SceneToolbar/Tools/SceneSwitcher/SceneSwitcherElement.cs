using Frame.Editor.SceneToolbar;
using UnityEditor;
using UnityEditor.Toolbars;

namespace Frame.Editor.SceneToolbar.Tools.SceneSwitcher
{
    /// <summary>
    /// 场景切换按钮（Toolbar 元素）。停靠在 Scene 顶栏时可直接点击，无需先展开 Overlay 面板。
    /// </summary>
    [EditorToolbarElement(Id, typeof(SceneView))]
    public sealed class SceneSwitcherElement : SceneToolbarDropdownElement
    {
        public const string Id = "Frame.SceneToolbar/SceneSwitcher";

        protected override string ToolId => SceneSwitcherTool.ToolId;
    }
}
