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
    [SerializeField]
    private float firstPersonCameraDistance = 5f;
    [SerializeField]
    private float rotationSpeed = 0.5f;
    private IAction dragActionMouse;

    private Vector2 buildingCenter;

    public CameraMode Mode => CameraMode.StreetView;

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


    private void Awake()
    {
        myCam = GetComponent<Camera>();

        buildingCenter = new Vector2(RestrictionChecker.ActiveBuilding.BuildingCenter.x, RestrictionChecker.ActiveBuilding.BuildingCenter.z);

        var deltaPos = new Vector2(transform.forward.x, transform.forward.z).normalized * Input.GetAxis("Vertical") + new Vector2(transform.right.x, transform.right.z).normalized * Input.GetAxis("Horizontal");
        deltaPos *= moveSpeed * Time.deltaTime;
        var newPos = new Vector2(transform.position.x, transform.position.z) + deltaPos;

        transform.position = new Vector3(newPos.x, transform.position.y, newPos.y);

    }

    private void OnEnable()
    {
        StartCoroutine( SetCameraStartPosition() );
    }

    private void Start()
    {
        AddActionListeners();
    }

    private void Update()
    {
        if (dragging && Input.GetMouseButton(0))
        {
            var mouseDelta = Mouse.current.delta.ReadValue();
            transform.RotateAround(transform.position, transform.right, -mouseDelta.y * rotationSpeed);
            transform.RotateAround(transform.position, Vector3.up, mouseDelta.x * rotationSpeed);
        }
        HandleFirstPersonKeyboard();
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

        dir.Normalize();

        var newpos = CameraTargetPoint + (dir * firstPersonCameraDistance);
        transform.position = new Vector3(newpos.x, transform.position.y, newpos.z);
       
        transform.LookAt(CameraTargetPoint);
    }

    private void HandleFirstPersonKeyboard()
    {

        var deltaPos = new Vector2(transform.forward.x, transform.forward.z).normalized * Input.GetAxis("Vertical") + new Vector2(transform.right.x, transform.right.z).normalized * Input.GetAxis("Horizontal");
        deltaPos *= moveSpeed * Time.deltaTime;
        var newPos = new Vector2(transform.position.x, transform.position.z) + deltaPos;

        var newDist = Vector2.Distance(newPos, buildingCenter);
        if (newDist > MaxFirstPersonDistance)
        {
            Vector2 fromOriginToObject = newPos - buildingCenter;
            fromOriginToObject *= MaxFirstPersonDistance / newDist;
            newPos = buildingCenter + fromOriginToObject;
        }
        transform.position = new Vector3(newPos.x, transform.position.y, newPos.y);
    }

    private void AddActionListeners()
    {
        //Mouse actions
        dragActionMouse = ServiceLocator.GetService<ActionHandler>().GetAction(ActionHandler.actions.GodViewMouse.Drag);

        //Listeners
        dragActionMouse.SubscribePerformed(Drag);
        dragActionMouse.SubscribeCancelled(Drag);
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
