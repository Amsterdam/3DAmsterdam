using System.Collections;
using System.Collections.Generic;
using ConvertCoordinates;
using Netherlands3D.Cameras;
using Netherlands3D.InputHandler;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownCamera : MonoBehaviour, ICameraControls
{
    private Camera myCam;
    //[SerializeField]
    private float cameraHeight = 10000f;
    public CameraMode Mode => CameraMode.TopDown;

    private void Awake()
    {
        myCam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (RestrictionChecker.ActivePerceel.IsLoaded)
            SetCameraStartPosition(RestrictionChecker.ActivePerceel.Center, RestrictionChecker.ActivePerceel.Radius);
        else
            ServiceLocator.GetService<MetadataLoader>().PerceelDataLoaded += OnPerceelDataLoaded;
    }

    public void SetCameraStartPosition(Vector3 perceelCenter, float perceelRadius)
    {
        cameraHeight = RestrictionChecker.ActiveBuilding.HeightLevel + 1; // add 1 to ensure no clipping occurs due to being exactly at height level
        //perceelRadius / Mathf.Tan(myCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        transform.position = new Vector3(perceelCenter.x, cameraHeight, perceelCenter.z);
        myCam.orthographicSize = perceelRadius;
    }

    private void OnPerceelDataLoaded(object source, PerceelDataEventArgs args)
    {
        var perceelCenter = CoordConvert.RDtoUnity(args.Center);
        SetCameraStartPosition(perceelCenter, args.Radius);
    }

    public float GetNormalizedCameraHeight()
    {
        return cameraHeight - RestrictionChecker.ActiveBuilding.GroundLevel;
    }

    public float GetCameraHeight()
    {
        return cameraHeight;
    }

    public void SetNormalizedCameraHeight(float height)
    {
        cameraHeight = RestrictionChecker.ActiveBuilding.HeightLevel + 1;
        transform.position = new Vector3(transform.position.x, cameraHeight, transform.position.z);
    }

    public void MoveAndFocusOnLocation(Vector3 targetLocation, Quaternion rotation)
    {

    }

    public Vector3 GetPointerPositionInWorld(Vector3 optionalPositionOverride = default)
    {
        return myCam.ScreenToWorldPoint(Input.mousePosition); //todo
    }

    public void EnableKeyboardActionMap(bool enabled)
    {
        if (enabled && !ActionHandler.actions.GodViewKeyboard.enabled)
        {
            ActionHandler.actions.GodViewKeyboard.Enable();
        }
        else if (!enabled && ActionHandler.actions.GodViewKeyboard.enabled)
        {
            ActionHandler.actions.GodViewKeyboard.Disable();
        }
    }

    public void EnableMouseActionMap(bool enabled)
    {
        //Wordt aangeroepen vanuit Selector.cs functie EnableCameraActionMaps
        if (enabled && !ActionHandler.actions.GodViewMouse.enabled)
        {
            ActionHandler.actions.GodViewMouse.Enable();
        }
        else if (!enabled && ActionHandler.actions.GodViewMouse.enabled)
        {
            ActionHandler.actions.GodViewMouse.Disable();
        }
    }

    public bool UsesActionMap(InputActionMap actionMap)
    {
        return ServiceLocator.GetService<CameraModeChanger>().AvailableActionMaps.Contains(actionMap);
    }
}
