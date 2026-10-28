namespace NonsensicalKit.Setting
{
    /// <summary>
    /// 设置面板视图抽象。由版本工厂选择 IMGUI 或 UI Toolkit 实现。
    /// </summary>
    public interface ISettingPanelView
    {
        void Bind(SettingManager manager);
        void SetVisible(bool visible);
        void ToggleVisible();
        string RendererName { get; }
    }
}
