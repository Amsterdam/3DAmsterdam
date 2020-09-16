using Amsterdam3D.CameraMotion;
using BruTile.Wms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

   public class CameraManager:MonoBehaviour
    {


    public ICameraExtents CurrentCameraExtends { get; private set; }
    [SerializeField]
    private GameObject currentCamera;
    public Camera currentCameraComponent { get; private set; }
    private StreetViewMoveToPoint streetView;

    [SerializeField]
    private GameObject GodviewCam;
    

    public CameraMode CameraMode { get; private set; }


    public static CameraManager instance;

    private void Awake()
    {
        streetView = FindObjectOfType<StreetViewMoveToPoint>();
        currentCamera = GodviewCam;
        currentCameraComponent = currentCamera.GetComponent<Camera>();
        CurrentCameraExtends = currentCamera.GetComponent<ICameraExtents>();
        instance = this;
    }


    public void FirstPersonMode(Vector3 position, Quaternion rotation) 
    {
        this.CameraMode = CameraMode.StreetView;

        Vector3 oldPosition = currentCamera.transform.position;
        Quaternion oldRotation = currentCamera.transform.rotation;
        currentCamera.SetActive(false);
        currentCamera = streetView.EnableFPSCam();
        currentCamera.transform.position = oldPosition;
        currentCamera.transform.rotation = oldRotation;
        currentCameraComponent = currentCamera.GetComponent<Camera>();
        CurrentCameraExtends = currentCamera.GetComponent<ICameraExtents>();
        MoveCameraToStreet(currentCamera.transform, position, rotation);
    }


    public void ChangeCamera(GameObject camera) 
    {
        if (currentCamera != null) 
        {
            currentCamera.SetActive(false);
        }
        CurrentCameraExtends = camera.GetComponent<ICameraExtents>();
        currentCamera = camera;
    }

    public void MoveCameraToStreet(Transform cameraTransform, Vector3 position, Quaternion rotation) 
    {
        StartCoroutine(streetView.MoveToPosition(cameraTransform, position, rotation));
    }
}

public enum CameraMode 
{
    GodView,
    StreetView,
    VR
}
