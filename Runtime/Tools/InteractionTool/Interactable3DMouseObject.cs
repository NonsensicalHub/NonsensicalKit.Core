using UnityEngine;
using UnityEngine.Events;

namespace NonsensicalKit.Tools.InteractionTool
{
    /// <summary>
    /// 可交互 3D 物体：向 <see cref="Interactable3DWorldPointer"/> 注册，由 Hub 统一射线与选中互斥。
    /// 射线参数（相机、Layer、距离等）在 Hub 上配置；本组件只负责事件与高光。
    /// </summary>
    public class Interactable3DMouseObject : MonoBehaviour
    {
        [Tooltip("为 true 时，在本物体上完成一次有效点击后会成为全局唯一选中，并取消上一选中。")]
        [SerializeField] private bool m_hubManagesSelection = true;

        [SerializeField] private UnityEvent m_onHoverEnter;

        [SerializeField] private UnityEvent m_onHoverExit;

        [SerializeField] private UnityEvent m_onClick;

        [SerializeField] private UnityEvent m_onSelected;

        [SerializeField] private UnityEvent m_onDeselected;

        private IInteractable3DHighlight[] _highlights = System.Array.Empty<IInteractable3DHighlight>();
        private Interactable3DVisualPhase _lastAppliedPhase = (Interactable3DVisualPhase)(-1);
        private bool _lastSelectionNotified;

        public bool HubManagesSelection => m_hubManagesSelection;

        /// <summary>当前帧指针是否在本物体上（Hub 射线最近命中且未被 UI 遮挡）。</summary>
        public bool IsPointerRayOverSelf { get; private set; }

        public bool IsHovered { get; private set; }

        /// <summary>是否为 Hub 维护的当前全局选中。</summary>
        public bool IsSelectedByHub =>
            Interactable3DWorldPointer.Instance != null &&
            Interactable3DWorldPointer.Instance.SelectedTarget == this;

        private void Awake()
        {
            RefreshHighlightTargets();
        }

        private void OnEnable()
        {
            var hub = Interactable3DWorldPointer.Instance;
            if (hub != null)
            {
                hub.Register(this);
            }
        }

        private void OnDisable()
        {
            var hub = Interactable3DWorldPointer.Instance;
            if (hub != null)
            {
                hub.Unregister(this);
            }
            else
            {
                if (_lastSelectionNotified)
                {
                    _lastSelectionNotified = false;
                    m_onDeselected?.Invoke();
                }

                if (IsHovered)
                {
                    IsHovered = false;
                    m_onHoverExit?.Invoke();
                }
            }

            IsPointerRayOverSelf = false;
            ApplyVisualPhase(Interactable3DVisualPhase.Normal);
        }

        /// <summary>运行时动态添加高光实现类后调用。</summary>
        public void RefreshHighlightTargets()
        {
            _highlights = GetComponentsInChildren<IInteractable3DHighlight>(true);
        }

        internal void NotifyHubHoverEdge(bool entered)
        {
            if (entered == IsHovered)
            {
                return;
            }

            if (entered)
            {
                IsHovered = true;
                m_onHoverEnter?.Invoke();
            }
            else
            {
                IsHovered = false;
                m_onHoverExit?.Invoke();
            }
        }

        internal void NotifyHubClicked()
        {
            m_onClick?.Invoke();
        }

        internal void NotifyHubSelectionBecame(bool selected)
        {
            if (selected == _lastSelectionNotified)
            {
                return;
            }

            _lastSelectionNotified = selected;
            if (selected)
            {
                m_onSelected?.Invoke();
            }
            else
            {
                m_onDeselected?.Invoke();
            }
        }

        internal void SyncVisualFromHub()
        {
            var hub = Interactable3DWorldPointer.Instance;
            if (hub == null || !isActiveAndEnabled)
            {
                IsPointerRayOverSelf = false;
                ApplyVisualPhase(Interactable3DVisualPhase.Normal);
                return;
            }

            var over = hub.HoverTarget == this;
            var sel = hub.SelectedTarget == this;
            var press = hub.GetPressContext(this);
            IsPointerRayOverSelf = over;

            var phase = ResolvePhase(over, sel, press);
            ApplyVisualPhase(phase);
        }

        private static Interactable3DVisualPhase ResolvePhase(bool pointerOver, bool selected, bool press)
        {
            if (!selected && !pointerOver)
            {
                return Interactable3DVisualPhase.Normal;
            }

            if (!selected && pointerOver && !press)
            {
                return Interactable3DVisualPhase.Hover;
            }

            if (!selected && pointerOver && press)
            {
                return Interactable3DVisualPhase.Pressed;
            }

            if (selected && !pointerOver)
            {
                return Interactable3DVisualPhase.Selected;
            }

            if (selected && pointerOver && !press)
            {
                return Interactable3DVisualPhase.SelectedHover;
            }

            return Interactable3DVisualPhase.SelectedPressed;
        }

        private void ApplyVisualPhase(Interactable3DVisualPhase phase)
        {
            if (phase == _lastAppliedPhase)
            {
                return;
            }

            _lastAppliedPhase = phase;
            for (var i = 0; i < _highlights.Length; i++)
            {
                var h = _highlights[i];
                if (h != null)
                {
                    h.ApplyInteractable3DPhase(phase);
                }
            }
        }
    }
}
