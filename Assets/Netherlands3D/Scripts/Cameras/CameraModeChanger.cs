using ConvertCoordinates;
using Netherlands3D.InputHandler;
using Netherlands3D.Interface;
using Netherlands3D.T3D.Uitbouw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Netherlands3D.Cameras
{
    public class CameraModeChanger : MonoBehaviour, IUniqueService
    {
        public CameraMode CurrentMode { get; private set; }
        public Camera ActiveCamera { get; private set; }
        public ICameraControls CurrentCameraControls { get; private set; }
        public List<InputActionMap> AvailableActionMaps;

        [SerializeField]
        private Camera[] cameras;
        private Dictionary<CameraMode, Camera> cameraDictionary;

        public delegate void OnCameraModeChangedEventHandler(object source, CameraMode newMode);
        public event OnCameraModeChangedEventHandler CameraModeChangedEvent;

        public float GroundLevel => RestrictionChecker.ActiveBuilding.GroundLevel;

        public ICameraExtents CurrentCameraExtends { get; private set; }

        private void Awake()
        {
            AvailableActionMaps = new List<InputActionMap>()
            {
                ActionHandler.actions.GodViewMouse,
                ActionHandler.actions.GodViewKeyboard
            };

            cameraDictionary = new Dictionary<CameraMode, Camera>();
            foreach (Camera cam in cameras)
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
            CurrentCameraExtends = ActiveCamera.GetComponent<ICameraExtents>();
            CurrentCameraControls.SetNormalizedCameraHeight(GroundLevel);

            CameraModeChangedEvent?.Invoke(this, newMode);
        }
    }

    public enum CameraMode
    {
        GodView,
        StreetView,
        VR,
        TopDown
    }
}
