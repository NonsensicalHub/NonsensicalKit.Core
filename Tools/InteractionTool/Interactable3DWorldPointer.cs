using System.Collections.Generic;
using NonsensicalKit.Core;
using NonsensicalKit.Tools.InputTool;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace NonsensicalKit.Tools.InteractionTool
{
    /// <summary>
    /// 场景级 3D 指针：每帧至多一次 <see cref="Physics.RaycastNonAlloc"/>，维护悬停与可选的全局单选。
    /// 各物体挂 <see cref="Interactable3DMouseObject"/> 并向本 Hub 注册即可。
    /// </summary>
    public class Interactable3DWorldPointer : MonoSingleton<Interactable3DWorldPointer>
    {
        [SerializeField] private Camera m_rayCamera;

        [SerializeField] private LayerMask m_layerMask = Physics.DefaultRaycastLayers;

        [SerializeField] private float m_maxDistance = 500f;

        [SerializeField] private QueryTriggerInteraction m_queryTriggers = QueryTriggerInteraction.Ignore;

        [Tooltip("为 true 时，若 EventSystem 认为指针在 UI 上，则不进行世界射线。")]
        [SerializeField] private bool m_blockWhenPointerOverUI = true;

        [Tooltip("0=左键，1=右键，2=中键。")]
        [SerializeField] [Range(0, 2)]
        private int m_mouseButton;

        [Tooltip("在空白处按下指针时清除当前选中。")]
        [SerializeField] private bool m_clearSelectionOnBackgroundPointerDown = true;

        private const int RaycastBufferInitial = 24;

        private readonly HashSet<Interactable3DMouseObject> _registry = new();

        private EventSystem _eventSystem;
        private Interactable3DMouseObject _hoverTarget;
        private Interactable3DMouseObject _selected;
        private Interactable3DMouseObject _pressBeganOn;
        private RaycastHit[] _raycastHits = new RaycastHit[RaycastBufferInitial];

        public Interactable3DMouseObject HoverTarget => _hoverTarget;

        public Interactable3DMouseObject SelectedTarget => _selected;

        public void Register(Interactable3DMouseObject target)
        {
            if (target != null)
            {
                _registry.Add(target);
            }
        }

        public void Unregister(Interactable3DMouseObject target)
        {
            if (target == null)
            {
                return;
            }

            _registry.Remove(target);
            if (_hoverTarget == target)
            {
                _hoverTarget = null;
                target.NotifyHubHoverEdge(false);
            }

            if (_pressBeganOn == target)
            {
                _pressBeganOn = null;
            }

            if (_selected == target)
            {
                _selected = null;
                target.NotifyHubSelectionBecame(false);
            }
        }

        public void ClearSelection()
        {
            if (_selected == null)
            {
                return;
            }

            var prev = _selected;
            _selected = null;
            prev.NotifyHubSelectionBecame(false);
            prev.SyncVisualFromHub();
            _hoverTarget?.SyncVisualFromHub();
        }

        protected override void Awake()
        {
            base.Awake();
            _eventSystem = EventSystem.current;
        }

        private void Start()
        {
            if (m_rayCamera == null)
            {
                m_rayCamera = Camera.main;
            }
        }

        private void LateUpdate()
        {
            if (_eventSystem == null && EventSystem.current != null)
            {
                _eventSystem = EventSystem.current;
            }

            if (m_rayCamera == null)
            {
                m_rayCamera = Camera.main;
            }

            var pointerBlocked = IsPointerBlockedByUi();
            Interactable3DMouseObject underPointer = null;

            if (m_rayCamera != null && !pointerBlocked)
            {
                var screen = GetPointerScreenPosition();
                var ray = m_rayCamera.ScreenPointToRay(screen);
                underPointer = ResolveInteractableUnderRay(ray);
            }
            
            SetHoverTarget(underPointer, pointerBlocked);

            var btnDown = GetMouseButtonDownSafe(m_mouseButton);
            var btnUp = GetMouseButtonUpSafe(m_mouseButton);

            if (pointerBlocked)
            {
                _pressBeganOn = null;
            }
            else if (btnDown)
            {
                if (underPointer == null && m_clearSelectionOnBackgroundPointerDown)
                {
                    ClearSelection();
                }

                _pressBeganOn = underPointer;
            }

            if (btnUp && !pointerBlocked && _pressBeganOn != null && underPointer == _pressBeganOn)
            {
                var clicked = _pressBeganOn;
                clicked.NotifyHubClicked();
                TryApplyExclusiveSelection(clicked);
            }

            if (btnUp)
            {
                _pressBeganOn = null;
            }

            _hoverTarget?.SyncVisualFromHub();
            if (_selected != null && _selected != _hoverTarget)
            {
                _selected.SyncVisualFromHub();
            }
        }

        internal bool GetPressContext(Interactable3DMouseObject o)
        {
            if (o == null || IsPointerBlockedByUi())
            {
                return false;
            }

            return _pressBeganOn == o && _hoverTarget == o && GetMouseButtonSafe(m_mouseButton);
        }

        private void TryApplyExclusiveSelection(Interactable3DMouseObject newSelection)
        {
            if (newSelection == null || !newSelection.HubManagesSelection)
            {
                return;
            }

            if (_selected == newSelection)
            {
                return;
            }

            var prev = _selected;
            _selected = newSelection;
            prev?.NotifyHubSelectionBecame(false);
            newSelection.NotifyHubSelectionBecame(true);
            prev?.SyncVisualFromHub();
            newSelection.SyncVisualFromHub();
        }

        private void SetHoverTarget(Interactable3DMouseObject next, bool pointerBlocked)
        {
            if (_hoverTarget == next)
            {
                return;
            }

            var prev = _hoverTarget;
            _hoverTarget = next;
            if (pointerBlocked)
            {
                _pressBeganOn = null;
            }

            prev?.NotifyHubHoverEdge(false);
            next?.NotifyHubHoverEdge(true);
            prev?.SyncVisualFromHub();
            next?.SyncVisualFromHub();
        }

        private Interactable3DMouseObject ResolveInteractableUnderRay(Ray ray)
        {
            int count;
            while (true)
            {
                count = Physics.RaycastNonAlloc(ray, _raycastHits, m_maxDistance, m_layerMask, m_queryTriggers);
                if (count < _raycastHits.Length)
                {
                    break;
                }

                System.Array.Resize(ref _raycastHits, _raycastHits.Length * 2);
            }

            if (count <= 0)
            {
                return null;
            }

            var bestDist = float.MaxValue;
            Interactable3DMouseObject best = null;
            for (var i = 0; i < count; i++)
            {
                var h = _raycastHits[i];
                if (h.collider == null)
                {
                    continue;
                }

                var t = h.collider.GetComponentInParent<Interactable3DMouseObject>();
                if (t == null || !t.isActiveAndEnabled || !_registry.Contains(t))
                {
                    continue;
                }

                if (h.distance < bestDist)
                {
                    bestDist = h.distance;
                    best = t;
                }
            }

            return best;
        }

        private Vector3 GetPointerScreenPosition()
        {
            if (PlatformInfo.IsMobile && Input.touchCount > 0)
            {
                return Input.GetTouch(0).position;
            }

            if (InputHub.Instance != null)
            {
                return InputHub.Instance.CrtMousePos;
            }

            return Input.mousePosition;
        }

        private bool IsPointerBlockedByUi()
        {
            if (!m_blockWhenPointerOverUI)
            {
                return false;
            }

            if (_eventSystem == null)
            {
                return false;
            }

            if (PlatformInfo.IsMobile && Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                return _eventSystem.IsPointerOverGameObject(touch.fingerId);
            }

#if ENABLE_INPUT_SYSTEM
            if (_eventSystem.currentInputModule is InputSystemUIInputModule systemUi && Pointer.current != null)
            {
                return systemUi.GetLastRaycastResult(Pointer.current.deviceId).isValid;
            }
#endif
            return _eventSystem.IsPointerOverGameObject();
        }

        private static bool GetMouseButtonSafe(int index)
        {
            return index switch
            {
                0 => Input.GetMouseButton(0),
                1 => Input.GetMouseButton(1),
                2 => Input.GetMouseButton(2),
                _ => Input.GetMouseButton(0),
            };
        }

        private static bool GetMouseButtonDownSafe(int index)
        {
            return index switch
            {
                0 => Input.GetMouseButtonDown(0),
                1 => Input.GetMouseButtonDown(1),
                2 => Input.GetMouseButtonDown(2),
                _ => Input.GetMouseButtonDown(0),
            };
        }

        private static bool GetMouseButtonUpSafe(int index)
        {
            return index switch
            {
                0 => Input.GetMouseButtonUp(0),
                1 => Input.GetMouseButtonUp(1),
                2 => Input.GetMouseButtonUp(2),
                _ => Input.GetMouseButtonUp(0),
            };
        }
    }
}
