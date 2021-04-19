// GENERATED AUTOMATICALLY FROM 'Assets/Netherlands3D/Input/3DNetherlands.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Netherlands3DInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Netherlands3DInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""3DNetherlands"",
    ""maps"": [
        {
            ""name"": ""GridSelection"",
            ""id"": ""fc0f2e8c-320f-4f22-bbb1-7ecc3341f85a"",
            ""actions"": [
                {
                    ""name"": ""ToggleVoxel"",
                    ""type"": ""Button"",
                    ""id"": ""4d71f97d-97df-4265-8641-cd87068cac97"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Tap""
                },
                {
                    ""name"": ""DrawVoxels"",
                    ""type"": ""Button"",
                    ""id"": ""50924954-e46f-44f9-8127-9103ef424e23"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Hold(duration=0.02)""
                },
                {
                    ""name"": ""EraseVoxels"",
                    ""type"": ""Button"",
                    ""id"": ""87d5dffa-a017-433c-addc-62c08df7c975"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Hold(duration=0.02)""
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""60312e81-d621-4a52-b35e-16557370191c"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""ToggleVoxel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Left mouse button + shift"",
                    ""id"": ""e143cc7b-d8d7-465b-89b9-43fcf1f91d70"",
                    ""path"": ""ButtonWithOneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""EraseVoxels"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""009480b6-4d28-4514-8291-b214f6366771"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""EraseVoxels"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""button"",
                    ""id"": ""5845d8ef-034a-40a1-b724-1c2646d177eb"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""EraseVoxels"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Left mouse button + shift"",
                    ""id"": ""9351aa68-d2d6-4be4-93e5-834ccfe177a6"",
                    ""path"": ""ButtonWithOneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DrawVoxels"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""b015cfff-a810-4270-9ce5-26c1fbe1e400"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DrawVoxels"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""button"",
                    ""id"": ""20d8e313-3851-419b-bf75-64bd5bd993c1"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DrawVoxels"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        },
        {
            ""name"": ""Selector"",
            ""id"": ""db9e8cf2-0efd-4ecf-b4d7-c9bc5e9a5062"",
            ""actions"": [
                {
                    ""name"": ""Multiselect"",
                    ""type"": ""Button"",
                    ""id"": ""a9f44bed-9b83-4ef6-abcf-acd5a4c00113"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""DoubleClick"",
                    ""type"": ""Button"",
                    ""id"": ""fe0aaeea-4249-440a-9e12-7aaead39a843"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ClickSecondary"",
                    ""type"": ""Button"",
                    ""id"": ""15c19d74-0310-409f-9e60-c9040640d172"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Click"",
                    ""type"": ""Button"",
                    ""id"": ""ac9497e2-93a6-4f44-947a-f2e56bafe8e3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""c19967f6-c230-400f-9262-23effdbce2bf"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""ClickSecondary"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f8316239-ec1c-4536-828e-244cd9633fc2"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""MultiTap"",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""DoubleClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6a41e736-680f-4b49-8848-9864b8217f1d"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Tap"",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3b1a83d8-326d-4aa2-a7fe-5110a8f0c7c1"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Multiselect"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""GodViewKeyboard"",
            ""id"": ""29c14926-d277-44e9-b8ec-1879fac15226"",
            ""actions"": [
                {
                    ""name"": ""Move Camera"",
                    ""type"": ""PassThrough"",
                    ""id"": ""d1e84f37-0f69-46fb-8b05-30decdc74b0e"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Move CameraHeight"",
                    ""type"": ""PassThrough"",
                    ""id"": ""84f25a91-8646-408e-b502-9de2b451285c"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Zoom"",
                    ""type"": ""Value"",
                    ""id"": ""fc627390-7bba-424a-91a5-a782adf200f2"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Rotate Camera"",
                    ""type"": ""Value"",
                    ""id"": ""72ab7859-34ab-4b42-bcc7-511fc65ebb4d"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Fly"",
                    ""type"": ""Value"",
                    ""id"": ""2dc5fe02-9dca-430f-bdbc-2b5039294e78"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""b0855886-00d6-4286-97e5-8a72537e5ef3"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate Camera"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""3b6e9b7d-a5e4-4a24-a1b2-c723797c149e"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""f7846781-7af1-4796-8cf1-a0ffe1b882f4"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""cf9cce08-ac18-4a24-872d-88a6c28443e4"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""20a508e9-3469-4faf-add9-b9e073b1e9b7"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""5b9c8bae-8868-422d-a0bc-c88ccf3afe51"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Rotate Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""9fcc156c-cdef-464f-94d8-496d0aabfaa2"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move Camera"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""d4ed3f5f-932a-4d1c-a10a-8f1ed1042326"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""up"",
                    ""id"": ""a9f0a527-8cf6-4c98-9dab-ad482c264c2b"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""b2c1e49f-4f25-4bf7-9528-6a20d22f7758"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""793ef613-9954-42f3-ae19-16940926e0c8"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""582a215d-7a89-4c0a-aa49-5227cdef4052"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""18f1362b-f4d9-4809-b99d-c70836eb1849"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""655c1d6a-01fa-4810-baab-6a0b2e4ae846"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""05f5b5fb-3643-4a64-a1be-ddfb5d2458db"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""ForwardBackward"",
                    ""id"": ""730774e9-8459-46c5-9fce-3732dbe94dca"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""3fc72e76-8a6a-4a69-9294-b616e1e27dac"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""884b1d8f-fbd0-45b5-8d5d-dcbe64d4c4ac"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""febbdd81-74d5-4722-8c00-c0d5330e0481"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move CameraHeight"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""84440e7e-0a0d-4469-9b5f-2870efdb7277"",
                    ""path"": ""<Keyboard>/pageDown"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move CameraHeight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""1636c197-cda4-453a-8949-a4dca2e3ab46"",
                    ""path"": ""<Keyboard>/pageUp"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move CameraHeight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""bbb6796f-6edc-458a-a390-1450b9bddad3"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move CameraHeight"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""709d3f57-6049-42c2-9d15-f441a57adb22"",
                    ""path"": ""<Gamepad>/leftStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move CameraHeight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""9836d1d1-14c6-40d2-8f15-1647a94f25bd"",
                    ""path"": ""<Gamepad>/rightStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move CameraHeight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""f5af82ee-fcc3-4de3-a57e-3af81528e048"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Fly"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""GodViewMouse"",
            ""id"": ""d0a58484-c489-430a-939d-f215553ffebe"",
            ""actions"": [
                {
                    ""name"": ""Zoom"",
                    ""type"": ""Value"",
                    ""id"": ""8cfbe0b0-e14f-439f-ab30-7eb11467c459"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ZoomDrag"",
                    ""type"": ""Button"",
                    ""id"": ""6c591bd6-36dd-4c10-903a-22e22b2016fe"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Drag"",
                    ""type"": ""Button"",
                    ""id"": ""903b8b44-4759-4735-9cb9-f43f9490d7b1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SpinDrag"",
                    ""type"": ""Button"",
                    ""id"": ""835ad5ed-5a4a-40e1-8d6f-ef4498493dfb"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""FirstPersonModifier"",
                    ""type"": ""Button"",
                    ""id"": ""053832fe-4432-49b1-96ca-0d639d5609d3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RotateAroundModifier"",
                    ""type"": ""Button"",
                    ""id"": ""b3fa879f-03f9-4550-bed8-3bff93ce6b84"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Position"",
                    ""type"": ""PassThrough"",
                    ""id"": ""c49c049d-757c-46a9-b9c5-5cf0aa7f0ccf"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""50b6cf8b-9977-4c2f-b97c-56d89099fda3"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Drag"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""688b3ef2-1a5b-4e9a-a411-204fe4fd1d3a"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""FirstPersonModifier"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b6aef2a6-736b-4a00-99c7-4720b5c4dc40"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": ""ScaleVector2(x=0.1,y=0.1)"",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a091bbae-e6cd-491b-9d82-98ec8164c56c"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ZoomDrag"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""95d405f2-cc8e-4b00-ae5f-c7c2edbf1366"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""SpinDrag"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e26af352-df3d-42b7-9c87-8a09f846590d"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Position"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9f7a1fe3-d6f3-4b8e-8595-f880cb550645"",
                    ""path"": ""<Keyboard>/alt"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""RotateAroundModifier"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Transformable"",
            ""id"": ""b90b70c2-2bfe-4c07-94cb-f1eae418634d"",
            ""actions"": [
                {
                    ""name"": ""Place"",
                    ""type"": ""Button"",
                    ""id"": ""1a3dbc7d-b1cd-4a2d-882d-2374142db434"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""c5cb54d9-ba8d-4f92-9377-e41a90124f88"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Place"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""PlaceOnClick"",
            ""id"": ""66a1f93e-fff9-4a38-89c6-c8f3ca6fafcc"",
            ""actions"": [
                {
                    ""name"": ""Place"",
                    ""type"": ""Button"",
                    ""id"": ""6446b212-f44f-4328-a9ff-b613c295e465"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""856681fa-710e-4246-b718-1fbbc380fb17"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Place"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""GizmoHandle"",
            ""id"": ""9f5cca77-43e4-4143-9b9e-2cf7c6125193"",
            ""actions"": [
                {
                    ""name"": ""Drag"",
                    ""type"": ""Button"",
                    ""id"": ""6f4bfda6-829a-42b1-8f75-11641f5ecf28"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""22dc82f1-c74e-4e0e-9d3b-1594618c3900"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Drag"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""StreetView"",
            ""id"": ""7ca4ea23-faa0-45a2-9684-e2f35e54e858"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""32572e08-53cb-4364-8152-1e3b886f4964"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Look"",
                    ""type"": ""Value"",
                    ""id"": ""675d653b-984c-4af7-8329-ccab49f75797"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Select"",
                    ""type"": ""Button"",
                    ""id"": ""f3ec09f8-ffc9-4b41-9b95-5baee755e9d0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Open Menu"",
                    ""type"": ""Button"",
                    ""id"": ""d58733f7-8c2a-4857-8540-f559b0939cc9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""978bfe49-cc26-4a3d-ab7b-7d7a29327403"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""00ca640b-d935-4593-8157-c05846ea39b3"",
                    ""path"": ""Dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""e2062cb9-1b15-46a2-838c-2f8d72a0bdd9"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""up"",
                    ""id"": ""8180e8bd-4097-4f4e-ab88-4523101a6ce9"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""320bffee-a40b-4347-ac70-c210eb8bc73a"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""1c5327b5-f71c-4f60-99c7-4e737386f1d1"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""d2581a9b-1d11-4566-b27d-b92aff5fabbc"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""2e46982e-44cc-431b-9f0b-c11910bf467a"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""fcfe95b8-67b9-4526-84b5-5d0bc98d6400"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""77bff152-3580-4b21-b6de-dcd0c7e41164"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""1635d3fe-58b6-4ba9-a4e2-f4b964f6b5c8"",
                    ""path"": ""<XRController>/{Primary2DAxis}"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""XR"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3ea4d645-4504-4529-b061-ab81934c3752"",
                    ""path"": ""<Joystick>/stick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Joystick"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c1f7a91b-d0fd-4a62-997e-7fb9b69bf235"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad;Joystick"",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8c8e490b-c610-4785-884f-f04217b23ca4"",
                    ""path"": ""<Pointer>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse;Touch"",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3e5f5442-8668-4b27-a940-df99bad7e831"",
                    ""path"": ""<Joystick>/{Hatswitch}"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Joystick"",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""143bb1cd-cc10-4eca-a2f0-a3664166fe91"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Select"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""05f6913d-c316-48b2-a6bb-e225f14c7960"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Hold"",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Select"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""886e731e-7071-4ae4-95c0-e61739dad6fd"",
                    ""path"": ""<Touchscreen>/primaryTouch/tap"",
                    ""interactions"": ""Hold"",
                    ""processors"": """",
                    ""groups"": "";Touch"",
                    ""action"": ""Select"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ee3d0cd2-254e-47a7-a8cb-bc94d9658c54"",
                    ""path"": ""<Joystick>/trigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Joystick"",
                    ""action"": ""Select"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8255d333-5683-4943-a58a-ccb207ff1dce"",
                    ""path"": ""<XRController>/{PrimaryAction}"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""XR"",
                    ""action"": ""Select"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""54674c50-bf1e-4944-af1d-3843418168de"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Open Menu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""UI"",
            ""id"": ""f3574253-e86f-48c7-b390-bd7d6ed801b9"",
            ""actions"": [
                {
                    ""name"": ""Navigate"",
                    ""type"": ""Value"",
                    ""id"": ""8478c688-8ac5-4930-8a8f-7c928e521fb9"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Submit"",
                    ""type"": ""Button"",
                    ""id"": ""b3f69f39-170c-40a8-82cd-8e950f71c93c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Cancel"",
                    ""type"": ""Button"",
                    ""id"": ""c6df8ea9-b388-4779-90d0-03cfc70db395"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Point"",
                    ""type"": ""PassThrough"",
                    ""id"": ""0e3d60b8-46ce-402e-bdee-da4466e9a6ab"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Click"",
                    ""type"": ""PassThrough"",
                    ""id"": ""52dc3f33-c6da-4c51-8398-d150afb62f6b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ScrollWheel"",
                    ""type"": ""PassThrough"",
                    ""id"": ""0c6a7fd3-03f0-41b1-8dea-2559dcaca775"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MiddleClick"",
                    ""type"": ""PassThrough"",
                    ""id"": ""6939734b-3a1e-4a58-9f90-9873e8da9ae5"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RightClick"",
                    ""type"": ""PassThrough"",
                    ""id"": ""16ec0a9b-b317-411b-a214-aaf766fa2c5c"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TrackedDevicePosition"",
                    ""type"": ""PassThrough"",
                    ""id"": ""b8af6fae-e0c6-46f7-b20b-a67565c0927a"",
                    ""expectedControlType"": ""Vector3"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TrackedDeviceOrientation"",
                    ""type"": ""PassThrough"",
                    ""id"": ""5b9c1739-b8d2-47b6-82a9-9783dc80ccbd"",
                    ""expectedControlType"": ""Quaternion"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Gamepad"",
                    ""id"": ""809f371f-c5e2-4e7a-83a1-d867598f40dd"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""14a5d6e8-4aaf-4119-a9ef-34b8c2c548bf"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""up"",
                    ""id"": ""9144cbe6-05e1-4687-a6d7-24f99d23dd81"",
                    ""path"": ""<Gamepad>/rightStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""2db08d65-c5fb-421b-983f-c71163608d67"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""58748904-2ea9-4a80-8579-b500e6a76df8"",
                    ""path"": ""<Gamepad>/rightStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""8ba04515-75aa-45de-966d-393d9bbd1c14"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""712e721c-bdfb-4b23-a86c-a0d9fcfea921"",
                    ""path"": ""<Gamepad>/rightStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""fcd248ae-a788-4676-a12e-f4d81205600b"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""1f04d9bc-c50b-41a1-bfcc-afb75475ec20"",
                    ""path"": ""<Gamepad>/rightStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""fb8277d4-c5cd-4663-9dc7-ee3f0b506d90"",
                    ""path"": ""<Gamepad>/dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Joystick"",
                    ""id"": ""e25d9774-381c-4a61-b47c-7b6b299ad9f9"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""3db53b26-6601-41be-9887-63ac74e79d19"",
                    ""path"": ""<Joystick>/stick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Joystick"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""0cb3e13e-3d90-4178-8ae6-d9c5501d653f"",
                    ""path"": ""<Joystick>/stick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Joystick"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""0392d399-f6dd-4c82-8062-c1e9c0d34835"",
                    ""path"": ""<Joystick>/stick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Joystick"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""942a66d9-d42f-43d6-8d70-ecb4ba5363bc"",
                    ""path"": ""<Joystick>/stick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Joystick"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""ff527021-f211-4c02-933e-5976594c46ed"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""563fbfdd-0f09-408d-aa75-8642c4f08ef0"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""up"",
                    ""id"": ""eb480147-c587-4a33-85ed-eb0ab9942c43"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""2bf42165-60bc-42ca-8072-8c13ab40239b"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""85d264ad-e0a0-4565-b7ff-1a37edde51ac"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""74214943-c580-44e4-98eb-ad7eebe17902"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""cea9b045-a000-445b-95b8-0c171af70a3b"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""8607c725-d935-4808-84b1-8354e29bab63"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""4cda81dc-9edd-4e03-9d7c-a71a14345d0b"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""9e92bb26-7e3b-4ec4-b06b-3c8f8e498ddc"",
                    ""path"": ""*/{Submit}"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Submit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""82627dcc-3b13-4ba9-841d-e4b746d6553e"",
                    ""path"": ""*/{Cancel}"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c52c8e0b-8179-41d3-b8a1-d149033bbe86"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Point"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e1394cbc-336e-44ce-9ea8-6007ed6193f7"",
                    ""path"": ""<Pen>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Point"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5693e57a-238a-46ed-b5ae-e64e6e574302"",
                    ""path"": ""<Touchscreen>/touch*/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Touch"",
                    ""action"": ""Point"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4faf7dc9-b979-4210-aa8c-e808e1ef89f5"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8d66d5ba-88d7-48e6-b1cd-198bbfef7ace"",
                    ""path"": ""<Pen>/tip"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""47c2a644-3ebc-4dae-a106-589b7ca75b59"",
                    ""path"": ""<Touchscreen>/touch*/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Touch"",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bb9e6b34-44bf-4381-ac63-5aa15d19f677"",
                    ""path"": ""<XRController>/trigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""XR"",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""38c99815-14ea-4617-8627-164d27641299"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""ScrollWheel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""24066f69-da47-44f3-a07e-0015fb02eb2e"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""MiddleClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4c191405-5738-4d4b-a523-c6a301dbf754"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""RightClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7236c0d9-6ca3-47cf-a6ee-a97f5b59ea77"",
                    ""path"": ""<XRController>/devicePosition"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""XR"",
                    ""action"": ""TrackedDevicePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""23e01e3a-f935-4948-8d8b-9bcac77714fb"",
                    ""path"": ""<XRController>/deviceRotation"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""XR"",
                    ""action"": ""TrackedDeviceOrientation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""SelectionTool"",
            ""id"": ""402bd2ad-ab88-4f3e-9dd3-032a690c2e47"",
            ""actions"": [
                {
                    ""name"": ""StartSelection"",
                    ""type"": ""Button"",
                    ""id"": ""48b15302-b62b-4d37-ba27-65de37a2578e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Hold(duration=0.02)""
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Left mouse button + shift"",
                    ""id"": ""699717e1-25c3-428c-a6ed-38915bc36daa"",
                    ""path"": ""ButtonWithOneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""StartSelection"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""5de6fc12-f6f8-4e0b-817f-f0e8ddad9e90"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""StartSelection"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""button"",
                    ""id"": ""6b75d90b-01d5-46b0-8545-b180edbbcf8f"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""StartSelection"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard&Mouse"",
            ""bindingGroup"": ""Keyboard&Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Gamepad"",
            ""bindingGroup"": ""Gamepad"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Touch"",
            ""bindingGroup"": ""Touch"",
            ""devices"": [
                {
                    ""devicePath"": ""<Touchscreen>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Joystick"",
            ""bindingGroup"": ""Joystick"",
            ""devices"": [
                {
                    ""devicePath"": ""<Joystick>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""XR"",
            ""bindingGroup"": ""XR"",
            ""devices"": [
                {
                    ""devicePath"": ""<XRController>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // GridSelection
        m_GridSelection = asset.FindActionMap("GridSelection", throwIfNotFound: true);
        m_GridSelection_ToggleVoxel = m_GridSelection.FindAction("ToggleVoxel", throwIfNotFound: true);
        m_GridSelection_DrawVoxels = m_GridSelection.FindAction("DrawVoxels", throwIfNotFound: true);
        m_GridSelection_EraseVoxels = m_GridSelection.FindAction("EraseVoxels", throwIfNotFound: true);
        // Selector
        m_Selector = asset.FindActionMap("Selector", throwIfNotFound: true);
        m_Selector_Multiselect = m_Selector.FindAction("Multiselect", throwIfNotFound: true);
        m_Selector_DoubleClick = m_Selector.FindAction("DoubleClick", throwIfNotFound: true);
        m_Selector_ClickSecondary = m_Selector.FindAction("ClickSecondary", throwIfNotFound: true);
        m_Selector_Click = m_Selector.FindAction("Click", throwIfNotFound: true);
        // GodViewKeyboard
        m_GodViewKeyboard = asset.FindActionMap("GodViewKeyboard", throwIfNotFound: true);
        m_GodViewKeyboard_MoveCamera = m_GodViewKeyboard.FindAction("Move Camera", throwIfNotFound: true);
        m_GodViewKeyboard_MoveCameraHeight = m_GodViewKeyboard.FindAction("Move CameraHeight", throwIfNotFound: true);
        m_GodViewKeyboard_Zoom = m_GodViewKeyboard.FindAction("Zoom", throwIfNotFound: true);
        m_GodViewKeyboard_RotateCamera = m_GodViewKeyboard.FindAction("Rotate Camera", throwIfNotFound: true);
        m_GodViewKeyboard_Fly = m_GodViewKeyboard.FindAction("Fly", throwIfNotFound: true);
        // GodViewMouse
        m_GodViewMouse = asset.FindActionMap("GodViewMouse", throwIfNotFound: true);
        m_GodViewMouse_Zoom = m_GodViewMouse.FindAction("Zoom", throwIfNotFound: true);
        m_GodViewMouse_ZoomDrag = m_GodViewMouse.FindAction("ZoomDrag", throwIfNotFound: true);
        m_GodViewMouse_Drag = m_GodViewMouse.FindAction("Drag", throwIfNotFound: true);
        m_GodViewMouse_SpinDrag = m_GodViewMouse.FindAction("SpinDrag", throwIfNotFound: true);
        m_GodViewMouse_FirstPersonModifier = m_GodViewMouse.FindAction("FirstPersonModifier", throwIfNotFound: true);
        m_GodViewMouse_RotateAroundModifier = m_GodViewMouse.FindAction("RotateAroundModifier", throwIfNotFound: true);
        m_GodViewMouse_Position = m_GodViewMouse.FindAction("Position", throwIfNotFound: true);
        // Transformable
        m_Transformable = asset.FindActionMap("Transformable", throwIfNotFound: true);
        m_Transformable_Place = m_Transformable.FindAction("Place", throwIfNotFound: true);
        // PlaceOnClick
        m_PlaceOnClick = asset.FindActionMap("PlaceOnClick", throwIfNotFound: true);
        m_PlaceOnClick_Place = m_PlaceOnClick.FindAction("Place", throwIfNotFound: true);
        // GizmoHandle
        m_GizmoHandle = asset.FindActionMap("GizmoHandle", throwIfNotFound: true);
        m_GizmoHandle_Drag = m_GizmoHandle.FindAction("Drag", throwIfNotFound: true);
        // StreetView
        m_StreetView = asset.FindActionMap("StreetView", throwIfNotFound: true);
        m_StreetView_Move = m_StreetView.FindAction("Move", throwIfNotFound: true);
        m_StreetView_Look = m_StreetView.FindAction("Look", throwIfNotFound: true);
        m_StreetView_Select = m_StreetView.FindAction("Select", throwIfNotFound: true);
        m_StreetView_OpenMenu = m_StreetView.FindAction("Open Menu", throwIfNotFound: true);
        // UI
        m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
        m_UI_Navigate = m_UI.FindAction("Navigate", throwIfNotFound: true);
        m_UI_Submit = m_UI.FindAction("Submit", throwIfNotFound: true);
        m_UI_Cancel = m_UI.FindAction("Cancel", throwIfNotFound: true);
        m_UI_Point = m_UI.FindAction("Point", throwIfNotFound: true);
        m_UI_Click = m_UI.FindAction("Click", throwIfNotFound: true);
        m_UI_ScrollWheel = m_UI.FindAction("ScrollWheel", throwIfNotFound: true);
        m_UI_MiddleClick = m_UI.FindAction("MiddleClick", throwIfNotFound: true);
        m_UI_RightClick = m_UI.FindAction("RightClick", throwIfNotFound: true);
        m_UI_TrackedDevicePosition = m_UI.FindAction("TrackedDevicePosition", throwIfNotFound: true);
        m_UI_TrackedDeviceOrientation = m_UI.FindAction("TrackedDeviceOrientation", throwIfNotFound: true);
        // SelectionTool
        m_SelectionTool = asset.FindActionMap("SelectionTool", throwIfNotFound: true);
        m_SelectionTool_StartSelection = m_SelectionTool.FindAction("StartSelection", throwIfNotFound: true);
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

    // GridSelection
    private readonly InputActionMap m_GridSelection;
    private IGridSelectionActions m_GridSelectionActionsCallbackInterface;
    private readonly InputAction m_GridSelection_ToggleVoxel;
    private readonly InputAction m_GridSelection_DrawVoxels;
    private readonly InputAction m_GridSelection_EraseVoxels;
    public struct GridSelectionActions
    {
        private @Netherlands3DInputActions m_Wrapper;
        public GridSelectionActions(@Netherlands3DInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @ToggleVoxel => m_Wrapper.m_GridSelection_ToggleVoxel;
        public InputAction @DrawVoxels => m_Wrapper.m_GridSelection_DrawVoxels;
        public InputAction @EraseVoxels => m_Wrapper.m_GridSelection_EraseVoxels;
        public InputActionMap Get() { return m_Wrapper.m_GridSelection; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GridSelectionActions set) { return set.Get(); }
        public void SetCallbacks(IGridSelectionActions instance)
        {
            if (m_Wrapper.m_GridSelectionActionsCallbackInterface != null)
            {
                @ToggleVoxel.started -= m_Wrapper.m_GridSelectionActionsCallbackInterface.OnToggleVoxel;
                @ToggleVoxel.performed -= m_Wrapper.m_GridSelectionActionsCallbackInterface.OnToggleVoxel;
                @ToggleVoxel.canceled -= m_Wrapper.m_GridSelectionActionsCallbackInterface.OnToggleVoxel;
                @DrawVoxels.started -= m_Wrapper.m_GridSelectionActionsCallbackInterface.OnDrawVoxels;
                @DrawVoxels.performed -= m_Wrapper.m_GridSelectionActionsCallbackInterface.OnDrawVoxels;
                @DrawVoxels.canceled -= m_Wrapper.m_GridSelectionActionsCallbackInterface.OnDrawVoxels;
                @EraseVoxels.started -= m_Wrapper.m_GridSelectionActionsCallbackInterface.OnEraseVoxels;
                @EraseVoxels.performed -= m_Wrapper.m_GridSelectionActionsCallbackInterface.OnEraseVoxels;
                @EraseVoxels.canceled -= m_Wrapper.m_GridSelectionActionsCallbackInterface.OnEraseVoxels;
            }
            m_Wrapper.m_GridSelectionActionsCallbackInterface = instance;
            if (instance != null)
            {
                @ToggleVoxel.started += instance.OnToggleVoxel;
                @ToggleVoxel.performed += instance.OnToggleVoxel;
                @ToggleVoxel.canceled += instance.OnToggleVoxel;
                @DrawVoxels.started += instance.OnDrawVoxels;
                @DrawVoxels.performed += instance.OnDrawVoxels;
                @DrawVoxels.canceled += instance.OnDrawVoxels;
                @EraseVoxels.started += instance.OnEraseVoxels;
                @EraseVoxels.performed += instance.OnEraseVoxels;
                @EraseVoxels.canceled += instance.OnEraseVoxels;
            }
        }
    }
    public GridSelectionActions @GridSelection => new GridSelectionActions(this);

    // Selector
    private readonly InputActionMap m_Selector;
    private ISelectorActions m_SelectorActionsCallbackInterface;
    private readonly InputAction m_Selector_Multiselect;
    private readonly InputAction m_Selector_DoubleClick;
    private readonly InputAction m_Selector_ClickSecondary;
    private readonly InputAction m_Selector_Click;
    public struct SelectorActions
    {
        private @Netherlands3DInputActions m_Wrapper;
        public SelectorActions(@Netherlands3DInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Multiselect => m_Wrapper.m_Selector_Multiselect;
        public InputAction @DoubleClick => m_Wrapper.m_Selector_DoubleClick;
        public InputAction @ClickSecondary => m_Wrapper.m_Selector_ClickSecondary;
        public InputAction @Click => m_Wrapper.m_Selector_Click;
        public InputActionMap Get() { return m_Wrapper.m_Selector; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(SelectorActions set) { return set.Get(); }
        public void SetCallbacks(ISelectorActions instance)
        {
            if (m_Wrapper.m_SelectorActionsCallbackInterface != null)
            {
                @Multiselect.started -= m_Wrapper.m_SelectorActionsCallbackInterface.OnMultiselect;
                @Multiselect.performed -= m_Wrapper.m_SelectorActionsCallbackInterface.OnMultiselect;
                @Multiselect.canceled -= m_Wrapper.m_SelectorActionsCallbackInterface.OnMultiselect;
                @DoubleClick.started -= m_Wrapper.m_SelectorActionsCallbackInterface.OnDoubleClick;
                @DoubleClick.performed -= m_Wrapper.m_SelectorActionsCallbackInterface.OnDoubleClick;
                @DoubleClick.canceled -= m_Wrapper.m_SelectorActionsCallbackInterface.OnDoubleClick;
                @ClickSecondary.started -= m_Wrapper.m_SelectorActionsCallbackInterface.OnClickSecondary;
                @ClickSecondary.performed -= m_Wrapper.m_SelectorActionsCallbackInterface.OnClickSecondary;
                @ClickSecondary.canceled -= m_Wrapper.m_SelectorActionsCallbackInterface.OnClickSecondary;
                @Click.started -= m_Wrapper.m_SelectorActionsCallbackInterface.OnClick;
                @Click.performed -= m_Wrapper.m_SelectorActionsCallbackInterface.OnClick;
                @Click.canceled -= m_Wrapper.m_SelectorActionsCallbackInterface.OnClick;
            }
            m_Wrapper.m_SelectorActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Multiselect.started += instance.OnMultiselect;
                @Multiselect.performed += instance.OnMultiselect;
                @Multiselect.canceled += instance.OnMultiselect;
                @DoubleClick.started += instance.OnDoubleClick;
                @DoubleClick.performed += instance.OnDoubleClick;
                @DoubleClick.canceled += instance.OnDoubleClick;
                @ClickSecondary.started += instance.OnClickSecondary;
                @ClickSecondary.performed += instance.OnClickSecondary;
                @ClickSecondary.canceled += instance.OnClickSecondary;
                @Click.started += instance.OnClick;
                @Click.performed += instance.OnClick;
                @Click.canceled += instance.OnClick;
            }
        }
    }
    public SelectorActions @Selector => new SelectorActions(this);

    // GodViewKeyboard
    private readonly InputActionMap m_GodViewKeyboard;
    private IGodViewKeyboardActions m_GodViewKeyboardActionsCallbackInterface;
    private readonly InputAction m_GodViewKeyboard_MoveCamera;
    private readonly InputAction m_GodViewKeyboard_MoveCameraHeight;
    private readonly InputAction m_GodViewKeyboard_Zoom;
    private readonly InputAction m_GodViewKeyboard_RotateCamera;
    private readonly InputAction m_GodViewKeyboard_Fly;
    public struct GodViewKeyboardActions
    {
        private @Netherlands3DInputActions m_Wrapper;
        public GodViewKeyboardActions(@Netherlands3DInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @MoveCamera => m_Wrapper.m_GodViewKeyboard_MoveCamera;
        public InputAction @MoveCameraHeight => m_Wrapper.m_GodViewKeyboard_MoveCameraHeight;
        public InputAction @Zoom => m_Wrapper.m_GodViewKeyboard_Zoom;
        public InputAction @RotateCamera => m_Wrapper.m_GodViewKeyboard_RotateCamera;
        public InputAction @Fly => m_Wrapper.m_GodViewKeyboard_Fly;
        public InputActionMap Get() { return m_Wrapper.m_GodViewKeyboard; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GodViewKeyboardActions set) { return set.Get(); }
        public void SetCallbacks(IGodViewKeyboardActions instance)
        {
            if (m_Wrapper.m_GodViewKeyboardActionsCallbackInterface != null)
            {
                @MoveCamera.started -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnMoveCamera;
                @MoveCamera.performed -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnMoveCamera;
                @MoveCamera.canceled -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnMoveCamera;
                @MoveCameraHeight.started -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnMoveCameraHeight;
                @MoveCameraHeight.performed -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnMoveCameraHeight;
                @MoveCameraHeight.canceled -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnMoveCameraHeight;
                @Zoom.started -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnZoom;
                @Zoom.performed -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnZoom;
                @Zoom.canceled -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnZoom;
                @RotateCamera.started -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnRotateCamera;
                @RotateCamera.performed -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnRotateCamera;
                @RotateCamera.canceled -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnRotateCamera;
                @Fly.started -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnFly;
                @Fly.performed -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnFly;
                @Fly.canceled -= m_Wrapper.m_GodViewKeyboardActionsCallbackInterface.OnFly;
            }
            m_Wrapper.m_GodViewKeyboardActionsCallbackInterface = instance;
            if (instance != null)
            {
                @MoveCamera.started += instance.OnMoveCamera;
                @MoveCamera.performed += instance.OnMoveCamera;
                @MoveCamera.canceled += instance.OnMoveCamera;
                @MoveCameraHeight.started += instance.OnMoveCameraHeight;
                @MoveCameraHeight.performed += instance.OnMoveCameraHeight;
                @MoveCameraHeight.canceled += instance.OnMoveCameraHeight;
                @Zoom.started += instance.OnZoom;
                @Zoom.performed += instance.OnZoom;
                @Zoom.canceled += instance.OnZoom;
                @RotateCamera.started += instance.OnRotateCamera;
                @RotateCamera.performed += instance.OnRotateCamera;
                @RotateCamera.canceled += instance.OnRotateCamera;
                @Fly.started += instance.OnFly;
                @Fly.performed += instance.OnFly;
                @Fly.canceled += instance.OnFly;
            }
        }
    }
    public GodViewKeyboardActions @GodViewKeyboard => new GodViewKeyboardActions(this);

    // GodViewMouse
    private readonly InputActionMap m_GodViewMouse;
    private IGodViewMouseActions m_GodViewMouseActionsCallbackInterface;
    private readonly InputAction m_GodViewMouse_Zoom;
    private readonly InputAction m_GodViewMouse_ZoomDrag;
    private readonly InputAction m_GodViewMouse_Drag;
    private readonly InputAction m_GodViewMouse_SpinDrag;
    private readonly InputAction m_GodViewMouse_FirstPersonModifier;
    private readonly InputAction m_GodViewMouse_RotateAroundModifier;
    private readonly InputAction m_GodViewMouse_Position;
    public struct GodViewMouseActions
    {
        private @Netherlands3DInputActions m_Wrapper;
        public GodViewMouseActions(@Netherlands3DInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Zoom => m_Wrapper.m_GodViewMouse_Zoom;
        public InputAction @ZoomDrag => m_Wrapper.m_GodViewMouse_ZoomDrag;
        public InputAction @Drag => m_Wrapper.m_GodViewMouse_Drag;
        public InputAction @SpinDrag => m_Wrapper.m_GodViewMouse_SpinDrag;
        public InputAction @FirstPersonModifier => m_Wrapper.m_GodViewMouse_FirstPersonModifier;
        public InputAction @RotateAroundModifier => m_Wrapper.m_GodViewMouse_RotateAroundModifier;
        public InputAction @Position => m_Wrapper.m_GodViewMouse_Position;
        public InputActionMap Get() { return m_Wrapper.m_GodViewMouse; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GodViewMouseActions set) { return set.Get(); }
        public void SetCallbacks(IGodViewMouseActions instance)
        {
            if (m_Wrapper.m_GodViewMouseActionsCallbackInterface != null)
            {
                @Zoom.started -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnZoom;
                @Zoom.performed -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnZoom;
                @Zoom.canceled -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnZoom;
                @ZoomDrag.started -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnZoomDrag;
                @ZoomDrag.performed -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnZoomDrag;
                @ZoomDrag.canceled -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnZoomDrag;
                @Drag.started -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnDrag;
                @Drag.performed -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnDrag;
                @Drag.canceled -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnDrag;
                @SpinDrag.started -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnSpinDrag;
                @SpinDrag.performed -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnSpinDrag;
                @SpinDrag.canceled -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnSpinDrag;
                @FirstPersonModifier.started -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnFirstPersonModifier;
                @FirstPersonModifier.performed -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnFirstPersonModifier;
                @FirstPersonModifier.canceled -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnFirstPersonModifier;
                @RotateAroundModifier.started -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnRotateAroundModifier;
                @RotateAroundModifier.performed -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnRotateAroundModifier;
                @RotateAroundModifier.canceled -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnRotateAroundModifier;
                @Position.started -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnPosition;
                @Position.performed -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnPosition;
                @Position.canceled -= m_Wrapper.m_GodViewMouseActionsCallbackInterface.OnPosition;
            }
            m_Wrapper.m_GodViewMouseActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Zoom.started += instance.OnZoom;
                @Zoom.performed += instance.OnZoom;
                @Zoom.canceled += instance.OnZoom;
                @ZoomDrag.started += instance.OnZoomDrag;
                @ZoomDrag.performed += instance.OnZoomDrag;
                @ZoomDrag.canceled += instance.OnZoomDrag;
                @Drag.started += instance.OnDrag;
                @Drag.performed += instance.OnDrag;
                @Drag.canceled += instance.OnDrag;
                @SpinDrag.started += instance.OnSpinDrag;
                @SpinDrag.performed += instance.OnSpinDrag;
                @SpinDrag.canceled += instance.OnSpinDrag;
                @FirstPersonModifier.started += instance.OnFirstPersonModifier;
                @FirstPersonModifier.performed += instance.OnFirstPersonModifier;
                @FirstPersonModifier.canceled += instance.OnFirstPersonModifier;
                @RotateAroundModifier.started += instance.OnRotateAroundModifier;
                @RotateAroundModifier.performed += instance.OnRotateAroundModifier;
                @RotateAroundModifier.canceled += instance.OnRotateAroundModifier;
                @Position.started += instance.OnPosition;
                @Position.performed += instance.OnPosition;
                @Position.canceled += instance.OnPosition;
            }
        }
    }
    public GodViewMouseActions @GodViewMouse => new GodViewMouseActions(this);

    // Transformable
    private readonly InputActionMap m_Transformable;
    private ITransformableActions m_TransformableActionsCallbackInterface;
    private readonly InputAction m_Transformable_Place;
    public struct TransformableActions
    {
        private @Netherlands3DInputActions m_Wrapper;
        public TransformableActions(@Netherlands3DInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Place => m_Wrapper.m_Transformable_Place;
        public InputActionMap Get() { return m_Wrapper.m_Transformable; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(TransformableActions set) { return set.Get(); }
        public void SetCallbacks(ITransformableActions instance)
        {
            if (m_Wrapper.m_TransformableActionsCallbackInterface != null)
            {
                @Place.started -= m_Wrapper.m_TransformableActionsCallbackInterface.OnPlace;
                @Place.performed -= m_Wrapper.m_TransformableActionsCallbackInterface.OnPlace;
                @Place.canceled -= m_Wrapper.m_TransformableActionsCallbackInterface.OnPlace;
            }
            m_Wrapper.m_TransformableActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Place.started += instance.OnPlace;
                @Place.performed += instance.OnPlace;
                @Place.canceled += instance.OnPlace;
            }
        }
    }
    public TransformableActions @Transformable => new TransformableActions(this);

    // PlaceOnClick
    private readonly InputActionMap m_PlaceOnClick;
    private IPlaceOnClickActions m_PlaceOnClickActionsCallbackInterface;
    private readonly InputAction m_PlaceOnClick_Place;
    public struct PlaceOnClickActions
    {
        private @Netherlands3DInputActions m_Wrapper;
        public PlaceOnClickActions(@Netherlands3DInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Place => m_Wrapper.m_PlaceOnClick_Place;
        public InputActionMap Get() { return m_Wrapper.m_PlaceOnClick; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlaceOnClickActions set) { return set.Get(); }
        public void SetCallbacks(IPlaceOnClickActions instance)
        {
            if (m_Wrapper.m_PlaceOnClickActionsCallbackInterface != null)
            {
                @Place.started -= m_Wrapper.m_PlaceOnClickActionsCallbackInterface.OnPlace;
                @Place.performed -= m_Wrapper.m_PlaceOnClickActionsCallbackInterface.OnPlace;
                @Place.canceled -= m_Wrapper.m_PlaceOnClickActionsCallbackInterface.OnPlace;
            }
            m_Wrapper.m_PlaceOnClickActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Place.started += instance.OnPlace;
                @Place.performed += instance.OnPlace;
                @Place.canceled += instance.OnPlace;
            }
        }
    }
    public PlaceOnClickActions @PlaceOnClick => new PlaceOnClickActions(this);

    // GizmoHandle
    private readonly InputActionMap m_GizmoHandle;
    private IGizmoHandleActions m_GizmoHandleActionsCallbackInterface;
    private readonly InputAction m_GizmoHandle_Drag;
    public struct GizmoHandleActions
    {
        private @Netherlands3DInputActions m_Wrapper;
        public GizmoHandleActions(@Netherlands3DInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Drag => m_Wrapper.m_GizmoHandle_Drag;
        public InputActionMap Get() { return m_Wrapper.m_GizmoHandle; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GizmoHandleActions set) { return set.Get(); }
        public void SetCallbacks(IGizmoHandleActions instance)
        {
            if (m_Wrapper.m_GizmoHandleActionsCallbackInterface != null)
            {
                @Drag.started -= m_Wrapper.m_GizmoHandleActionsCallbackInterface.OnDrag;
                @Drag.performed -= m_Wrapper.m_GizmoHandleActionsCallbackInterface.OnDrag;
                @Drag.canceled -= m_Wrapper.m_GizmoHandleActionsCallbackInterface.OnDrag;
            }
            m_Wrapper.m_GizmoHandleActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Drag.started += instance.OnDrag;
                @Drag.performed += instance.OnDrag;
                @Drag.canceled += instance.OnDrag;
            }
        }
    }
    public GizmoHandleActions @GizmoHandle => new GizmoHandleActions(this);

    // StreetView
    private readonly InputActionMap m_StreetView;
    private IStreetViewActions m_StreetViewActionsCallbackInterface;
    private readonly InputAction m_StreetView_Move;
    private readonly InputAction m_StreetView_Look;
    private readonly InputAction m_StreetView_Select;
    private readonly InputAction m_StreetView_OpenMenu;
    public struct StreetViewActions
    {
        private @Netherlands3DInputActions m_Wrapper;
        public StreetViewActions(@Netherlands3DInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_StreetView_Move;
        public InputAction @Look => m_Wrapper.m_StreetView_Look;
        public InputAction @Select => m_Wrapper.m_StreetView_Select;
        public InputAction @OpenMenu => m_Wrapper.m_StreetView_OpenMenu;
        public InputActionMap Get() { return m_Wrapper.m_StreetView; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(StreetViewActions set) { return set.Get(); }
        public void SetCallbacks(IStreetViewActions instance)
        {
            if (m_Wrapper.m_StreetViewActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_StreetViewActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_StreetViewActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_StreetViewActionsCallbackInterface.OnMove;
                @Look.started -= m_Wrapper.m_StreetViewActionsCallbackInterface.OnLook;
                @Look.performed -= m_Wrapper.m_StreetViewActionsCallbackInterface.OnLook;
                @Look.canceled -= m_Wrapper.m_StreetViewActionsCallbackInterface.OnLook;
                @Select.started -= m_Wrapper.m_StreetViewActionsCallbackInterface.OnSelect;
                @Select.performed -= m_Wrapper.m_StreetViewActionsCallbackInterface.OnSelect;
                @Select.canceled -= m_Wrapper.m_StreetViewActionsCallbackInterface.OnSelect;
                @OpenMenu.started -= m_Wrapper.m_StreetViewActionsCallbackInterface.OnOpenMenu;
                @OpenMenu.performed -= m_Wrapper.m_StreetViewActionsCallbackInterface.OnOpenMenu;
                @OpenMenu.canceled -= m_Wrapper.m_StreetViewActionsCallbackInterface.OnOpenMenu;
            }
            m_Wrapper.m_StreetViewActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Look.started += instance.OnLook;
                @Look.performed += instance.OnLook;
                @Look.canceled += instance.OnLook;
                @Select.started += instance.OnSelect;
                @Select.performed += instance.OnSelect;
                @Select.canceled += instance.OnSelect;
                @OpenMenu.started += instance.OnOpenMenu;
                @OpenMenu.performed += instance.OnOpenMenu;
                @OpenMenu.canceled += instance.OnOpenMenu;
            }
        }
    }
    public StreetViewActions @StreetView => new StreetViewActions(this);

    // UI
    private readonly InputActionMap m_UI;
    private IUIActions m_UIActionsCallbackInterface;
    private readonly InputAction m_UI_Navigate;
    private readonly InputAction m_UI_Submit;
    private readonly InputAction m_UI_Cancel;
    private readonly InputAction m_UI_Point;
    private readonly InputAction m_UI_Click;
    private readonly InputAction m_UI_ScrollWheel;
    private readonly InputAction m_UI_MiddleClick;
    private readonly InputAction m_UI_RightClick;
    private readonly InputAction m_UI_TrackedDevicePosition;
    private readonly InputAction m_UI_TrackedDeviceOrientation;
    public struct UIActions
    {
        private @Netherlands3DInputActions m_Wrapper;
        public UIActions(@Netherlands3DInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Navigate => m_Wrapper.m_UI_Navigate;
        public InputAction @Submit => m_Wrapper.m_UI_Submit;
        public InputAction @Cancel => m_Wrapper.m_UI_Cancel;
        public InputAction @Point => m_Wrapper.m_UI_Point;
        public InputAction @Click => m_Wrapper.m_UI_Click;
        public InputAction @ScrollWheel => m_Wrapper.m_UI_ScrollWheel;
        public InputAction @MiddleClick => m_Wrapper.m_UI_MiddleClick;
        public InputAction @RightClick => m_Wrapper.m_UI_RightClick;
        public InputAction @TrackedDevicePosition => m_Wrapper.m_UI_TrackedDevicePosition;
        public InputAction @TrackedDeviceOrientation => m_Wrapper.m_UI_TrackedDeviceOrientation;
        public InputActionMap Get() { return m_Wrapper.m_UI; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(UIActions set) { return set.Get(); }
        public void SetCallbacks(IUIActions instance)
        {
            if (m_Wrapper.m_UIActionsCallbackInterface != null)
            {
                @Navigate.started -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigate;
                @Navigate.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigate;
                @Navigate.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigate;
                @Submit.started -= m_Wrapper.m_UIActionsCallbackInterface.OnSubmit;
                @Submit.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnSubmit;
                @Submit.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnSubmit;
                @Cancel.started -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
                @Cancel.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
                @Cancel.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
                @Point.started -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
                @Point.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
                @Point.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
                @Click.started -= m_Wrapper.m_UIActionsCallbackInterface.OnClick;
                @Click.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnClick;
                @Click.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnClick;
                @ScrollWheel.started -= m_Wrapper.m_UIActionsCallbackInterface.OnScrollWheel;
                @ScrollWheel.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnScrollWheel;
                @ScrollWheel.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnScrollWheel;
                @MiddleClick.started -= m_Wrapper.m_UIActionsCallbackInterface.OnMiddleClick;
                @MiddleClick.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnMiddleClick;
                @MiddleClick.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnMiddleClick;
                @RightClick.started -= m_Wrapper.m_UIActionsCallbackInterface.OnRightClick;
                @RightClick.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnRightClick;
                @RightClick.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnRightClick;
                @TrackedDevicePosition.started -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDevicePosition;
                @TrackedDevicePosition.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDevicePosition;
                @TrackedDevicePosition.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDevicePosition;
                @TrackedDeviceOrientation.started -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDeviceOrientation;
                @TrackedDeviceOrientation.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDeviceOrientation;
                @TrackedDeviceOrientation.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDeviceOrientation;
            }
            m_Wrapper.m_UIActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Navigate.started += instance.OnNavigate;
                @Navigate.performed += instance.OnNavigate;
                @Navigate.canceled += instance.OnNavigate;
                @Submit.started += instance.OnSubmit;
                @Submit.performed += instance.OnSubmit;
                @Submit.canceled += instance.OnSubmit;
                @Cancel.started += instance.OnCancel;
                @Cancel.performed += instance.OnCancel;
                @Cancel.canceled += instance.OnCancel;
                @Point.started += instance.OnPoint;
                @Point.performed += instance.OnPoint;
                @Point.canceled += instance.OnPoint;
                @Click.started += instance.OnClick;
                @Click.performed += instance.OnClick;
                @Click.canceled += instance.OnClick;
                @ScrollWheel.started += instance.OnScrollWheel;
                @ScrollWheel.performed += instance.OnScrollWheel;
                @ScrollWheel.canceled += instance.OnScrollWheel;
                @MiddleClick.started += instance.OnMiddleClick;
                @MiddleClick.performed += instance.OnMiddleClick;
                @MiddleClick.canceled += instance.OnMiddleClick;
                @RightClick.started += instance.OnRightClick;
                @RightClick.performed += instance.OnRightClick;
                @RightClick.canceled += instance.OnRightClick;
                @TrackedDevicePosition.started += instance.OnTrackedDevicePosition;
                @TrackedDevicePosition.performed += instance.OnTrackedDevicePosition;
                @TrackedDevicePosition.canceled += instance.OnTrackedDevicePosition;
                @TrackedDeviceOrientation.started += instance.OnTrackedDeviceOrientation;
                @TrackedDeviceOrientation.performed += instance.OnTrackedDeviceOrientation;
                @TrackedDeviceOrientation.canceled += instance.OnTrackedDeviceOrientation;
            }
        }
    }
    public UIActions @UI => new UIActions(this);

    // SelectionTool
    private readonly InputActionMap m_SelectionTool;
    private ISelectionToolActions m_SelectionToolActionsCallbackInterface;
    private readonly InputAction m_SelectionTool_StartSelection;
    public struct SelectionToolActions
    {
        private @Netherlands3DInputActions m_Wrapper;
        public SelectionToolActions(@Netherlands3DInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @StartSelection => m_Wrapper.m_SelectionTool_StartSelection;
        public InputActionMap Get() { return m_Wrapper.m_SelectionTool; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(SelectionToolActions set) { return set.Get(); }
        public void SetCallbacks(ISelectionToolActions instance)
        {
            if (m_Wrapper.m_SelectionToolActionsCallbackInterface != null)
            {
                @StartSelection.started -= m_Wrapper.m_SelectionToolActionsCallbackInterface.OnStartSelection;
                @StartSelection.performed -= m_Wrapper.m_SelectionToolActionsCallbackInterface.OnStartSelection;
                @StartSelection.canceled -= m_Wrapper.m_SelectionToolActionsCallbackInterface.OnStartSelection;
            }
            m_Wrapper.m_SelectionToolActionsCallbackInterface = instance;
            if (instance != null)
            {
                @StartSelection.started += instance.OnStartSelection;
                @StartSelection.performed += instance.OnStartSelection;
                @StartSelection.canceled += instance.OnStartSelection;
            }
        }
    }
    public SelectionToolActions @SelectionTool => new SelectionToolActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    private int m_GamepadSchemeIndex = -1;
    public InputControlScheme GamepadScheme
    {
        get
        {
            if (m_GamepadSchemeIndex == -1) m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
            return asset.controlSchemes[m_GamepadSchemeIndex];
        }
    }
    private int m_TouchSchemeIndex = -1;
    public InputControlScheme TouchScheme
    {
        get
        {
            if (m_TouchSchemeIndex == -1) m_TouchSchemeIndex = asset.FindControlSchemeIndex("Touch");
            return asset.controlSchemes[m_TouchSchemeIndex];
        }
    }
    private int m_JoystickSchemeIndex = -1;
    public InputControlScheme JoystickScheme
    {
        get
        {
            if (m_JoystickSchemeIndex == -1) m_JoystickSchemeIndex = asset.FindControlSchemeIndex("Joystick");
            return asset.controlSchemes[m_JoystickSchemeIndex];
        }
    }
    private int m_XRSchemeIndex = -1;
    public InputControlScheme XRScheme
    {
        get
        {
            if (m_XRSchemeIndex == -1) m_XRSchemeIndex = asset.FindControlSchemeIndex("XR");
            return asset.controlSchemes[m_XRSchemeIndex];
        }
    }
    public interface IGridSelectionActions
    {
        void OnToggleVoxel(InputAction.CallbackContext context);
        void OnDrawVoxels(InputAction.CallbackContext context);
        void OnEraseVoxels(InputAction.CallbackContext context);
    }
    public interface ISelectorActions
    {
        void OnMultiselect(InputAction.CallbackContext context);
        void OnDoubleClick(InputAction.CallbackContext context);
        void OnClickSecondary(InputAction.CallbackContext context);
        void OnClick(InputAction.CallbackContext context);
    }
    public interface IGodViewKeyboardActions
    {
        void OnMoveCamera(InputAction.CallbackContext context);
        void OnMoveCameraHeight(InputAction.CallbackContext context);
        void OnZoom(InputAction.CallbackContext context);
        void OnRotateCamera(InputAction.CallbackContext context);
        void OnFly(InputAction.CallbackContext context);
    }
    public interface IGodViewMouseActions
    {
        void OnZoom(InputAction.CallbackContext context);
        void OnZoomDrag(InputAction.CallbackContext context);
        void OnDrag(InputAction.CallbackContext context);
        void OnSpinDrag(InputAction.CallbackContext context);
        void OnFirstPersonModifier(InputAction.CallbackContext context);
        void OnRotateAroundModifier(InputAction.CallbackContext context);
        void OnPosition(InputAction.CallbackContext context);
    }
    public interface ITransformableActions
    {
        void OnPlace(InputAction.CallbackContext context);
    }
    public interface IPlaceOnClickActions
    {
        void OnPlace(InputAction.CallbackContext context);
    }
    public interface IGizmoHandleActions
    {
        void OnDrag(InputAction.CallbackContext context);
    }
    public interface IStreetViewActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnLook(InputAction.CallbackContext context);
        void OnSelect(InputAction.CallbackContext context);
        void OnOpenMenu(InputAction.CallbackContext context);
    }
    public interface IUIActions
    {
        void OnNavigate(InputAction.CallbackContext context);
        void OnSubmit(InputAction.CallbackContext context);
        void OnCancel(InputAction.CallbackContext context);
        void OnPoint(InputAction.CallbackContext context);
        void OnClick(InputAction.CallbackContext context);
        void OnScrollWheel(InputAction.CallbackContext context);
        void OnMiddleClick(InputAction.CallbackContext context);
        void OnRightClick(InputAction.CallbackContext context);
        void OnTrackedDevicePosition(InputAction.CallbackContext context);
        void OnTrackedDeviceOrientation(InputAction.CallbackContext context);
    }
    public interface ISelectionToolActions
    {
        void OnStartSelection(InputAction.CallbackContext context);
    }
}
