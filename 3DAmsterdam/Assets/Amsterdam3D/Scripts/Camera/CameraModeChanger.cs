
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
        [SerializeField]
        private GameObject currentCamera;
        // instead of Camera, make this some kind of interface that both CameraControls.cs and FirstPersonMouselook.cs implement
        public ICameraControls CurrentCameraControlsComponent { get; private set; }
        public Camera CurrentCameraComponent { get; private set; }
        private StreetViewMoveToPoint streetView;

        [SerializeField]
        private GameObject godviewCam;

        //idk if public events in a singleton are the best idea
        // Maybe make these ScriptableObject eventhandlers instead?
        public delegate void OnFirstPersonMode();
        public event OnFirstPersonMode OnFirstPersonModeEvent;


        public delegate void OnGodViewMode();
        public event OnGodViewMode OnGodViewModeEvent;


        private Quaternion oldGodViewRotation;



        public CameraMode CameraMode { get; private set; }


        public static CameraModeChanger instance;

        private void Awake()
        {
            streetView = FindObjectOfType<StreetViewMoveToPoint>();
            currentCamera = godviewCam;
            CurrentCameraControlsComponent = currentCamera.GetComponent<ICameraControls>();
            CurrentCameraExtends = currentCamera.GetComponent<ICameraExtents>();
            CurrentCameraComponent = currentCamera.GetComponent<Camera>();
            instance = this;
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
            CurrentCameraControlsComponent = currentCamera.GetComponent<ICameraControls>();
            CurrentCameraComponent = currentCamera.GetComponent<Camera>();
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
            currentCamera = godviewCam;
            currentCamera.transform.position = currentPosition;
            currentCamera.transform.rotation = rot;
            currentCamera.SetActive(true);
            CurrentCameraExtends = currentCamera.GetComponent<ICameraExtents>();
            CurrentCameraControlsComponent = currentCamera.GetComponent<ICameraControls>();
            CurrentCameraComponent = currentCamera.GetComponent<Camera>();
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
