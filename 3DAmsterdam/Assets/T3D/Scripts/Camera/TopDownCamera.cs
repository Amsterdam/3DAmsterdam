using System.Collections;
using System.Collections.Generic;
using ConvertCoordinates;
using Netherlands3D.Cameras;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownCamera : MonoBehaviour, ICameraControls
{
    private Camera myCam;
    [SerializeField]
    private float cameraHeightAboveGroundLevel = 50f;

    private void Awake()
    {
        myCam = GetComponent<Camera>();
    }

    private void Start()
    {
        MetadataLoader.Instance.PerceelDataLoaded += OnPerceelDataLoaded;
    }

    private void OnPerceelDataLoaded(object source, PerceelDataEventArgs args)
    {
        //PerceelCenter = args.Center;
        var perceelCenter = CoordConvert.RDtoUnity(args.Center);
        cameraHeightAboveGroundLevel = args.Radius / Mathf.Tan(myCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        transform.position = new Vector3(perceelCenter.x, T3D.CameraModeChanger.Instance.GroundLevel + cameraHeightAboveGroundLevel, perceelCenter.z);
    }

    public void EnableKeyboardActionMap(bool enabled)
    {
        throw new System.NotImplementedException();
    }

    public void EnableMouseActionMap(bool enabled)
    {
        throw new System.NotImplementedException();
    }

    public float GetCameraHeight()
    {
        throw new System.NotImplementedException();
    }

    public float GetNormalizedCameraHeight()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetPointerPositionInWorld(Vector3 optionalPositionOverride = default)
    {
        throw new System.NotImplementedException();
    }

    public void MoveAndFocusOnLocation(Vector3 targetLocation, Quaternion rotation)
    {
        throw new System.NotImplementedException();
    }

    public void SetNormalizedCameraHeight(float height)
    {
        throw new System.NotImplementedException();
    }

    public bool UsesActionMap(InputActionMap actionMap)
    {
        throw new System.NotImplementedException();
    }
}
