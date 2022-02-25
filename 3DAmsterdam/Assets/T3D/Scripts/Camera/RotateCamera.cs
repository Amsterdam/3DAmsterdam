using Netherlands3D.Cameras;
using UnityEngine;
using Netherlands3D.T3D.Uitbouw;
using Netherlands3D.InputHandler;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using ConvertCoordinates;

public class RotateCamera : MonoBehaviour, ICameraControls
{
    private Camera mycam;
    public float MinCameraHeight = 4;
    public float RotationSpeed = 0.5f;
    public float ZoomSpeed = 0.01f;
    public float spinSpeed = 60;
    public float moveSpeed = 5f;
    public float firstPersonHeight = 2f;
    public float firstPersonCameraDistance = 5f;
    public float MaxCameraDistance = 200;
    public float MaxFirstPersonDistance = 40;

    [SerializeField]
    private bool dragging = false;

    private float scrollDelta;

    private IAction dragActionMouse;
    private IAction zoomScrollActionMouse;

    List<InputActionMap> availableActionMaps;

    Vector3 lastRotatePosition;
    Quaternion lastRotateRotation;
    Vector2 currentRotation;

    Vector2RD PerceelCenter;
    Vector2RD BuildingCenter;
    float BuildingRadius;


    float groundLevel;

    public static RotateCamera Instance;
    bool isFirstPersonMode = false;

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

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        MetadataLoader.Instance.PerceelDataLoaded += OnPerceelDataLoaded;
        BuildingMeshGenerator.Instance.BuildingDataProcessed += OnBuildingDataProcessed;
        MetadataLoader.Instance.BuildingOutlineLoaded += OnBuildingOutlineLoaded;

        mycam = CameraModeChanger.Instance.ActiveCamera;

        availableActionMaps = new List<InputActionMap>()
        {
            ActionHandler.actions.GodViewMouse,
            ActionHandler.actions.GodViewKeyboard
        };

        AddActionListeners();
    }

    private void OnBuildingOutlineLoaded(object source, BuildingOutlineEventArgs args)
    {
        BuildingCenter = args.Center;
        BuildingRadius = args.Radius;
    }

    private void OnPerceelDataLoaded(object source, PerceelDataEventArgs args)
    {
        PerceelCenter = args.Center;
    }

    private void Update()
    {
        var mouseDelta = Mouse.current.delta.ReadValue();
        if (dragging && Input.GetMouseButton(0) && isFirstPersonMode == false)
        {
            RotateAround(mouseDelta.x, mouseDelta.y);
        }
        else if (isFirstPersonMode)
        {
            if (dragging && Input.GetMouseButton(0))
            {
                mycam.transform.RotateAround(mycam.transform.position, mycam.transform.right, -mouseDelta.y * RotationSpeed);
                mycam.transform.RotateAround(mycam.transform.position, Vector3.up, mouseDelta.x * RotationSpeed);
            }
            HandleFirstPersonKeyboard();
        }
    }

    private void OnBuildingDataProcessed(BuildingMeshGenerator building)
    {
        groundLevel = building.GroundLevel;
    }

    public bool ToggleRotateFirstPersonMode()
    {
        isFirstPersonMode = !isFirstPersonMode;

        if (isFirstPersonMode)
        {
            lastRotatePosition = mycam.transform.position;
            lastRotateRotation = mycam.transform.rotation;

            var perceelCenter = CoordConvert.RDtoUnity(PerceelCenter);
            var buildingCenter = CoordConvert.RDtoUnity(BuildingCenter);
            var cameraoffset = (perceelCenter - buildingCenter).normalized * (BuildingRadius + firstPersonCameraDistance);

            mycam.transform.position = new Vector3(buildingCenter.x + cameraoffset.x, groundLevel + firstPersonHeight, buildingCenter.z + cameraoffset.z);

            if (RestrictionChecker.ActiveUitbouw == null)
            {
                mycam.transform.LookAt(buildingCenter);
            }
            else
            {
                mycam.transform.LookAt(new Vector3(RestrictionChecker.ActiveUitbouw.CenterPoint.x, RestrictionChecker.ActiveUitbouw.CenterPoint.y, RestrictionChecker.ActiveUitbouw.CenterPoint.z));
            }

            currentRotation = new Vector2(mycam.transform.rotation.eulerAngles.y, mycam.transform.rotation.eulerAngles.x);
        }
        else
        {
            mycam.transform.position = lastRotatePosition;
            mycam.transform.rotation = lastRotateRotation;
        }

        return isFirstPersonMode;
    }


    private void HandleFirstPersonKeyboard()
    {
        var buildingCenter = CoordConvert.RDtoUnity(BuildingCenter);

        Vector3? newpos = null;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            newpos = mycam.transform.position - mycam.transform.right * moveSpeed / 5 * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            newpos = mycam.transform.position + mycam.transform.right * moveSpeed / 5 * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            newpos = mycam.transform.position + mycam.transform.forward * moveSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            newpos = mycam.transform.position - mycam.transform.forward * moveSpeed * Time.deltaTime;
        }

        if (newpos == null || Vector3.Distance(newpos.Value, buildingCenter) > MaxFirstPersonDistance) return;

        newpos = new Vector3(newpos.Value.x, groundLevel + firstPersonHeight, newpos.Value.z);
        mycam.transform.position = newpos.Value;
    }

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
            var lastY = mycam.transform.position.y;
            var moveSpeed = Mathf.Sqrt(mycam.transform.position.y) * 1.3f;

            var newpos = mycam.transform.position + mycam.transform.forward.normalized * (scrollDelta * moveSpeed * ZoomSpeed);

            if (isFirstPersonMode)
            {
                if (Vector3.Distance(newpos, CoordConvert.RDtoUnity(BuildingCenter)) > MaxFirstPersonDistance) return;
                newpos.y = lastY;
            }
            else if (newpos.y < MinCameraHeight) return;
            else if (CameraInRange(newpos) == false) return;

            mycam.transform.position = newpos;
        }
    }

    void RotateAround(float xaxis, float yaxis)
    {
        mycam.transform.RotateAround(RestrictionChecker.ActiveUitbouw.CenterPoint, Vector3.up, xaxis * RotationSpeed);

        //if (RestrictionChecker.ActiveUitbouw != null)
        //{
        //    //mycam.transform.RotateAround(RestrictionChecker.ActiveUitbouw.CenterPoint, mycam.transform.right, -yaxis * RotationSpeed);
        //    mycam.transform.RotateAround(RestrictionChecker.ActiveUitbouw.CenterPoint, Vector3.up, xaxis * RotationSpeed);
        //} 
        //else if (RestrictionChecker.ActiveBuilding)
        //    mycam.transform.RotateAround(RestrictionChecker.ActiveBuilding.BuildingCenter, Vector3.up, xaxis * RotationSpeed);        
    }

    bool CameraInRange(Vector3 newCameraPosition)
    {
        return Vector3.Distance(CameraTargetPoint, newCameraPosition) < MaxCameraDistance;

        //if (RestrictionChecker.ActiveUitbouw != null)
        //{
        //    return Vector3.Distance(RestrictionChecker.ActiveUitbouw.CenterPoint, newCameraPosition) < MaxCameraDistance;
        //}
        //else if (RestrictionChecker.ActiveBuilding)
        //{
        //    return Vector3.Distance(RestrictionChecker.ActiveBuilding.BuildingCenter, newCameraPosition) < MaxCameraDistance;
        //}
        //return false;
    }

    public float GetNormalizedCameraHeight()
    {
        return 0;
    }

    public float GetCameraHeight()
    {
        return 0;
    }

    public void SetNormalizedCameraHeight(float height)
    {

    }

    public void MoveAndFocusOnLocation(Vector3 targetLocation, Quaternion rotation)
    {

    }

    public Vector3 GetPointerPositionInWorld(Vector3 optionalPositionOverride = default)
    {
        return Vector3.zero;
    }

    public void EnableKeyboardActionMap(bool enabled)
    {

    }

    public void EnableMouseActionMap(bool enabled)
    {

    }

    public bool UsesActionMap(InputActionMap actionMap)
    {
        return availableActionMaps.Contains(actionMap);
    }
}
