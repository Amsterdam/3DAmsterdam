using Netherlands3D.Cameras;
using UnityEngine;
using Netherlands3D.T3D.Uitbouw;
using Netherlands3D.InputHandler;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using ConvertCoordinates;
using CameraModeChanger = Netherlands3D.T3D.CameraModeChanger;

public class RotateCamera : MonoBehaviour, ICameraControls
{

    public delegate void FocusPointChanged(Vector3 pointerPosition);

    public static FocusPointChanged focusingOnTargetPoint;

    //public CameraModeT3D CameraMode { get; private set; }

    private float cameraHeightAboveGroundLevel = 15f;
    private Camera myCam;
    [SerializeField]
    private float minCameraHeight = 4;
    [SerializeField]
    private float rotationSpeed = 0.5f;
    [SerializeField]
    private float zoomSpeed = 0.01f;
    [SerializeField]
    private float spinSpeed = 60;
    [SerializeField]
    private float maxCameraDistance = 200;
    [SerializeField]
    private float startDistanceFromCenter = 15f;

    //[SerializeField]
    private bool dragging = false;

    private float scrollDelta;

    private IAction dragActionMouse;
    private IAction zoomScrollActionMouse;

    //List<InputActionMap> availableActionMaps;

    //Vector3 lastRotatePosition;
    //Quaternion lastRotateRotation;

    //Vector2RD PerceelCenter;
    //Vector2RD BuildingCenter;
    //float BuildingRadius;

    [SerializeField]
    private float autoOrientRotateSpeed = 10f;

    //float groundLevel;

    //public static RotateCamera Instance;
    //bool isFirstPersonMode = false;

    public static Vector3 CameraTargetPoint
    {
        get
        {
            if (RestrictionChecker.ActiveUitbouw)
            {
                return RestrictionChecker.ActiveUitbouw.CenterPoint;
            }
            else if (RestrictionChecker.ActiveBuilding)
            {
                return RestrictionChecker.ActiveBuilding.BuildingCenter;
            }
            return Vector3.zero;
        }
    }

    //private void Awake()
    //{
    //    Instance = this;
    //}

    void Start()
    {
        //BuildingMeshGenerator.Instance.BuildingDataProcessed += OnBuildingDataProcessed;
        //MetadataLoader.Instance.BuildingOutlineLoaded += OnBuildingOutlineLoaded;

        //mycam = CameraModeChanger.Instance.ActiveCamera;
        myCam = GetComponent<Camera>();

        AddActionListeners();
        BuildingMeshGenerator.Instance.BuildingDataProcessed += Instance_BuildingDataProcessed;
    }

    private void Instance_BuildingDataProcessed(BuildingMeshGenerator building)
    {
        print(RestrictionChecker.ActivePerceel.Center);
        SetCameraStartPosition(building.GroundLevel);
    }

    //private void OnBuildingOutlineLoaded(object source, BuildingOutlineEventArgs args)
    //{
    //    BuildingCenter = args.Center;
    //    BuildingRadius = args.Radius;
    //}

    private void Update()
    {
        if (StateSaver.Instance.ActiveStateIndex > 2)
        {
            SmoothRotateToCameraTargetPoint();
        }
    }

    public void SetCameraStartPosition(float groundLevel)
    {
        var dir = (RestrictionChecker.ActivePerceel.Center - RestrictionChecker.ActiveBuilding.BuildingCenter).normalized;
        dir.y = 0;
        transform.position = CameraTargetPoint + dir * startDistanceFromCenter;
        SetNormalizedCameraHeight(groundLevel);
        //myCam.transform.Translate(0, RestrictionChecker.ActiveBuilding.GroundLevel + cameraHeightAboveGroundLevel, 0, Space.World);
        transform.LookAt(CameraTargetPoint);
    }

    private void SmoothRotateToCameraTargetPoint()
    {
        var mouseDelta = Mouse.current.delta.ReadValue();
        if (!dragging && !Input.GetMouseButton(0))
        {
            Quaternion targetRotation = Quaternion.LookRotation((CameraTargetPoint - transform.position).normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * autoOrientRotateSpeed);
            //mycam.transform.LookAt(CameraTargetPoint, Vector3.up);
        }

        if (dragging && Input.GetMouseButton(0))
        {
            RotateAround(mouseDelta.x, mouseDelta.y);
        }
    }

    //private void OnBuildingDataProcessed(BuildingMeshGenerator building)
    //{
    //    groundLevel = building.GroundLevel;
    //    myCam.transform.position = new Vector3(myCam.transform.position.x, groundLevel + cameraHeightAboveGroundLevel, myCam.transform.position.z);
    //}

    private void AddActionListeners()
    {
        //Mouse actions
        dragActionMouse = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewMouse.Drag);
        zoomScrollActionMouse = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewMouse.Zoom);

        //Listeners
        dragActionMouse.SubscribePerformed(Drag);
        dragActionMouse.SubscribeCancelled(Drag);

        zoomScrollActionMouse.SubscribePerformed(Zoom);
    }

    private void Drag(IAction action)
    {
        if (action.Cancelled)
        {
            dragging = false;
        }
        else if (action.Performed)
        {
            dragging = true;
        }
    }

    private void Zoom(IAction action)
    {
        scrollDelta = ActionHandler.actions.GodViewMouse.Zoom.ReadValue<Vector2>().y;

        if (scrollDelta != 0)
        {
            //var lastY = myCam.transform.position.y;
            var moveSpeed = Mathf.Sqrt(transform.position.y) * 1.3f;

            var newpos = transform.position + transform.forward.normalized * (scrollDelta * moveSpeed * zoomSpeed);

            //if (isFirstPersonMode)
            //{
            //    if (Vector3.Distance(newpos, CoordConvert.RDtoUnity(BuildingCenter)) > MaxFirstPersonDistance) return;
            //    newpos.y = lastY;
            //}
            if (newpos.y < minCameraHeight) return;
            else if (CameraInRange(newpos) == false) return;

            transform.position = newpos;
        }
    }

    void RotateAround(float xaxis, float yaxis)
    {
        transform.RotateAround(CameraTargetPoint, Vector3.up, xaxis * rotationSpeed);
        //myCam.transform.RotateAround(CameraTargetPoint, transform.right, -yaxis * rotationSpeed);
    }

    bool CameraInRange(Vector3 newCameraPosition)
    {
        return Vector3.Distance(CameraTargetPoint, newCameraPosition) < maxCameraDistance;
    }

    // Interface methods
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
