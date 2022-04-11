using ConvertCoordinates;
using Netherlands3D.Interface;
using Netherlands3D.Interface.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Netherlands3D.Cameras
{
    public class CameraModeChanger : MonoBehaviour
    {
        public ICameraExtents CurrentCameraExtends { get; private set; }

        private GameObject currentCamera;

        public ICameraControls CurrentCameraControls { get; private set; }
        public Camera ActiveCamera { get; private set; }
        private StreetViewMoveToPoint streetView;

        [SerializeField]
        private GameObject godViewCam;

        public delegate void OnFirstPersonMode();
        public event OnFirstPersonMode OnFirstPersonModeEvent;

        public delegate void OnGodViewMode();
        public event OnGodViewMode OnGodViewModeEvent;

        private Quaternion oldGodViewRotation;

        public CameraMode CameraMode { get; private set; }

        public static CameraModeChanger Instance;

        [SerializeField]
        private float groundTransitionOffset = 2.0f;

        private void Awake()
        {
            streetView = FindObjectOfType<StreetViewMoveToPoint>();
            currentCamera = godViewCam;
            CurrentCameraControls = currentCamera.GetComponent<ICameraControls>();
            CurrentCameraExtends = currentCamera.GetComponent<ICameraExtents>();
            ActiveCamera = currentCamera.GetComponent<Camera>();
            Instance = this;
        }
        private void Start()
        {
            
        }

        public void FirstPersonMode(Vector3 position, Quaternion rotation)
        {
            PointerLock.SetMode(PointerLock.Mode.FIRST_PERSON);

            var gridSelection = FindObjectOfType<GridSelection>();
            if (gridSelection)
                gridSelection.gameObject.SetActive(false);

            this.CameraMode = CameraMode.StreetView;

            Vector3 oldPosition = currentCamera.transform.position;
            Quaternion oldRotation = currentCamera.transform.rotation;
            oldGodViewRotation = currentCamera.transform.rotation;
            currentCamera.SetActive(false);

            currentCamera = streetView.EnableFPSCam();
            currentCamera.transform.position = oldPosition;
            currentCamera.transform.rotation = oldRotation;
            CurrentCameraControls = currentCamera.GetComponent<ICameraControls>();
            ActiveCamera = currentCamera.GetComponent<Camera>();
            CurrentCameraExtends = currentCamera.GetComponent<ICameraExtents>();
            OnFirstPersonModeEvent?.Invoke();
            TransitionCamera(currentCamera.transform, position, rotation);
        }

        public void TransitionCamera(Transform cameraTransform, Vector3 position, Quaternion rotation, bool reverse = false)
        {
            if (reverse)
            {
                StartCoroutine(streetView.MoveToPositionReverse(cameraTransform, position, rotation));
            }
            else
            {
                StartCoroutine(streetView.MoveToPosition(cameraTransform, position + Vector3.up * groundTransitionOffset, rotation));
            }
        }

        /// <summary>
        /// Used from Javascript to move the camera to a specific WGS84 (gps) location based on the hash # in the url
        /// </summary>
        /// <param name="latitudeLongitude">Comma seperated lat,long string</param>
        public void ChangedPointFromUrl(string latitudeLongitude)
        {
            string[] coordinates = latitudeLongitude.Split(',');
            var latitude = double.Parse(coordinates[0]);
            var longitude = double.Parse(coordinates[1]);

            var convertedCoordinate = CoordConvert.WGS84toUnity(longitude, latitude);
            currentCamera.transform.position = new Vector3(convertedCoordinate.x, this.transform.position.y, convertedCoordinate.z);
        }

        public void GodViewMode()
        {
            PointerLock.SetMode(PointerLock.Mode.DEFAULT);

            this.CameraMode = CameraMode.GodView;
            Vector3 currentPosition = currentCamera.transform.position;
            Quaternion rot = currentCamera.transform.rotation;
            currentCamera.SetActive(false);
            currentCamera = godViewCam;
            currentCamera.transform.position = currentPosition;
            currentCamera.transform.rotation = rot;
            currentCamera.SetActive(true);
            CurrentCameraExtends = currentCamera.GetComponent<ICameraExtents>();
            CurrentCameraControls = currentCamera.GetComponent<ICameraControls>();
            ActiveCamera = currentCamera.GetComponent<Camera>();
            OnGodViewModeEvent?.Invoke();
            TransitionCamera(currentCamera.transform, currentPosition + Vector3.up * 400, oldGodViewRotation, true);
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
