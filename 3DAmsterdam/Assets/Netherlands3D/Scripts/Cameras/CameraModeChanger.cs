using Netherlands3D.Core;
using Netherlands3D.Events;
using Netherlands3D.Interface;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Netherlands3D.Cameras
{
    public class CameraModeChanger : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void AutoCursorLock();

        private GameObject currentCamera;

        [SerializeField]
        private StringEvent latLongStringEvent;

        public ICameraControls CurrentCameraControls { get; private set; }
        public Camera ActiveCamera { get; private set; }
        private StreetViewMoveToPoint streetView;

        [SerializeField]
        private GameObject godViewCam;

        public delegate void OnFirstPersonMode();
        public event OnFirstPersonMode OnFirstPersonModeEvent;

        public delegate void OnGodViewMode();
        public event OnGodViewMode OnGodViewModeEvent;

        public delegate void OnOrtographicMode();
        public event OnOrtographicMode OnOrtographicModeEvent;

        public delegate void OnPerspectiveMode();
        public event OnPerspectiveMode OnPerspectiveModeEvent;

        private Quaternion oldGodViewRotation;

        public CameraMode CameraMode { get; private set; }

        public static CameraModeChanger Instance;

        [SerializeField]
        private float groundTransitionOffset = 2.0f;

        private bool ortographic = false;

        private void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            AutoCursorLock();
#endif
            streetView = FindObjectOfType<StreetViewMoveToPoint>();
            currentCamera = godViewCam;
            CurrentCameraControls = currentCamera.GetComponent<ICameraControls>();
            ActiveCamera = currentCamera.GetComponent<Camera>();
            Instance = this;

            if (latLongStringEvent) latLongStringEvent.AddListenerStarted(ChangedPointFromUrl);
        }

        public void BackToFirstPerson()
        {
            FirstPersonMode(currentCamera.transform.position, currentCamera.transform.rotation);
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
            OnFirstPersonModeEvent?.Invoke();

            //Flatten rotation (so end result looks forward instead of down)
            var eulerRotation = rotation.eulerAngles;
            eulerRotation.x = 0;
            rotation = Quaternion.Euler(eulerRotation);

            TransitionCamera(currentCamera.transform, position, rotation);
        }
        public void GodViewMode()
        {
            PointerLock.SetMode(PointerLock.Mode.DEFAULT);

            this.CameraMode = CameraMode.GodView;
            Vector3 currentPosition = currentCamera.transform.position;
            Quaternion rot = currentCamera.transform.rotation;
            Vector3 forward = currentCamera.transform.forward;
            currentCamera.SetActive(false);
            currentCamera = godViewCam;
            currentCamera.transform.position = currentPosition;
            currentCamera.transform.rotation = rot;
            currentCamera.SetActive(true);
            CurrentCameraControls = currentCamera.GetComponent<ICameraControls>();
            ActiveCamera = currentCamera.GetComponent<Camera>();
            OnGodViewModeEvent?.Invoke();

            var lookingDown = Quaternion.LookRotation(Vector3.down, forward);
            TransitionCamera(currentCamera.transform, currentPosition + Vector3.up * 400, lookingDown, true);
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

        public bool TogglePerspective()
        {
            ortographic = !ortographic;
            CurrentCameraControls.ToggleOrtographic(ortographic);

            //Invoke an event, so UI a.o. items can hide/show themselves
			switch (ortographic)
			{
                case true:
                    OnOrtographicModeEvent?.Invoke();
                    break;
                case false:
                    OnPerspectiveModeEvent?.Invoke();
                    break;
            }

            return ortographic;
        }

        /// <summary>
        /// Used from Javascript to move the camera to a specific WGS84 (gps) location based on the hash # in the url
        /// </summary>
        /// <param name="latitudeLongitude">Comma seperated lat,long string</param>
        public void ChangedPointFromUrl(string latitudeLongitude)
        {
            Debug.Log($"Received lat long string: {latitudeLongitude}");
            string[] coordinates = latitudeLongitude.Split(',');
            var latitude = double.Parse(coordinates[0]);
            var longitude = double.Parse(coordinates[1]);

            var convertedCoordinate = CoordConvert.WGS84toUnity(longitude, latitude);
            currentCamera.transform.position = new Vector3(convertedCoordinate.x, Mathf.Max(this.transform.position.y,300), convertedCoordinate.z);
        }
    }

    public enum CameraMode
    {
        GodView,
        StreetView,
        VR
    }
}
