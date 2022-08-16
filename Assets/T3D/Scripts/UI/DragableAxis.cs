using Netherlands3D.Cameras;
using Netherlands3D.ObjectInteraction;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InteractableState
{
    Default = 0,
    Hover = 1,
    Active = 2,
}

namespace Netherlands3D.T3D.Uitbouw
{
    public class DragableAxis : Interactable
    {
        [SerializeField]
        private float maxClickDragDistance = 50f;

        //[SerializeField]
        private UitbouwBase uitbouw;
        private Vector3 planeProjectedOffset;
        private Vector3 axisProjectedOffset;
        private float heightOffset;

        public Vector3 PlanarDeltaPosition { get; private set; }
        public Vector3 LateralDeltaPosition { get; private set; }
        public float HeightDeltaPosition { get; private set; }
        private const float heigtSpeedMultiplier = 0.5f;
        public bool IsDragging { get; private set; }

        [SerializeField]
        private ColorPalette interactionColors;

        private CameraMode cameraMode;

        public static DragableAxis CreateDragableAxis(GameObject prefab, Vector3 position, Quaternion rotation, UitbouwBase linkedUitbouw)
        {
            var axisObject = Instantiate(prefab, position, rotation, linkedUitbouw.transform);
            var axis = axisObject.GetComponent<DragableAxis>();
            axis.SetUitbouw(linkedUitbouw);
            return axis;
        }

        public void SetUitbouw(UitbouwBase linkedUitbouw)
        {
            uitbouw = linkedUitbouw;
        }

        protected virtual void Start()
        {
            ServiceLocator.GetService<CameraModeChanger>().CameraModeChangedEvent += DragableAxis_CameraModeChangedEvent;
        }

        protected virtual void Update()
        {
            ProcessInteractionState();

            if (IsDragging)
            {
                CalculateDeltaPosition();
                //transform.position += DeltaPosition;
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.GetService<CameraModeChanger>().CameraModeChangedEvent -= DragableAxis_CameraModeChangedEvent;
        }

        private void ProcessInteractionState()
        {
            

            if (cameraMode != CameraMode.StreetView && IsHovered())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //start drag
                    TakeInteractionPriority();
                    IsDragging = true;
                    SetHighlight(InteractableState.Active);
                    RecalculateOffset();
                }
                else if (!Input.GetMouseButton(0))
                {
                    SetHighlight(InteractableState.Hover);
                }
            }
            else if (!Input.GetMouseButton(0))
            {
                //not dragging and not hovering
                SetHighlight(InteractableState.Default);
            }

            if (Input.GetMouseButtonUp(0))
            {
                //end drag
                StopInteraction();
                IsDragging = false;

                axisProjectedOffset = Vector3.zero;
                LateralDeltaPosition = Vector3.zero;

                planeProjectedOffset = Vector3.zero;
                PlanarDeltaPosition = Vector3.zero;

                heightOffset = 0;
                HeightDeltaPosition = 0;
            }
        }

        private void DragableAxis_CameraModeChangedEvent(object source, CameraMode newMode)
        {
            cameraMode = newMode;
        }

        public void RecalculateOffset()
        {
            Vector3 aimedPosition = GetPointerPositionInWorld();
            var axisPorjectedPoint = Vector3.Project((aimedPosition - transform.position), uitbouw.transform.right);
            var planeProjectedPoint = uitbouw.GroundPlane.ClosestPointOnPlane(aimedPosition);

            axisProjectedOffset = axisPorjectedPoint;
            planeProjectedOffset = planeProjectedPoint - transform.position;

            heightOffset = Input.mousePosition.y;
        }

        private void CalculateDeltaPosition()
        {
            Vector3 aimedPosition = GetPointerPositionInWorld();
            var axisProjectedPoint = Vector3.Project((aimedPosition - transform.position), uitbouw.transform.right) + transform.position;
            var planeProjectedPoint = uitbouw.GroundPlane.ClosestPointOnPlane(aimedPosition);
            PlanarDeltaPosition = planeProjectedPoint - planeProjectedOffset - transform.position;
            LateralDeltaPosition = axisProjectedPoint - axisProjectedOffset - transform.position;

            var dist = Vector3.Distance(transform.position, ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position);
            HeightDeltaPosition = (Input.mousePosition.y - heightOffset) * heigtSpeedMultiplier * (1/dist);
            heightOffset = Input.mousePosition.y;
        }

        private void SetHighlight(InteractableState status) //0: normal, 1: hover, 2: selected
        {
            if (!interactionColors)
                return;

            //switch (status)
            //{
            //    case InteractableState.Default:
            //        ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.AUTO);
            //        break;
            //    case InteractableState.Hover:
            //        ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.GRAB);
            //        print("grab");
            //        break;
            //    case InteractableState.Active:
            //        print("grabing");
            //        ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.GRABBING);
            //        break;
            //}

            var renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.material.color = interactionColors[status.ToString()];
            }
        }

        public Vector3 GetPointerPositionInWorld(Vector3 optionalPositionOverride = default)
        {
            var pointerPosition = Mouse.current.position.ReadValue();
            if (optionalPositionOverride != default) pointerPosition = optionalPositionOverride;

            var cameraComponent = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera;
            var screenRay = cameraComponent.ScreenPointToRay(pointerPosition);
            uitbouw.GroundPlane.Raycast(screenRay, out float distance);
            var samplePoint = screenRay.GetPoint(Mathf.Min(maxClickDragDistance, distance));
            samplePoint.y = Config.activeConfiguration.zeroGroundLevelY;

            return samplePoint;
        }
    }
}
