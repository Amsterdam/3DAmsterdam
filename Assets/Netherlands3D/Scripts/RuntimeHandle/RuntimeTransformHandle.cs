using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using Netherlands3D.ObjectInteraction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace RuntimeHandle
{
    /**
     * Created by Peter @sHTiF Stefcek 21.10.2020
     * Altered by 3D Amsterdam Team to make translation handles be relative to world, use the new input system and not mouse delta. 07.01.2021
     */
    public class RuntimeTransformHandle : Interactable
    {
        public HandleAxes axes = HandleAxes.XYZ;
        public HandleSpace space = HandleSpace.LOCAL;
        public HandleType type = HandleType.POSITION;
        public HandleSnappingType snappingType = HandleSnappingType.RELATIVE;

        public Vector3 positionSnap = Vector3.zero;
        public float rotationSnap = 0;
        public Vector3 scaleSnap = Vector3.zero;

        public bool autoScale = false;
        public float autoScaleFactor = 1;

        private Vector3 previousPosition;
        private HandleBase previousAxis;
        
        private HandleBase draggingAxis;

        private HandleType previousType;
        private HandleAxes previousAxes;

        private PositionHandle positionHandle;
        private RotationHandle rotationHandle;
        private ScaleHandle scaleHandle;

        public Transform target;

        public UnityEvent movedHandle;

        private string raycastLayerName = "Interface3D";
        public int raycastLayer;

        void Awake()
        {
            contextMenuState = ContextPointerMenu.ContextState.CUSTOM_OBJECTS;

            raycastLayer = LayerMask.NameToLayer(raycastLayerName);

            movedHandle = new UnityEvent();

            previousType = type;

            if (target == null)
                target = transform;

            CreateHandles();
        }

        void CreateHandles()
        {
            switch (type)
            {
                case HandleType.POSITION:
                    positionHandle = gameObject.AddComponent<PositionHandle>().Initialize(this);
                    break;
                case HandleType.ROTATION:
                    rotationHandle = gameObject.AddComponent<RotationHandle>().Initialize(this);
                    break;
                case HandleType.SCALE:
                    scaleHandle = gameObject.AddComponent<ScaleHandle>().Initialize(this);
                    break;
            }
        } 

        void Clear()
        {
            draggingAxis = null;
            
            if (positionHandle) positionHandle.Destroy();
            if (rotationHandle) rotationHandle.Destroy();
            if (scaleHandle) scaleHandle.Destroy();
        }

        public override void SecondarySelect()
        {
            base.Select();
            ServiceLocator.GetService<ContextPointerMenu>().SetTargetInteractable(target.GetComponent<Interactable>());
        }

        void Update()
        {
            if (!target)
            {
                gameObject.SetActive(false);
                return;
            }
                        
            if (previousType != type || previousAxes != axes)
            {
                Clear();
                CreateHandles();
                previousType = type;
                previousAxes = axes;
            }

            HandleBase axis = GetAxis();

            HandleOverEffect(axis);

            if (Input.GetMouseButton(0) && draggingAxis != null)
            {
                draggingAxis.Interact(previousPosition);
                movedHandle.Invoke();
            }

            if (Input.GetMouseButtonDown(0) && axis != null)
            {
                draggingAxis = axis;
                draggingAxis.StartInteraction();

                TakeInteractionPriority();
            }

            if (Input.GetMouseButtonUp(0) && draggingAxis != null)
            {
                Debug.Log("End interaction");
                draggingAxis.EndInteraction();
                draggingAxis = null;

                StopInteraction();
            }

            previousPosition = Mouse.current.position.ReadValue();

            transform.position = target.transform.position;
            if (space == HandleSpace.LOCAL || type == HandleType.SCALE)
            {
                transform.rotation = target.transform.rotation;
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }
        }

		private void LateUpdate()
		{
            if (autoScale)
                transform.localScale =
                    Vector3.one * (Vector3.Distance(ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position, transform.position) * autoScaleFactor) / 15;
        }

		void HandleOverEffect(HandleBase p_axis)
        {
            if (draggingAxis == null && previousAxis != null && previousAxis != p_axis)
            {
                previousAxis.SetDefaultColor();
            }

            if (p_axis != null && draggingAxis == null)
            {
                p_axis.SetColor(Color.yellow);
            }

            previousAxis = p_axis;
        }

        private HandleBase GetAxis()
        {
            RaycastHit[] hits = Physics.RaycastAll(Selector.mainSelectorRay, 10000, LayerMask.GetMask(raycastLayerName));
            if (hits.Length == 0)
                return null;

            foreach (RaycastHit hit in hits)
            {
                HandleBase axis = hit.collider.gameObject.GetComponentInParent<HandleBase>();
                if (axis != null)
                {
                    return axis;
                }
            }
            return null;
        }

        static public RuntimeTransformHandle Create(Transform p_target, HandleType p_handleType)
        {
            RuntimeTransformHandle runtimeTransformHandle = new GameObject().AddComponent<RuntimeTransformHandle>();
            runtimeTransformHandle.target = p_target;
            runtimeTransformHandle.type = p_handleType;

            return runtimeTransformHandle;
        }
    }
}