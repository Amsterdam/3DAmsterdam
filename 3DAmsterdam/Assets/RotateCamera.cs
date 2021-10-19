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
    public float RotationSpeed = 0.2f;
    public float ZoomSpeed = 0.01f;

    [SerializeField]
    private bool dragging = false;
    
    [SerializeField]
    private bool rotatingAroundPoint = false;

    private float scrollDelta;
#if UNITY_WEBGL && !UNITY_EDITOR
        float webGLScrollMultiplier = 100.0f;
#endif

    private IAction dragActionMouse;
    private IAction zoomScrollActionMouse;
    
    List<InputActionMap> availableActionMaps;

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
            rotatingAroundPoint = false;
        }
        else if (action.Performed)
        {            
            dragging = true;
        }
    }

    private void Zoom(IAction action)
    {
        scrollDelta = ActionHandler.actions.GodViewMouse.Zoom.ReadValue<Vector2>().y;

        //A bug with the new inputsystem only fixed in Unity 2021 causes scroll input to be very low on WebGL builds
#if UNITY_WEBGL && !UNITY_EDITOR
                scrollDelta *= webGLScrollMultiplier;
#endif

        if (scrollDelta != 0)
        {
            var moveSpeed = Mathf.Sqrt(mycam.transform.position.y) * 1.3f;
            var newpos = mycam.transform.position + mycam.transform.forward.normalized * (scrollDelta * moveSpeed * ZoomSpeed);

            if (newpos.y < MinCameraHeight) return;
            
            mycam.transform.position = newpos;            
        }
    }

    void RotateAround(float xaxis, float yaxis)
    {
        var previousPosition = mycam.transform.position;
        var previousRotation = mycam.transform.rotation;

        if (Uitbouw.Instance == null) return;

        mycam.transform.RotateAround(Uitbouw.Instance.CenterPoint, Vector3.up, xaxis * RotationSpeed);
      //  mycam.transform.RotateAround(Uitbouw.Instance.CenterPoint, mycam.transform.right, -yaxis * RotationSpeed);
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
