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

    private bool dragging = false;
    private float scrollDelta;

    private IAction dragActionMouse;
    private IAction zoomScrollActionMouse;

    [SerializeField]
    private float autoOrientRotateSpeed = 10f;
    public CameraMode Mode => CameraMode.GodView;

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

    void Start()
    {
        myCam = GetComponent<Camera>();

        AddActionListeners();
        RestrictionChecker.ActiveBuilding.BuildingDataProcessed += Instance_BuildingDataProcessed;
    }

    private void Instance_BuildingDataProcessed(BuildingMeshGenerator building)
    {
        SetCameraStartPosition(building.GroundLevel);
    }

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
        transform.LookAt(CameraTargetPoint);
    }

    private void SmoothRotateToCameraTargetPoint()
    {
        var mouseDelta = Mouse.current.delta.ReadValue();
        if (!dragging && !Input.GetMouseButton(0))
        {
            Quaternion targetRotation = Quaternion.LookRotation((CameraTargetPoint - transform.position).normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * autoOrientRotateSpeed);
        }

        if (dragging && Input.GetMouseButton(0))
        {
            RotateAround(mouseDelta.x, mouseDelta.y);
        }
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
            //var lastY = myCam.transform.position.y;
            var moveSpeed = Mathf.Sqrt(transform.position.y) * 1.3f;

            var newpos = transform.position + transform.forward.normalized * (scrollDelta * moveSpeed * zoomSpeed);

            if (newpos.y < minCameraHeight) return;
            else if (CameraInRange(newpos) == false) return;

            transform.position = newpos;
        }
    }

    void RotateAround(float xaxis, float yaxis)
    {
        transform.RotateAround(CameraTargetPoint, Vector3.up, xaxis * rotationSpeed);
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
