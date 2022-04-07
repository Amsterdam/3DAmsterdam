using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using Netherlands3D.InputHandler;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.InputSystem;

namespace T3D
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
            for (int i = 0; i < cameras.Length; i++)
            {
                cameraDictionary.Add((CameraMode)i, cameras[i]);
            }

            InitializeCameraMode(CameraMode.GodView);
        }

        private void Start()
        {
            BuildingMeshGenerator.Instance.BuildingDataProcessed += Instance_BuildingDataProcessed;
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

        //public bool ToggleRotateFirstPersonMode()
        //{
        //    isFirstPersonMode = !isFirstPersonMode;

        //    if (isFirstPersonMode)
        //    {
        //        lastRotatePosition = myCam.transform.position;
        //        lastRotateRotation = myCam.transform.rotation;

        //        var perceelCenter = CoordConvert.RDtoUnity(PerceelCenter);
        //        var buildingCenter = CoordConvert.RDtoUnity(BuildingCenter);
        //        var cameraoffset = (perceelCenter - buildingCenter).normalized * (BuildingRadius + firstPersonCameraDistance);

        //        myCam.transform.position = new Vector3(buildingCenter.x + cameraoffset.x, groundLevel + firstPersonHeight, buildingCenter.z + cameraoffset.z);
        //        myCam.transform.LookAt(CameraTargetPoint);

        //        //currentRotation = new Vector2(mycam.transform.rotation.eulerAngles.y, mycam.transform.rotation.eulerAngles.x);
        //    }
        //    else
        //    {
        //        myCam.transform.position = lastRotatePosition;
        //        myCam.transform.rotation = lastRotateRotation;
        //    }

        //    return isFirstPersonMode;
        //}
    }
}