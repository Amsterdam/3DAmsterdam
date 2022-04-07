using System.Collections;
using System.Collections.Generic;
using ConvertCoordinates;
using Netherlands3D.Cameras;
using Netherlands3D.InputHandler;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCamera : MonoBehaviour, ICameraControls
{
    private Camera myCam;
    [SerializeField]
    private float cameraHeightAboveGroundLevel = 1.75f;
    [SerializeField]
    private float moveSpeed = 5f;
    private bool dragging = false;
    [SerializeField]
    private float MaxFirstPersonDistance = 40;
    //[SerializeField]
    //private float firstPersonHeight = 2f;
    [SerializeField]
    private float firstPersonCameraDistance = 5f;
    [SerializeField]
    private float rotationSpeed = 0.5f;
    [SerializeField]
    private float zoomSpeed = 0.01f;

    private float scrollDelta;
    private IAction dragActionMouse;
    private IAction zoomScrollActionMouse;

    //public static Vector3 CameraTargetPoint
    //{
    //    get
    //    {
    //        if (RestrictionChecker.ActiveUitbouw)
    //        {
    //            return RestrictionChecker.ActiveUitbouw.CenterPoint;
    //        }
    //        else if (RestrictionChecker.ActiveBuilding)
    //        {
    //            return RestrictionChecker.ActiveBuilding.BuildingCenter;
    //        }
    //        return Vector3.zero;
    //    }
    //}

    private void Awake()
    {
        myCam = GetComponent<Camera>();
    }
    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    private void Update()
    {
        if (dragging && Input.GetMouseButton(0))
        {
            var mouseDelta = Mouse.current.delta.ReadValue();
            myCam.transform.RotateAround(myCam.transform.position, myCam.transform.right, -mouseDelta.y * rotationSpeed);
            myCam.transform.RotateAround(myCam.transform.position, Vector3.up, mouseDelta.x * rotationSpeed);
        }
        HandleFirstPersonKeyboard();
    }

    private void HandleFirstPersonKeyboard()
    {
        var buildingCenter = CoordConvert.RDtoUnity(RestrictionChecker.ActiveBuilding.BuildingCenter);

        Vector3? newpos = null;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            newpos = myCam.transform.position - myCam.transform.right * moveSpeed / 5 * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            newpos = myCam.transform.position + myCam.transform.right * moveSpeed / 5 * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            newpos = myCam.transform.position + myCam.transform.forward * moveSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            newpos = myCam.transform.position - myCam.transform.forward * moveSpeed * Time.deltaTime;
        }

        if (newpos == null || Vector3.Distance(newpos.Value, buildingCenter) > MaxFirstPersonDistance) return;

        newpos = new Vector3(newpos.Value.x, T3D.CameraModeChanger.Instance.GroundLevel + cameraHeightAboveGroundLevel, newpos.Value.z);
        myCam.transform.position = newpos.Value;
    }

    //private void OnBuildingDataProcessed(BuildingMeshGenerator building)
    //{
    //    groundLevel = building.GroundLevel;
    //    myCam.transform.position = new Vector3(myCam.transform.position.x, T3D.CameraModeChanger.Instance.GroundLevel + cameraHeightAboveGroundLevel, myCam.transform.position.z);
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
            var lastY = myCam.transform.position.y;
            var moveSpeed = Mathf.Sqrt(myCam.transform.position.y) * 1.3f;

            var newpos = myCam.transform.position + myCam.transform.forward.normalized * (scrollDelta * moveSpeed * zoomSpeed);

            if (Vector3.Distance(newpos, CoordConvert.RDtoUnity(RestrictionChecker.ActiveBuilding.BuildingCenter)) > MaxFirstPersonDistance) return;
            newpos.y = lastY;

            myCam.transform.position = newpos;
        }
    }

    //void RotateAround(float xaxis, float yaxis)
    //{
    //    myCam.transform.RotateAround(CameraTargetPoint, Vector3.up, xaxis * RotationSpeed);
    //}

    //bool CameraInRange(Vector3 newCameraPosition)
    //{
    //    return Vector3.Distance(CameraTargetPoint, newCameraPosition) < MaxCameraDistance;
    //}

    //Interface methods
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
        print("nwe height " + height + " " + cameraHeightAboveGroundLevel);
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

    }

    public void EnableMouseActionMap(bool enabled)
    {

    }

    public bool UsesActionMap(InputActionMap actionMap)
    {
        return T3D.CameraModeChanger.Instance.AvailableActionMaps.Contains(actionMap);
    }
}
