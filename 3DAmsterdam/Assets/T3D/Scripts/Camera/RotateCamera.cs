using Netherlands3D.Cameras;
using UnityEngine;
using Netherlands3D.T3D.Uitbouw;
using Netherlands3D.InputHandler;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using ConvertCoordinates;

public class RotateCamera : MonoBehaviour, ICameraControls
{

    public delegate void FocusPointChanged(Vector3 pointerPosition);
    public static FocusPointChanged focusingOnTargetPoint;

    //public CameraModeT3D CameraMode { get; private set; }

    private float cameraHeightAboveGroundLevel = 15f;
    private Camera myCam;
    public float MinCameraHeight = 4;
    public float RotationSpeed = 0.5f;
    public float ZoomSpeed = 0.01f;
    public float spinSpeed = 60;
    public float MaxCameraDistance = 200;
    
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

    List<InputActionMap> availableActionMaps;

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

        //availableActionMaps = T3D.CameraModeChanger.Instance.AvailableActionMaps;
        availableActionMaps = new List<InputActionMap>()
        {
            ActionHandler.actions.GodViewMouse,
            ActionHandler.actions.GodViewKeyboard
        };

        AddActionListeners();
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

    private void SmoothRotateToCameraTargetPoint()
    {
        var mouseDelta = Mouse.current.delta.ReadValue();
        if (!dragging && !Input.GetMouseButton(0))
        {
            Quaternion targetRotation = Quaternion.LookRotation((CameraTargetPoint - myCam.transform.position).normalized, Vector3.up);
            myCam.transform.rotation = Quaternion.Slerp(myCam.transform.rotation, targetRotation, Time.deltaTime * autoOrientRotateSpeed);
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
        print("subscribing");
        //Mouse actions
        dragActionMouse = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewMouse.Drag);
        zoomScrollActionMouse = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewMouse.Zoom);

        print(dragActionMouse);

        //Listeners
        dragActionMouse.SubscribePerformed(Drag);
        dragActionMouse.SubscribeCancelled(Drag);

        zoomScrollActionMouse.SubscribePerformed(Zoom);
    }

    private void Drag(IAction action)
    {
        print("drag");
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
        print("zoom");
        scrollDelta = ActionHandler.actions.GodViewMouse.Zoom.ReadValue<Vector2>().y;

        if (scrollDelta != 0)
        {
            var lastY = myCam.transform.position.y;
            var moveSpeed = Mathf.Sqrt(myCam.transform.position.y) * 1.3f;

            var newpos = myCam.transform.position + myCam.transform.forward.normalized * (scrollDelta * moveSpeed * ZoomSpeed);

            //if (isFirstPersonMode)
            //{
            //    if (Vector3.Distance(newpos, CoordConvert.RDtoUnity(BuildingCenter)) > MaxFirstPersonDistance) return;
            //    newpos.y = lastY;
            //}
            if (newpos.y < MinCameraHeight) return;
            else if (CameraInRange(newpos) == false) return;

            myCam.transform.position = newpos;
        }
    }

    void RotateAround(float xaxis, float yaxis)
    {
        myCam.transform.RotateAround(CameraTargetPoint, Vector3.up, xaxis * RotationSpeed);
    }

    bool CameraInRange(Vector3 newCameraPosition)
    {
        return Vector3.Distance(CameraTargetPoint, newCameraPosition) < MaxCameraDistance;
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
        return availableActionMaps.Contains(actionMap);
    }
}
