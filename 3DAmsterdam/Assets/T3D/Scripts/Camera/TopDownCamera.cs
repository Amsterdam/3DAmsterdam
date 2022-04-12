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
    [SerializeField]
    private float cameraHeightAboveGroundLevel = 50f;
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
            MetadataLoader.Instance.PerceelDataLoaded += OnPerceelDataLoaded;
    }

    public void SetCameraStartPosition(Vector3 perceelCenter, float perceelRadius)
    {
        cameraHeightAboveGroundLevel = RestrictionChecker.ActiveBuilding.HeightLevel;//perceelRadius / Mathf.Tan(myCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        transform.position = new Vector3(perceelCenter.x, CameraModeChanger.Instance.GroundLevel + cameraHeightAboveGroundLevel, perceelCenter.z);
        myCam.orthographicSize = perceelRadius;

        var test = new GameObject();
        test.transform.position = RestrictionChecker.ActivePerceel.Center;
    }

    private void OnPerceelDataLoaded(object source, PerceelDataEventArgs args)
    {
        var perceelCenter = CoordConvert.RDtoUnity(args.Center);
        SetCameraStartPosition(perceelCenter, args.Radius);
    }

    public float GetNormalizedCameraHeight()
    {
        return cameraHeightAboveGroundLevel;
    }

    public float GetCameraHeight()
    {
        return transform.position.y;
    }

    public void SetNormalizedCameraHeight(float height)
    {
        transform.position = new Vector3(transform.position.x, height + cameraHeightAboveGroundLevel, transform.position.z);
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
        return CameraModeChanger.Instance.AvailableActionMaps.Contains(actionMap);
    }
}
