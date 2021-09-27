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

public class DragableAxis : Interactable
{
    [SerializeField]
    private float maxClickDragDistance = 50f;

    private Uitbouw uitbouw;
    private Vector3 offset;

    public Vector3 DeltaPosition { get; private set; }
    public bool IsDragging { get; private set; }

    public void Awake()
    {
        uitbouw = GetComponentInParent<Uitbouw>();
    }

    private void Update()
    {
        ProcessInteractionState();

        if (IsDragging)
        {
            CalculateDeltaPosition();
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
                IsDragging = true;
                SetHighlight(InteractableState.Active);
                RecalculateOffset();
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
            IsDragging = false;
            offset = Vector3.zero;
            DeltaPosition = Vector3.zero;
        }
    }

    public void RecalculateOffset()
    {
        Vector3 aimedPosition = GetPointerPositionInWorld();
        var projectedLocalPoint = Vector3.Project((aimedPosition - transform.position), uitbouw.transform.right);
        offset = projectedLocalPoint;
    }

    private void CalculateDeltaPosition()
    {
        Vector3 aimedPosition = GetPointerPositionInWorld();
        var projectedPoint = Vector3.Project((aimedPosition - transform.position), uitbouw.transform.right) + transform.position;
        DeltaPosition = projectedPoint - offset - transform.position;
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
}
