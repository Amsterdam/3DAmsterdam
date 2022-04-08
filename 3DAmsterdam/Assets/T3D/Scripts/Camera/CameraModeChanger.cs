using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using Netherlands3D.InputHandler;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Netherlands3D.T3D
{
    public class CameraModeChanger : MonoBehaviour
    {
        public static CameraModeChanger Instance;

        public CameraMode CurrentMode { get; private set; }
        public Camera ActiveCamera { get; private set; }
        public ICameraControls CurrentCameraControls { get; private set; }
        public List<InputActionMap> AvailableActionMaps;

        [SerializeField]
        private Camera[] cameras;
        private Dictionary<CameraMode, Camera> cameraDictionary;

        public float GroundLevel => RestrictionChecker.ActiveBuilding.GroundLevel;

        private void Awake()
        {
            Instance = this;
            AvailableActionMaps = new List<InputActionMap>()
            {
                ActionHandler.actions.GodViewMouse,
                ActionHandler.actions.GodViewKeyboard
            };

            cameraDictionary = new Dictionary<CameraMode, Camera>();
            foreach(Camera cam in cameras)
            {
                cameraDictionary.Add(cam.GetComponent<ICameraControls>().Mode, cam);
            }

            InitializeCameraMode(CameraMode.GodView);
        }

        private void Start()
        {
            RestrictionChecker.ActiveBuilding.BuildingDataProcessed += Instance_BuildingDataProcessed;
        }

        private void Instance_BuildingDataProcessed(BuildingMeshGenerator building)
        {
            CurrentCameraControls.SetNormalizedCameraHeight(building.GroundLevel);
        }

        public void InitializeCameraMode(CameraMode newMode)
        {
            CurrentMode = newMode;
            //ActiveCamera.gameObject.SetActive(false);
            ActiveCamera = cameraDictionary[newMode];
            ActiveCamera.gameObject.SetActive(true);
            CurrentCameraControls = ActiveCamera.GetComponent<ICameraControls>();
            //CurrentCameraControls.SetNormalizedCameraHeight(GroundLevel);
        }

        public void SetCameraMode(CameraMode newMode)
        {
            CurrentMode = newMode;
            ActiveCamera.gameObject.SetActive(false);
            ActiveCamera = cameraDictionary[newMode];
            ActiveCamera.gameObject.SetActive(true);
            CurrentCameraControls = ActiveCamera.GetComponent<ICameraControls>();
            CurrentCameraControls.SetNormalizedCameraHeight(GroundLevel);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
                SetCameraMode(CameraMode.TopDown);
            if (Input.GetKeyDown(KeyCode.R))
                SetCameraMode(CameraMode.GodView);
        }
    }
}