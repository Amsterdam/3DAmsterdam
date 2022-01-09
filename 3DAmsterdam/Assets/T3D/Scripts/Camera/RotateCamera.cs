using Netherlands3D.Cameras;
using UnityEngine;
using Netherlands3D.T3D.Uitbouw;
using Netherlands3D.InputHandler;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class RotateCamera : MonoBehaviour, ICameraControls
{
    private Camera mycam;
    public float MinCameraHeight = 4;
    public float RotationSpeed = 0.5f;
    public float ZoomSpeed = 0.01f;
    public float spinSpeed = 60;
    public float moveSpeed = 5f;
    public float firstPersonHeight = 3.23f;

    [SerializeField]
    private bool dragging = false;
    
    private float scrollDelta;

    private IAction dragActionMouse;
    private IAction zoomScrollActionMouse;
    
    List<InputActionMap> availableActionMaps;

    Vector3 lastRotatePosition;
    Quaternion lastRotateRotation;
    Vector2 currentRotation;

    public static RotateCamera Instance;
    bool isFirstPersonMode = false;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mycam = CameraModeChanger.Instance.ActiveCamera;

        availableActionMaps = new List<InputActionMap>()
        {
            ActionHandler.actions.GodViewMouse,
            ActionHandler.actions.GodViewKeyboard
        };
        
        AddActionListeners();
    }

    private void Update()
    {        
        var mouseDelta = Mouse.current.delta.ReadValue();
        if (dragging && Input.GetMouseButton(0) && isFirstPersonMode == false)
        {            
            RotateAround(mouseDelta.x, mouseDelta.y);
        }        
        else if(isFirstPersonMode)
        {
            if (dragging && Input.GetMouseButton(0))
            {
                mycam.transform.RotateAround(mycam.transform.position, mycam.transform.right, -mouseDelta.y * RotationSpeed);
                mycam.transform.RotateAround(mycam.transform.position, Vector3.up, mouseDelta.x * RotationSpeed);
            }
            FirstPersonLook();
        }
    }

    public bool ToggleRotateFirstPersonMode()
    {
        if (RestrictionChecker.ActiveUitbouw == null) return false;

        isFirstPersonMode = !isFirstPersonMode;

        if (isFirstPersonMode)        
        {
            lastRotatePosition = mycam.transform.position;
            lastRotateRotation = mycam.transform.rotation;
            var perceelmidden = ConvertCoordinates.CoordConvert.RDtoUnity(MetadataLoader.Instance.perceelnummerPlaatscoordinaat);
            mycam.transform.position = new Vector3(perceelmidden.x, firstPersonHeight, perceelmidden.z);
            mycam.transform.LookAt(new Vector3(RestrictionChecker.ActiveUitbouw.CenterPoint.x, firstPersonHeight, RestrictionChecker.ActiveUitbouw.CenterPoint.z));
            currentRotation = new Vector2(mycam.transform.rotation.eulerAngles.y, mycam.transform.rotation.eulerAngles.x);
        }
        else
        {
            mycam.transform.position = lastRotatePosition;
            mycam.transform.rotation = lastRotateRotation;
        }

        return isFirstPersonMode;
    }
   

    private void FirstPersonLook()
    {
        var lastY = mycam.transform.position.y;

        if (Input.GetKey(KeyCode.LeftArrow))
        {            
            mycam.transform.position += -mycam.transform.right * moveSpeed/5 * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {         
            mycam.transform.position += mycam.transform.right * moveSpeed/5 * Time.deltaTime;
        }
        
        if (Input.GetKey(KeyCode.UpArrow))
        {
            var newpos = mycam.transform.position += mycam.transform.forward * moveSpeed * Time.deltaTime;
            newpos.y = lastY;
            mycam.transform.position = newpos;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            var newpos = mycam.transform.position += -mycam.transform.forward * moveSpeed * Time.deltaTime;
            newpos.y = lastY;
            mycam.transform.position = newpos;
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
            var lastY = mycam.transform.position.y;

            var moveSpeed = Mathf.Sqrt(mycam.transform.position.y) * 1.3f;
            var newpos = mycam.transform.position + mycam.transform.forward.normalized * (scrollDelta * moveSpeed * ZoomSpeed);

            if (isFirstPersonMode)
            {
                newpos.y = lastY;
            }
            else if (newpos.y < MinCameraHeight) return;

            mycam.transform.position = newpos;            
        }
    }

    void RotateAround(float xaxis, float yaxis)
    {
        if (RestrictionChecker.ActiveUitbouw != null)
        {
            //mycam.transform.RotateAround(RestrictionChecker.ActiveUitbouw.CenterPoint, mycam.transform.right, -yaxis * RotationSpeed);
            mycam.transform.RotateAround(RestrictionChecker.ActiveUitbouw.CenterPoint, Vector3.up, xaxis * RotationSpeed);
        } 
        else if (RestrictionChecker.ActiveBuilding)
            mycam.transform.RotateAround(RestrictionChecker.ActiveBuilding.BuildingCenter, Vector3.up, xaxis * RotationSpeed);        
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
