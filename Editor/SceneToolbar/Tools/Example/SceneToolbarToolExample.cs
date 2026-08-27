using Frame.Editor.SceneToolbar;
using UnityEditor;
using UnityEngine;

namespace Frame.Editor.SceneToolbar.Tools.Example
{
    /// <summary>
    /// 扩展示例（默认未启用）。启用需三步：
    /// 1. Register 本工具（取消底部 Registrar 注释，或写入 Core/SceneToolbarBootstrap）
    /// 2. 取消 SceneToolbarExampleElement 的注释
    /// 3. 把 SceneToolbarExampleElement.Id 加入 Core/SceneToolbarOverlay 的 base(...)
    /// </summary>
    public sealed class SceneToolbarToolExample : ISceneToolbarTool
    {
        public const string ToolId = "frame.scene-toolbar.example";

        public string Id => ToolId;
        public string Tooltip => "示例工具（可删除）";
        public int Order => 100;

        public Texture Icon
        {
            get
            {
                var content = EditorGUIUtility.IconContent("d_Settings Icon");
                return content != null ? content.image : null;
            }
        }

        public void OnClick(Rect buttonScreenRect)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("示例菜单项 A"), false, () => Debug.Log("示例 A"));
            menu.AddItem(new GUIContent("示例菜单项 B"), false, () => Debug.Log("示例 B"));
            if (buttonScreenRect.width > 0f || buttonScreenRect.height > 0f)
                menu.DropDown(buttonScreenRect);
            else
                menu.ShowAsContext();
        }

        // [InitializeOnLoad]
        // static class Registrar
        // {
        //     static Registrar() =>
        //         EditorApplication.delayCall += () =>
        //             SceneToolbarRegistry.Register(new SceneToolbarToolExample());
        // }
    }

    // 启用时取消注释，并加入 Core/SceneToolbarOverlay 构造函数：
    // [EditorToolbarElement(Id, typeof(SceneView))]
    // public sealed class SceneToolbarExampleElement : SceneToolbarDropdownElement
    // {
    //     public const string Id = "Frame.SceneToolbar/Example";
    //     protected override string ToolId => SceneToolbarToolExample.ToolId;
    // }
}
