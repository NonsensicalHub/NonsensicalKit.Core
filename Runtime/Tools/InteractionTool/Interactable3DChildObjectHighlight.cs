using UnityEngine;

namespace NonsensicalKit.Tools.InteractionTool
{
    /// <summary>
    /// 通过开关子物体实现悬停/按下/选中高光，不依赖具体 Shader。
    /// </summary>
    public class Interactable3DChildObjectHighlight : MonoBehaviour, IInteractable3DHighlight
    {
        [SerializeField] private GameObject m_hoverVisual;
        [SerializeField] private GameObject m_pressedVisual;
        [SerializeField] private GameObject m_selectedVisual;

        private void Awake()
        {
            if (m_hoverVisual != null)
            {
                m_hoverVisual.SetActive(false);
            }

            if (m_pressedVisual != null)
            {
                m_pressedVisual.SetActive(false);
            }

            if (m_selectedVisual != null)
            {
                m_selectedVisual.SetActive(false);
            }
        }

        public void ApplyInteractable3DPhase(Interactable3DVisualPhase phase)
        {
            var selected = phase == Interactable3DVisualPhase.Selected ||
                           phase == Interactable3DVisualPhase.SelectedHover ||
                           phase == Interactable3DVisualPhase.SelectedPressed;
            var pressed = phase == Interactable3DVisualPhase.Pressed ||
                          phase == Interactable3DVisualPhase.SelectedPressed;
            var hoverOnly = phase == Interactable3DVisualPhase.Hover || phase == Interactable3DVisualPhase.SelectedHover;

            if (m_hoverVisual != null)
            {
                m_hoverVisual.SetActive(hoverOnly && !pressed);
            }

            if (m_pressedVisual != null)
            {
                m_pressedVisual.SetActive(pressed);
            }

            if (m_selectedVisual != null)
            {
                m_selectedVisual.SetActive(selected);
            }
        }
    }
}
