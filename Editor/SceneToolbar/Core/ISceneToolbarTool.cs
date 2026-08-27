using UnityEngine;

namespace Frame.Editor.SceneToolbar
{
    /// <summary>
    /// Scene 视图浮动工具栏中的单个工具接口。
    /// 实现此接口并调用 <see cref="SceneToolbarRegistry.Register"/> 即可扩展工具栏。
    /// </summary>
    public interface ISceneToolbarTool
    {
        /// <summary>唯一标识，用于注册与注销。</summary>
        string Id { get; }

        /// <summary>鼠标悬停提示。</summary>
        string Tooltip { get; }

        /// <summary>工具栏按钮图标。</summary>
        Texture Icon { get; }

        /// <summary>显示顺序，数值越小越靠前。</summary>
        int Order { get; }

        /// <summary>
        /// 点击按钮时回调。
        /// </summary>
        /// <param name="buttonScreenRect">按钮在屏幕空间的矩形，可用于定位二级菜单。</param>
        void OnClick(Rect buttonScreenRect);
    }
}
