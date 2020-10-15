
using BruTile.Wms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Amsterdam3D.CameraMotion
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

        private void Awake()
        {
            streetView = FindObjectOfType<StreetViewMoveToPoint>();
            currentCamera = godViewCam;
            CurrentCameraControls = currentCamera.GetComponent<ICameraControls>();
            CurrentCameraExtends = currentCamera.GetComponent<ICameraExtents>();
            ActiveCamera = currentCamera.GetComponent<Camera>();
            Instance = this;
        }

        public void FirstPersonMode(Vector3 position, Quaternion rotation)
        {
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
            MoveCameraToStreet(currentCamera.transform, position, rotation);
        }

        public void MoveCameraToStreet(Transform cameraTransform, Vector3 position, Quaternion rotation, bool reverse = false)
        {
            if (reverse)
            {
                StartCoroutine(streetView.MoveToPositionReverse(cameraTransform, position, rotation));
            }
            else
            {
                StartCoroutine(streetView.MoveToPosition(cameraTransform, position, rotation));
            }
        }

        public void GodViewMode()
        {
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
            MoveCameraToStreet(currentCamera.transform, currentPosition + Vector3.up * 400, oldGodViewRotation, true);
        }
    }

    public enum CameraMode
    {
        GodView,
        StreetView,
        VR
    }
}
