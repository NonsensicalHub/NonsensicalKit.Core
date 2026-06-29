namespace NonsensicalKit.Tools.InteractionTool
{
    /// <summary>
    /// 3D 鼠标/触摸交互时的高光（或轮廓、缩放等）表现阶段。
    /// </summary>
    public enum Interactable3DVisualPhase
    {
        Normal,
        Hover,
        Pressed,
        /// <summary>全局选中且指针不在本物体上。</summary>
        Selected,
        /// <summary>全局选中且指针悬停在本物体上。</summary>
        SelectedHover,
        /// <summary>全局选中且在本物体上按下主键尚未松开。</summary>
        SelectedPressed,
    }

    /// <summary>
    /// 由 <see cref="Interactable3DWorldPointer"/> 与 <see cref="Interactable3DMouseObject"/> 驱动；在同一物体或子物体上实现即可接入高光逻辑。
    /// </summary>
    public interface IInteractable3DHighlight
    {
        void ApplyInteractable3DPhase(Interactable3DVisualPhase phase);
    }
}
