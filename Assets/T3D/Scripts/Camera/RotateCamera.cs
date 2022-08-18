using Netherlands3D.Cameras;
using UnityEngine;
using Netherlands3D.T3D.Uitbouw;
using Netherlands3D.InputHandler;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using ConvertCoordinates;
using System.Collections;

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
    private float rotationSpeed = 5f;
    [SerializeField]
    private float zoomSpeed = 0.01f;
    [SerializeField]
    private float spinSpeed = 60;
    [SerializeField]
    private float maxCameraDistance = 200;
    [SerializeField]
    private float startDistanceFromCenter = 15f;
    [SerializeField]
    private float minimumYangle = 10f;
    [SerializeField]
    private float maximumYangle = 65f;

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
            print("no camera target point found using Vector3.zero");
            return Vector3.zero;
        }
    }

    void Start()
    {
        myCam = GetComponent<Camera>();

        AddActionListeners();
    }

    private void OnEnable()
    {
        StartCoroutine(SetCameraStartPosition());
    }

    private void Update()
    {
        //print("update: snap, waiting :" + RestrictionChecker.ActiveBuilding.BuildingDataIsProcessed + "\t" + (RestrictionChecker.ActivePerceel != null));
        //print("update: no snap, waiting :" + RestrictionChecker.ActiveBuilding.BuildingDataIsProcessed + "\t" + (RestrictionChecker.ActiveUitbouw != null));
        ProcessUserInput();
        SmoothRotateToCameraTargetPoint();
    }

    private void ProcessUserInput()
    {
        if (dragging && Input.GetMouseButton(0))
        {
            var mouseDelta = Mouse.current.delta.ReadValue();
            RotateAround(mouseDelta.x, mouseDelta.y);
        }
    }

    private IEnumerator SetCameraStartPosition()
    {
        //wait until Ground level and building center are known, and  the active uitbouw exists
        Vector3 dir;
        if (ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel == false || ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall)
        {
            yield return new WaitUntil(() => RestrictionChecker.ActiveBuilding.BuildingDataIsProcessed && RestrictionChecker.ActivePerceel != null);
            dir = RestrictionChecker.ActivePerceel.Center - RestrictionChecker.ActiveBuilding.BuildingCenter;
        }
        else
        {
            yield return new WaitUntil(() => RestrictionChecker.ActiveBuilding.BuildingDataIsProcessed && RestrictionChecker.ActiveUitbouw != null);
            dir = RestrictionChecker.ActiveUitbouw.CenterPoint - RestrictionChecker.ActiveBuilding.BuildingCenter;
        }
        dir.y = 0;
        dir.Normalize();

        transform.position = CameraTargetPoint + dir * startDistanceFromCenter;
        SetNormalizedCameraHeight(RestrictionChecker.ActiveBuilding.GroundLevel);
        transform.LookAt(CameraTargetPoint);
    }

    private void SmoothRotateToCameraTargetPoint()
    {
        if (!dragging && !Input.GetMouseButton(0))
        {
            Quaternion targetRotation = Quaternion.LookRotation((CameraTargetPoint - transform.position).normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * autoOrientRotateSpeed);
        }
    }

    private void AddActionListeners()
    {
        //Mouse actions
        dragActionMouse = ServiceLocator.GetService<ActionHandler>().GetAction(ActionHandler.actions.GodViewMouse.Drag);
        zoomScrollActionMouse = ServiceLocator.GetService<ActionHandler>().GetAction(ActionHandler.actions.GodViewMouse.Zoom);

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
        transform.RotateAround(CameraTargetPoint, Vector3.up, xaxis * rotationSpeed * Time.deltaTime);

        var yAngle = Vector3.Angle(Vector3.up, (transform.position - CameraTargetPoint).normalized);
        var deltaYAngle = yaxis * rotationSpeed * Time.deltaTime;

        if (yAngle + deltaYAngle > maximumYangle)
        {
            deltaYAngle = maximumYangle - yAngle;
        }
        else if (yAngle + deltaYAngle < minimumYangle)
        {
            deltaYAngle = minimumYangle - yAngle;
        }

        transform.RotateAround(CameraTargetPoint, transform.right, -deltaYAngle); //use -deltaAngle because this function rotates in the opposite direction for some reason
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
        return ServiceLocator.GetService<CameraModeChanger>().AvailableActionMaps.Contains(actionMap);
    }
}
