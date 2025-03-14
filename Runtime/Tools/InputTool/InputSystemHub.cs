#if ENABLE_INPUT_SYSTEM
using System;
using System.Collections;
using System.Collections.Generic;
using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Tools.InputTool
{
    public partial class InputHub
    {
        private InputHubControls _controls;

        protected override void Awake()
        {
            base.Awake();
            InitInputSystem();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_controls != null)
            {
                _controls.Disable();
                _controls.Dispose();
            }
        }

        private void InitInputSystem()
        {
            _controls = new InputHubControls();
            _controls.Enable();

            _controls.KeyAndMouse.mousePos.started += MousePosChanged;
            _controls.KeyAndMouse.mousePos.performed += MousePosChanged;
            _controls.KeyAndMouse.mousePos.canceled += MousePosChanged;
            //_controls.KeyAndMouse.mouseMove.started += MouseMoveChanged;
            _controls.KeyAndMouse.mouseMove.performed += MouseMoveChanged;
            _controls.KeyAndMouse.mouseMove.canceled += MouseMoveChanged;
            _controls.KeyAndMouse.zoom.started += ZoomChanged;
            _controls.KeyAndMouse.zoom.performed += ZoomChanged;
            _controls.KeyAndMouse.zoom.canceled += ZoomChanged;

            _controls.KeyAndMouse.mouseLeft.started += MouseLeftButtonDown;
            _controls.KeyAndMouse.mouseLeft.canceled += MouseLeftButtonUp;
            _controls.KeyAndMouse.mouseRight.started += MouseRightButtonDown;
            _controls.KeyAndMouse.mouseRight.canceled += MouseRightButtonUp;
            _controls.KeyAndMouse.mouseMiddle.performed += MouseMiddleButtonDown;
            _controls.KeyAndMouse.mouseMiddle.canceled += MouseMiddleButtonUp;

            _controls.KeyAndMouse.move.started += MoveChanged;
            _controls.KeyAndMouse.move.performed += MoveChanged;
            _controls.KeyAndMouse.move.canceled += MoveChanged;

            _controls.KeyAndMouse.fKey.performed += FKeyEnter;
            _controls.KeyAndMouse.spaceKey.performed += SpaceKeyEnter;
            _controls.KeyAndMouse.leftShift.started += LeftShiftKeyEnter;
            _controls.KeyAndMouse.leftShift.canceled += LeftShiftKeyLeave;
            _controls.KeyAndMouse.leftAlt.started += LeftAltKeyEnter;
            _controls.KeyAndMouse.leftAlt.canceled += LeftAltKeyLeave;
        }

        private void MousePosChanged(InputAction.CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();
            CrtMousePos = value;
            OnMousePosChanged?.Invoke(value);
        }

        private void MouseMoveChanged(InputAction.CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();
            CrtMouseMove = value;
            OnMouseMoveChanged?.Invoke(value);
        }

        private void ZoomChanged(InputAction.CallbackContext context)
        {
            float value = context.ReadValue<float>();
            CrtZoom = value;
            OnZoomChanged?.Invoke(value);
        }

        private void MouseLeftButtonDown(InputAction.CallbackContext context)
        {
            IsMouseLeftButtonHold = true;
            OnMouseLeftButtonDown?.Invoke();
        }

        private void MouseLeftButtonUp(InputAction.CallbackContext context)
        {
            IsMouseLeftButtonHold = false;
            OnMouseLeftButtonUp?.Invoke();
        }

        private void MouseRightButtonDown(InputAction.CallbackContext context)
        {
            IsMouseRightButtonHold = true;
            OnMouseRightButtonDown?.Invoke();
        }

        private void MouseRightButtonUp(InputAction.CallbackContext context)
        {
            IsMouseRightButtonHold = false;
            OnMouseRightButtonUp?.Invoke();
        }

        private void MouseMiddleButtonDown(InputAction.CallbackContext context)
        {
            IsMouseMiddleButtonHold = true;
            OnMouseMiddleButtonDown?.Invoke();
        }

        private void MouseMiddleButtonUp(InputAction.CallbackContext context)
        {
            IsMouseMiddleButtonHold = false;
            OnMouseMiddleButtonUp?.Invoke();
        }

        private void MoveChanged(InputAction.CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();
            CrtMove = value;
            OnMoveChanged?.Invoke(value);
        }

        private void SpaceKeyEnter(InputAction.CallbackContext context)
        {
            OnSpaceKeyEnter?.Invoke();
        }

        private void FKeyEnter(InputAction.CallbackContext context)
        {
            OnFKeyEnter?.Invoke();
        }

        private void LeftShiftKeyEnter(InputAction.CallbackContext context)
        {
            OnLeftShiftKeyChanged?.Invoke(true);
            IsLeftShiftKeyHold = true;
        }

        private void LeftShiftKeyLeave(InputAction.CallbackContext context)
        {
            OnLeftShiftKeyChanged?.Invoke(false);
            IsLeftShiftKeyHold = false;
        }

        private void LeftAltKeyEnter(InputAction.CallbackContext context)
        {
            OnLeftAltKeyChanged?.Invoke(true);
            IsLeftAltKeyHold = true;
        }

        private void LeftAltKeyLeave(InputAction.CallbackContext context)
        {
            OnLeftAltKeyChanged?.Invoke(false);
            IsLeftAltKeyHold = false;
        }
    }

    public partial class @InputHubControls : IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }

        public @InputHubControls()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputHubTest"",
    ""maps"": [
        {
            ""name"": ""KeyAndMouse"",
            ""id"": ""5a4ad185-fcb7-4cde-9ba8-4df85fa3ab4d"",
            ""actions"": [
                {
                    ""name"": ""move"",
                    ""type"": ""Value"",
                    ""id"": ""06a8b5f6-46c3-4bbe-b421-55fbf5758519"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""mousePos"",
                    ""type"": ""Value"",
                    ""id"": ""aa520eb0-38b3-417d-baff-8f95d50129a5"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""zoom"",
                    ""type"": ""Value"",
                    ""id"": ""101305f5-7525-489d-9eb2-fc3661f31d3e"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""mouseMove"",
                    ""type"": ""Value"",
                    ""id"": ""136d7e53-3b41-45c3-9d2d-cb5227ec70ef"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""mouseLeft"",
                    ""type"": ""Button"",
                    ""id"": ""1598cd6d-0424-46c3-abbe-133f35fa14c7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""mouseRight"",
                    ""type"": ""Button"",
                    ""id"": ""c7d877ff-e6c9-41f4-9f96-f8d7834a8f42"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""mouseMiddle"",
                    ""type"": ""Button"",
                    ""id"": ""eb9942c6-458c-4505-873f-51ddc2978c4f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""leftShift"",
                    ""type"": ""Button"",
                    ""id"": ""6f46153b-01c8-4284-b3f0-9aebe175eec4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""spaceKey"",
                    ""type"": ""Button"",
                    ""id"": ""5a3756cf-f856-4717-b3be-4694d7a0f242"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""fKey"",
                    ""type"": ""Button"",
                    ""id"": ""4a2dcf8c-88fc-4b9f-b6c8-0f81247142c8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""leftAlt"",
                    ""type"": ""Button"",
                    ""id"": ""186f7ee0-46c8-4d6f-9282-fb2d254d9bf8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""959fb853-aa12-41cd-9a15-39af848f8841"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f0098c73-2ac1-4cd7-bd31-045c2e49b55b"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""mouseLeft"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d9c859ea-34dc-43fc-9ed2-3823d5a01824"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""mouseRight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""797b8376-46c4-4821-8e8c-b831f9933a24"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""mouseMiddle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dad2def7-7245-4d60-8ee4-0d128765792f"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""mousePos"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""19d8dc98-70d0-43be-a1a9-ea58d4b34f28"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""spaceKey"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f14e9521-31f5-4814-8b4f-142f115646e8"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""leftShift"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""aff7a8a3-333a-4c33-b048-ad12ac2f030f"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""52d9cb80-4596-4f3d-b261-c1523d365651"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""7d318085-ba9f-495e-89d6-8de0f0ccf69d"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""3d1a74b2-73b9-4acf-975b-550cb3e5a99c"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""e63d7228-185d-46a4-9713-7c14fee22a9c"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""f031fee9-c5c8-478e-91a6-210828b8141b"",
                    ""path"": ""<Pointer>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""mouseMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""40343153-ece4-4649-99c5-98ed013506dc"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""fKey"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""76e4c366-f499-4c9d-a326-1af1cf025581"",
                    ""path"": ""<Keyboard>/leftAlt"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""leftAlt"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // KeyAndMouse
            m_KeyAndMouse = asset.FindActionMap("KeyAndMouse", throwIfNotFound: true);
            m_KeyAndMouse_move = m_KeyAndMouse.FindAction("move", throwIfNotFound: true);
            m_KeyAndMouse_mousePos = m_KeyAndMouse.FindAction("mousePos", throwIfNotFound: true);
            m_KeyAndMouse_zoom = m_KeyAndMouse.FindAction("zoom", throwIfNotFound: true);
            m_KeyAndMouse_mouseMove = m_KeyAndMouse.FindAction("mouseMove", throwIfNotFound: true);
            m_KeyAndMouse_mouseLeft = m_KeyAndMouse.FindAction("mouseLeft", throwIfNotFound: true);
            m_KeyAndMouse_mouseRight = m_KeyAndMouse.FindAction("mouseRight", throwIfNotFound: true);
            m_KeyAndMouse_mouseMiddle = m_KeyAndMouse.FindAction("mouseMiddle", throwIfNotFound: true);
            m_KeyAndMouse_leftShift = m_KeyAndMouse.FindAction("leftShift", throwIfNotFound: true);
            m_KeyAndMouse_spaceKey = m_KeyAndMouse.FindAction("spaceKey", throwIfNotFound: true);
            m_KeyAndMouse_fKey = m_KeyAndMouse.FindAction("fKey", throwIfNotFound: true);
            m_KeyAndMouse_leftAlt = m_KeyAndMouse.FindAction("leftAlt", throwIfNotFound: true);
        }

        public void Dispose()
        {
            Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // KeyAndMouse
        private readonly InputActionMap m_KeyAndMouse;
        private IKeyAndMouseActions m_KeyAndMouseActionsCallbackInterface;
        private readonly InputAction m_KeyAndMouse_move;
        private readonly InputAction m_KeyAndMouse_mousePos;
        private readonly InputAction m_KeyAndMouse_zoom;
        private readonly InputAction m_KeyAndMouse_mouseMove;
        private readonly InputAction m_KeyAndMouse_mouseLeft;
        private readonly InputAction m_KeyAndMouse_mouseRight;
        private readonly InputAction m_KeyAndMouse_mouseMiddle;
        private readonly InputAction m_KeyAndMouse_leftShift;
        private readonly InputAction m_KeyAndMouse_spaceKey;
        private readonly InputAction m_KeyAndMouse_fKey;
        private readonly InputAction m_KeyAndMouse_leftAlt;

        public struct KeyAndMouseActions
        {
            private @InputHubControls m_Wrapper;
            public KeyAndMouseActions(@InputHubControls wrapper) { m_Wrapper = wrapper; }
            public InputAction @move => m_Wrapper.m_KeyAndMouse_move;
            public InputAction @mousePos => m_Wrapper.m_KeyAndMouse_mousePos;
            public InputAction @zoom => m_Wrapper.m_KeyAndMouse_zoom;
            public InputAction @mouseMove => m_Wrapper.m_KeyAndMouse_mouseMove;
            public InputAction @mouseLeft => m_Wrapper.m_KeyAndMouse_mouseLeft;
            public InputAction @mouseRight => m_Wrapper.m_KeyAndMouse_mouseRight;
            public InputAction @mouseMiddle => m_Wrapper.m_KeyAndMouse_mouseMiddle;
            public InputAction @leftShift => m_Wrapper.m_KeyAndMouse_leftShift;
            public InputAction @spaceKey => m_Wrapper.m_KeyAndMouse_spaceKey;
            public InputAction @fKey => m_Wrapper.m_KeyAndMouse_fKey;
            public InputAction @leftAlt => m_Wrapper.m_KeyAndMouse_leftAlt;
            public InputActionMap Get() { return m_Wrapper.m_KeyAndMouse; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(KeyAndMouseActions set) { return set.Get(); }

            public void SetCallbacks(IKeyAndMouseActions instance)
            {
                if (m_Wrapper.m_KeyAndMouseActionsCallbackInterface != null)
                {
                    @move.started -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMove;
                    @move.performed -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMove;
                    @move.canceled -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMove;
                    @mousePos.started -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMousePos;
                    @mousePos.performed -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMousePos;
                    @mousePos.canceled -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMousePos;
                    @zoom.started -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnZoom;
                    @zoom.performed -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnZoom;
                    @zoom.canceled -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnZoom;
                    @mouseMove.started -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMouseMove;
                    @mouseMove.performed -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMouseMove;
                    @mouseMove.canceled -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMouseMove;
                    @mouseLeft.started -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMouseLeft;
                    @mouseLeft.performed -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMouseLeft;
                    @mouseLeft.canceled -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMouseLeft;
                    @mouseRight.started -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMouseRight;
                    @mouseRight.performed -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMouseRight;
                    @mouseRight.canceled -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMouseRight;
                    @mouseMiddle.started -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMouseMiddle;
                    @mouseMiddle.performed -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMouseMiddle;
                    @mouseMiddle.canceled -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnMouseMiddle;
                    @leftShift.started -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnLeftShift;
                    @leftShift.performed -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnLeftShift;
                    @leftShift.canceled -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnLeftShift;
                    @spaceKey.started -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnSpaceKey;
                    @spaceKey.performed -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnSpaceKey;
                    @spaceKey.canceled -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnSpaceKey;
                    @fKey.started -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnFKey;
                    @fKey.performed -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnFKey;
                    @fKey.canceled -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnFKey;
                    @leftAlt.started -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnLeftAlt;
                    @leftAlt.performed -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnLeftAlt;
                    @leftAlt.canceled -= m_Wrapper.m_KeyAndMouseActionsCallbackInterface.OnLeftAlt;
                }

                m_Wrapper.m_KeyAndMouseActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @move.started += instance.OnMove;
                    @move.performed += instance.OnMove;
                    @move.canceled += instance.OnMove;
                    @mousePos.started += instance.OnMousePos;
                    @mousePos.performed += instance.OnMousePos;
                    @mousePos.canceled += instance.OnMousePos;
                    @zoom.started += instance.OnZoom;
                    @zoom.performed += instance.OnZoom;
                    @zoom.canceled += instance.OnZoom;
                    @mouseMove.started += instance.OnMouseMove;
                    @mouseMove.performed += instance.OnMouseMove;
                    @mouseMove.canceled += instance.OnMouseMove;
                    @mouseLeft.started += instance.OnMouseLeft;
                    @mouseLeft.performed += instance.OnMouseLeft;
                    @mouseLeft.canceled += instance.OnMouseLeft;
                    @mouseRight.started += instance.OnMouseRight;
                    @mouseRight.performed += instance.OnMouseRight;
                    @mouseRight.canceled += instance.OnMouseRight;
                    @mouseMiddle.started += instance.OnMouseMiddle;
                    @mouseMiddle.performed += instance.OnMouseMiddle;
                    @mouseMiddle.canceled += instance.OnMouseMiddle;
                    @leftShift.started += instance.OnLeftShift;
                    @leftShift.performed += instance.OnLeftShift;
                    @leftShift.canceled += instance.OnLeftShift;
                    @spaceKey.started += instance.OnSpaceKey;
                    @spaceKey.performed += instance.OnSpaceKey;
                    @spaceKey.canceled += instance.OnSpaceKey;
                    @fKey.started += instance.OnFKey;
                    @fKey.performed += instance.OnFKey;
                    @fKey.canceled += instance.OnFKey;
                    @leftAlt.started += instance.OnLeftAlt;
                    @leftAlt.performed += instance.OnLeftAlt;
                    @leftAlt.canceled += instance.OnLeftAlt;
                }
            }
        }

        public KeyAndMouseActions @KeyAndMouse => new KeyAndMouseActions(this);

        public interface IKeyAndMouseActions
        {
            void OnMove(InputAction.CallbackContext context);
            void OnMousePos(InputAction.CallbackContext context);
            void OnZoom(InputAction.CallbackContext context);
            void OnMouseMove(InputAction.CallbackContext context);
            void OnMouseLeft(InputAction.CallbackContext context);
            void OnMouseRight(InputAction.CallbackContext context);
            void OnMouseMiddle(InputAction.CallbackContext context);
            void OnLeftShift(InputAction.CallbackContext context);
            void OnSpaceKey(InputAction.CallbackContext context);
            void OnFKey(InputAction.CallbackContext context);
            void OnLeftAlt(InputAction.CallbackContext context);
        }
    }
}
#endif
