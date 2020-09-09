// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/Input/PlayerControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace DavidRios.Input
{
    public class @PlayerController : IInputActionCollection, IDisposable
    {
        public InputActionAsset asset { get; }
        public @PlayerController()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""Default"",
            ""id"": ""6112a1ad-4d9f-46d9-8429-e4896f672f51"",
            ""actions"": [
                {
                    ""name"": ""MoveCamera"",
                    ""type"": ""PassThrough"",
                    ""id"": ""05033c6c-d2e0-4bb6-84f1-44d7833fed30"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RotateCamera"",
                    ""type"": ""Button"",
                    ""id"": ""d912355f-5b9c-4fc5-8482-b07ae4a1f24f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CameraVerticalMovement"",
                    ""type"": ""Button"",
                    ""id"": ""f2a1babe-b8c0-4949-aada-62e651211e2e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""BoostCamera"",
                    ""type"": ""Button"",
                    ""id"": ""914c31a5-4289-4b52-96eb-207be7997d74"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""LeftClick"",
                    ""type"": ""Button"",
                    ""id"": ""d8a15e9f-a4e4-4e3a-9d1e-56b3279131d4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RightClick"",
                    ""type"": ""Button"",
                    ""id"": ""8f8e27d0-478e-4304-8a64-e4c2aa47fa3a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MiddleClick"",
                    ""type"": ""Button"",
                    ""id"": ""ec594ef8-2313-4a09-8bfe-6784d1e082ce"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Reset/Rotate"",
                    ""type"": ""Button"",
                    ""id"": ""04bdec51-c12d-46e6-845e-5a7f9ab7839b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Deselect/Pause"",
                    ""type"": ""Button"",
                    ""id"": ""3c7a1d73-1a35-4ac5-9935-9c613ff75b0d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CinematicMode"",
                    ""type"": ""Button"",
                    ""id"": ""df429ab5-24cf-495d-9434-d6923c25f706"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""BuildMenuToggle"",
                    ""type"": ""Button"",
                    ""id"": ""1236280e-b53f-411b-a96d-48769d030d36"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""JobsMenuToggle"",
                    ""type"": ""Button"",
                    ""id"": ""9b04f406-0a8d-4688-9801-17407d8d2218"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Chop Tree"",
                    ""type"": ""Button"",
                    ""id"": ""db597c62-008d-4010-8a67-c7f4ed600ca2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""StopTime"",
                    ""type"": ""Button"",
                    ""id"": ""b0b264b0-2ba2-4f14-abf6-ba6c943ba9e8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""LockBuilding"",
                    ""type"": ""Button"",
                    ""id"": ""3bdfeb96-7abd-42db-9e9b-0e77ef222046"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TimeSpeed1"",
                    ""type"": ""Button"",
                    ""id"": ""0000e721-6dd8-4968-8d2c-09be0ae7e58d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TimeSpeed2"",
                    ""type"": ""Button"",
                    ""id"": ""6cb46f7d-1884-4205-b3a2-19c15a2dc01b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TimeSpeed3"",
                    ""type"": ""Button"",
                    ""id"": ""948e742b-0410-401c-893d-f1fef9ce797d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Delete"",
                    ""type"": ""Button"",
                    ""id"": ""3b6e6cbe-2bec-41c9-bfc5-09c47ba84dc8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD Controls"",
                    ""id"": ""5fc68d24-cbf1-4a2e-8136-6477f10e6a0e"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveCamera"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""69c007c7-6d1c-44b6-a527-ca47cbd13e02"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""b2c6cc42-3dad-4c3e-a2a1-a8915bfcf6e6"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""a17cf9c6-4f4b-429d-b5e3-08aebf5c7c7c"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""292f1d1e-7e20-49f5-9c43-77654bf1582c"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""d977282a-c86c-446a-8b28-3cd5e5e17b20"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RotateCamera"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""9742b587-e515-4592-a257-7381d1fc5346"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RotateCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""c3bdf5d9-ecf7-46a0-b606-24d8d95fc047"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RotateCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""747c13a8-0d2b-4423-8f05-ed71323455ac"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraVerticalMovement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""0799dc9a-a792-469d-bba3-473cb55b1d18"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraVerticalMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""7d5dd4ff-7d16-4487-a21c-9cee976c172d"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraVerticalMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""177c0d6d-17a6-4eb6-8672-b5ddd07d1276"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""BoostCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2549f986-a7d1-40f3-935f-9a38d1204925"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9f863bd7-138d-4957-8d19-c8c0fd237af8"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RightClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f8881b81-66ba-4cae-bcc7-277f5aec9de0"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MiddleClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e23e7e93-4f84-4e4c-8c86-7013922c8c56"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Reset/Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dcc7955b-5eed-4743-bc43-36c2d429df02"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Deselect/Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4a78e5a7-3a41-4748-8fa0-5b066cf5225e"",
                    ""path"": ""<Keyboard>/leftAlt"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CinematicMode"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""fe667d2c-0874-4f48-9f98-2e5f3789db22"",
                    ""path"": ""<Keyboard>/b"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""BuildMenuToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ccb807dc-4c62-4536-bb9b-e2e7c37d5e27"",
                    ""path"": ""<Keyboard>/backquote"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""StopTime"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""03aaff6a-caea-4755-aed2-c9c3a1efd670"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TimeSpeed1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""05b755c3-5613-49b7-be31-d8781433b4a2"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TimeSpeed2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8e1c317a-0929-46b3-9b57-33692f80d79d"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TimeSpeed3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a075d9a4-d008-41b3-ba3d-c80717870abb"",
                    ""path"": ""<Keyboard>/j"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""JobsMenuToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8d3fe846-7324-44fd-a4f7-99d57fff2d11"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Chop Tree"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c9077279-3e27-4867-8232-fde25cb9c60b"",
                    ""path"": ""<Keyboard>/delete"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Delete"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0d0622e1-9ec4-4d15-b686-94d9128800ad"",
                    ""path"": ""<Keyboard>/l"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LockBuilding"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // Default
            m_Default = asset.FindActionMap("Default", throwIfNotFound: true);
            m_Default_MoveCamera = m_Default.FindAction("MoveCamera", throwIfNotFound: true);
            m_Default_RotateCamera = m_Default.FindAction("RotateCamera", throwIfNotFound: true);
            m_Default_CameraVerticalMovement = m_Default.FindAction("CameraVerticalMovement", throwIfNotFound: true);
            m_Default_BoostCamera = m_Default.FindAction("BoostCamera", throwIfNotFound: true);
            m_Default_LeftClick = m_Default.FindAction("LeftClick", throwIfNotFound: true);
            m_Default_RightClick = m_Default.FindAction("RightClick", throwIfNotFound: true);
            m_Default_MiddleClick = m_Default.FindAction("MiddleClick", throwIfNotFound: true);
            m_Default_ResetRotate = m_Default.FindAction("Reset/Rotate", throwIfNotFound: true);
            m_Default_DeselectPause = m_Default.FindAction("Deselect/Pause", throwIfNotFound: true);
            m_Default_CinematicMode = m_Default.FindAction("CinematicMode", throwIfNotFound: true);
            m_Default_BuildMenuToggle = m_Default.FindAction("BuildMenuToggle", throwIfNotFound: true);
            m_Default_JobsMenuToggle = m_Default.FindAction("JobsMenuToggle", throwIfNotFound: true);
            m_Default_ChopTree = m_Default.FindAction("Chop Tree", throwIfNotFound: true);
            m_Default_StopTime = m_Default.FindAction("StopTime", throwIfNotFound: true);
            m_Default_LockBuilding = m_Default.FindAction("LockBuilding", throwIfNotFound: true);
            m_Default_TimeSpeed1 = m_Default.FindAction("TimeSpeed1", throwIfNotFound: true);
            m_Default_TimeSpeed2 = m_Default.FindAction("TimeSpeed2", throwIfNotFound: true);
            m_Default_TimeSpeed3 = m_Default.FindAction("TimeSpeed3", throwIfNotFound: true);
            m_Default_Delete = m_Default.FindAction("Delete", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
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

        // Default
        private readonly InputActionMap m_Default;
        private IDefaultActions m_DefaultActionsCallbackInterface;
        private readonly InputAction m_Default_MoveCamera;
        private readonly InputAction m_Default_RotateCamera;
        private readonly InputAction m_Default_CameraVerticalMovement;
        private readonly InputAction m_Default_BoostCamera;
        private readonly InputAction m_Default_LeftClick;
        private readonly InputAction m_Default_RightClick;
        private readonly InputAction m_Default_MiddleClick;
        private readonly InputAction m_Default_ResetRotate;
        private readonly InputAction m_Default_DeselectPause;
        private readonly InputAction m_Default_CinematicMode;
        private readonly InputAction m_Default_BuildMenuToggle;
        private readonly InputAction m_Default_JobsMenuToggle;
        private readonly InputAction m_Default_ChopTree;
        private readonly InputAction m_Default_StopTime;
        private readonly InputAction m_Default_LockBuilding;
        private readonly InputAction m_Default_TimeSpeed1;
        private readonly InputAction m_Default_TimeSpeed2;
        private readonly InputAction m_Default_TimeSpeed3;
        private readonly InputAction m_Default_Delete;
        public struct DefaultActions
        {
            private @PlayerController m_Wrapper;
            public DefaultActions(@PlayerController wrapper) { m_Wrapper = wrapper; }
            public InputAction @MoveCamera => m_Wrapper.m_Default_MoveCamera;
            public InputAction @RotateCamera => m_Wrapper.m_Default_RotateCamera;
            public InputAction @CameraVerticalMovement => m_Wrapper.m_Default_CameraVerticalMovement;
            public InputAction @BoostCamera => m_Wrapper.m_Default_BoostCamera;
            public InputAction @LeftClick => m_Wrapper.m_Default_LeftClick;
            public InputAction @RightClick => m_Wrapper.m_Default_RightClick;
            public InputAction @MiddleClick => m_Wrapper.m_Default_MiddleClick;
            public InputAction @ResetRotate => m_Wrapper.m_Default_ResetRotate;
            public InputAction @DeselectPause => m_Wrapper.m_Default_DeselectPause;
            public InputAction @CinematicMode => m_Wrapper.m_Default_CinematicMode;
            public InputAction @BuildMenuToggle => m_Wrapper.m_Default_BuildMenuToggle;
            public InputAction @JobsMenuToggle => m_Wrapper.m_Default_JobsMenuToggle;
            public InputAction @ChopTree => m_Wrapper.m_Default_ChopTree;
            public InputAction @StopTime => m_Wrapper.m_Default_StopTime;
            public InputAction @LockBuilding => m_Wrapper.m_Default_LockBuilding;
            public InputAction @TimeSpeed1 => m_Wrapper.m_Default_TimeSpeed1;
            public InputAction @TimeSpeed2 => m_Wrapper.m_Default_TimeSpeed2;
            public InputAction @TimeSpeed3 => m_Wrapper.m_Default_TimeSpeed3;
            public InputAction @Delete => m_Wrapper.m_Default_Delete;
            public InputActionMap Get() { return m_Wrapper.m_Default; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(DefaultActions set) { return set.Get(); }
            public void SetCallbacks(IDefaultActions instance)
            {
                if (m_Wrapper.m_DefaultActionsCallbackInterface != null)
                {
                    @MoveCamera.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnMoveCamera;
                    @MoveCamera.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnMoveCamera;
                    @MoveCamera.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnMoveCamera;
                    @RotateCamera.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnRotateCamera;
                    @RotateCamera.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnRotateCamera;
                    @RotateCamera.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnRotateCamera;
                    @CameraVerticalMovement.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnCameraVerticalMovement;
                    @CameraVerticalMovement.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnCameraVerticalMovement;
                    @CameraVerticalMovement.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnCameraVerticalMovement;
                    @BoostCamera.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnBoostCamera;
                    @BoostCamera.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnBoostCamera;
                    @BoostCamera.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnBoostCamera;
                    @LeftClick.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnLeftClick;
                    @LeftClick.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnLeftClick;
                    @LeftClick.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnLeftClick;
                    @RightClick.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnRightClick;
                    @RightClick.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnRightClick;
                    @RightClick.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnRightClick;
                    @MiddleClick.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnMiddleClick;
                    @MiddleClick.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnMiddleClick;
                    @MiddleClick.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnMiddleClick;
                    @ResetRotate.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnResetRotate;
                    @ResetRotate.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnResetRotate;
                    @ResetRotate.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnResetRotate;
                    @DeselectPause.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnDeselectPause;
                    @DeselectPause.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnDeselectPause;
                    @DeselectPause.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnDeselectPause;
                    @CinematicMode.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnCinematicMode;
                    @CinematicMode.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnCinematicMode;
                    @CinematicMode.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnCinematicMode;
                    @BuildMenuToggle.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnBuildMenuToggle;
                    @BuildMenuToggle.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnBuildMenuToggle;
                    @BuildMenuToggle.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnBuildMenuToggle;
                    @JobsMenuToggle.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnJobsMenuToggle;
                    @JobsMenuToggle.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnJobsMenuToggle;
                    @JobsMenuToggle.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnJobsMenuToggle;
                    @ChopTree.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnChopTree;
                    @ChopTree.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnChopTree;
                    @ChopTree.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnChopTree;
                    @StopTime.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStopTime;
                    @StopTime.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStopTime;
                    @StopTime.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStopTime;
                    @LockBuilding.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnLockBuilding;
                    @LockBuilding.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnLockBuilding;
                    @LockBuilding.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnLockBuilding;
                    @TimeSpeed1.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnTimeSpeed1;
                    @TimeSpeed1.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnTimeSpeed1;
                    @TimeSpeed1.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnTimeSpeed1;
                    @TimeSpeed2.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnTimeSpeed2;
                    @TimeSpeed2.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnTimeSpeed2;
                    @TimeSpeed2.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnTimeSpeed2;
                    @TimeSpeed3.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnTimeSpeed3;
                    @TimeSpeed3.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnTimeSpeed3;
                    @TimeSpeed3.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnTimeSpeed3;
                    @Delete.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnDelete;
                    @Delete.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnDelete;
                    @Delete.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnDelete;
                }
                m_Wrapper.m_DefaultActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @MoveCamera.started += instance.OnMoveCamera;
                    @MoveCamera.performed += instance.OnMoveCamera;
                    @MoveCamera.canceled += instance.OnMoveCamera;
                    @RotateCamera.started += instance.OnRotateCamera;
                    @RotateCamera.performed += instance.OnRotateCamera;
                    @RotateCamera.canceled += instance.OnRotateCamera;
                    @CameraVerticalMovement.started += instance.OnCameraVerticalMovement;
                    @CameraVerticalMovement.performed += instance.OnCameraVerticalMovement;
                    @CameraVerticalMovement.canceled += instance.OnCameraVerticalMovement;
                    @BoostCamera.started += instance.OnBoostCamera;
                    @BoostCamera.performed += instance.OnBoostCamera;
                    @BoostCamera.canceled += instance.OnBoostCamera;
                    @LeftClick.started += instance.OnLeftClick;
                    @LeftClick.performed += instance.OnLeftClick;
                    @LeftClick.canceled += instance.OnLeftClick;
                    @RightClick.started += instance.OnRightClick;
                    @RightClick.performed += instance.OnRightClick;
                    @RightClick.canceled += instance.OnRightClick;
                    @MiddleClick.started += instance.OnMiddleClick;
                    @MiddleClick.performed += instance.OnMiddleClick;
                    @MiddleClick.canceled += instance.OnMiddleClick;
                    @ResetRotate.started += instance.OnResetRotate;
                    @ResetRotate.performed += instance.OnResetRotate;
                    @ResetRotate.canceled += instance.OnResetRotate;
                    @DeselectPause.started += instance.OnDeselectPause;
                    @DeselectPause.performed += instance.OnDeselectPause;
                    @DeselectPause.canceled += instance.OnDeselectPause;
                    @CinematicMode.started += instance.OnCinematicMode;
                    @CinematicMode.performed += instance.OnCinematicMode;
                    @CinematicMode.canceled += instance.OnCinematicMode;
                    @BuildMenuToggle.started += instance.OnBuildMenuToggle;
                    @BuildMenuToggle.performed += instance.OnBuildMenuToggle;
                    @BuildMenuToggle.canceled += instance.OnBuildMenuToggle;
                    @JobsMenuToggle.started += instance.OnJobsMenuToggle;
                    @JobsMenuToggle.performed += instance.OnJobsMenuToggle;
                    @JobsMenuToggle.canceled += instance.OnJobsMenuToggle;
                    @ChopTree.started += instance.OnChopTree;
                    @ChopTree.performed += instance.OnChopTree;
                    @ChopTree.canceled += instance.OnChopTree;
                    @StopTime.started += instance.OnStopTime;
                    @StopTime.performed += instance.OnStopTime;
                    @StopTime.canceled += instance.OnStopTime;
                    @LockBuilding.started += instance.OnLockBuilding;
                    @LockBuilding.performed += instance.OnLockBuilding;
                    @LockBuilding.canceled += instance.OnLockBuilding;
                    @TimeSpeed1.started += instance.OnTimeSpeed1;
                    @TimeSpeed1.performed += instance.OnTimeSpeed1;
                    @TimeSpeed1.canceled += instance.OnTimeSpeed1;
                    @TimeSpeed2.started += instance.OnTimeSpeed2;
                    @TimeSpeed2.performed += instance.OnTimeSpeed2;
                    @TimeSpeed2.canceled += instance.OnTimeSpeed2;
                    @TimeSpeed3.started += instance.OnTimeSpeed3;
                    @TimeSpeed3.performed += instance.OnTimeSpeed3;
                    @TimeSpeed3.canceled += instance.OnTimeSpeed3;
                    @Delete.started += instance.OnDelete;
                    @Delete.performed += instance.OnDelete;
                    @Delete.canceled += instance.OnDelete;
                }
            }
        }
        public DefaultActions @Default => new DefaultActions(this);
        public interface IDefaultActions
        {
            void OnMoveCamera(InputAction.CallbackContext context);
            void OnRotateCamera(InputAction.CallbackContext context);
            void OnCameraVerticalMovement(InputAction.CallbackContext context);
            void OnBoostCamera(InputAction.CallbackContext context);
            void OnLeftClick(InputAction.CallbackContext context);
            void OnRightClick(InputAction.CallbackContext context);
            void OnMiddleClick(InputAction.CallbackContext context);
            void OnResetRotate(InputAction.CallbackContext context);
            void OnDeselectPause(InputAction.CallbackContext context);
            void OnCinematicMode(InputAction.CallbackContext context);
            void OnBuildMenuToggle(InputAction.CallbackContext context);
            void OnJobsMenuToggle(InputAction.CallbackContext context);
            void OnChopTree(InputAction.CallbackContext context);
            void OnStopTime(InputAction.CallbackContext context);
            void OnLockBuilding(InputAction.CallbackContext context);
            void OnTimeSpeed1(InputAction.CallbackContext context);
            void OnTimeSpeed2(InputAction.CallbackContext context);
            void OnTimeSpeed3(InputAction.CallbackContext context);
            void OnDelete(InputAction.CallbackContext context);
        }
    }
}
