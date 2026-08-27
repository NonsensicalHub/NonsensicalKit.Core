using Frame.Editor.SceneToolbar.Tools.SceneSwitcher;
using UnityEditor;

namespace Frame.Editor.SceneToolbar
{
    /// <summary>
    /// 编辑器启动时注册内置工具逻辑。
    /// 具体工具实现见 <c>Tools/</c> 目录；此处只负责 Register。
    /// </summary>
    [InitializeOnLoad]
    static class SceneToolbarBootstrap
    {
        static SceneToolbarBootstrap()
        {
            // 延迟一帧，避免域重载时 Overlay 尚未就绪
            EditorApplication.delayCall += RegisterBuiltInTools;
        }

        static void RegisterBuiltInTools()
        {
            SceneToolbarRegistry.Register(new SceneSwitcherTool());
            // 新工具在此追加 Register，例如：
            // SceneToolbarRegistry.Register(new MyTool());
        }
    }
}
