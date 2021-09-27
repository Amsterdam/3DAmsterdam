using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D;
using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using Netherlands3D.ObjectInteraction;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InteractableState
{
    None = 0,
    Hover = 1,
    Active = 2,
}

public class UitbouwMovementArrow : Interactable
{
    [SerializeField]
    private LayerMask dropTargetLayerMask;

    private bool isDragging;
    private Vector3 offset;
    [SerializeField]
    private float maxClickDragDistance = 50f;
    private Uitbouw uitbouw;
    public Vector3 deltaPosition;

    private void Awake()
    {
        uitbouw = GetComponentInParent<Uitbouw>();
    }

    public override void Select()
    {
        print("select");
        //FollowMousePointer();
    }

    public override void Deselect()
    {
        print("");
    }

    private void Update()
    {
        ProcessInteractionState();

        if (isDragging)
        {
            FollowMousePointer();
        }
    }

    private void ProcessInteractionState()
    {
        if (IsHovered())
        {
            SetHighlight(InteractableState.Hover);
            if (Input.GetMouseButtonDown(0))
            {
                //start drag
                TakeInteractionPriority();
                isDragging = true;
                SetHighlight(InteractableState.Active);
                CalculateOffset();
            }
        }
        else if (!Input.GetMouseButton(0))
        {
            //not dragging and not hovering
            SetHighlight(InteractableState.None);
        }

        if (Input.GetMouseButtonUp(0))
        {
            //end drag
            StopInteraction();
            isDragging = false;
            offset = Vector3.zero;
        }
    }

    public void CalculateOffset()
    {
        Vector3 aimedPosition = GetPointerPositionInWorld();
        var projectedLocalPoint = Vector3.Project((aimedPosition - transform.position), uitbouw.transform.right);
        offset = projectedLocalPoint;
    }

    private void FollowMousePointer()
    {
        //if (Selector.Instance.HoveringInterface()) return;
        Vector3 aimedPosition = GetPointerPositionInWorld();
        //if (aimedPosition == Vector3.zero)
        //{
        //    return;
        //}
        var projectedPoint = Vector3.Project((aimedPosition - transform.position), uitbouw.transform.right) + transform.position;

        //transform.position = projectedPoint - offset;
        //uitbouw.transform.position = projectedPoint - offset - transform.localPosition;
        deltaPosition = projectedPoint - offset - transform.position;
    }

    private void SetHighlight(InteractableState status) //0: normal, 1: hover, 2: selected
    {
    }

    public Vector3 GetPointerPositionInWorld(Vector3 optionalPositionOverride = default)
    {
        var pointerPosition = Mouse.current.position.ReadValue();
        if (optionalPositionOverride != default) pointerPosition = optionalPositionOverride;

        var cameraComponent = CameraModeChanger.Instance.ActiveCamera;
        var screenRay = cameraComponent.ScreenPointToRay(pointerPosition);
        uitbouw.GroundPlane.Raycast(screenRay, out float distance);
        var samplePoint = screenRay.GetPoint(Mathf.Min(maxClickDragDistance, distance));
        samplePoint.y = Config.activeConfiguration.zeroGroundLevelY;

        return samplePoint;
    }

    /// <summary>
    /// Returns the mouse position on the layer.
    /// If the raycast fails (didnt hit anything) we use plane set at average ground height.
    /// </summary>
    /// <returns>The world point where our mouse is</returns>
    private Vector3 GetMousePointOnLayerMask()
    {

        RaycastHit hit;
        if (Physics.Raycast(Selector.mainSelectorRay, out hit, CameraModeChanger.Instance.ActiveCamera.farClipPlane, dropTargetLayerMask.value))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                return Vector3.zero;
            }
            return hit.point;
        }
        else
        {
            return CameraModeChanger.Instance.CurrentCameraControls.GetPointerPositionInWorld();
        }
    }
}
