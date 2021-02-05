using Amsterdam3D.JavascriptConnection;
using ConvertCoordinates;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Amsterdam3D.InputHandler;
using UnityEngine.InputSystem;

namespace Amsterdam3D.CameraMotion
{
    public class GodViewCamera : MonoBehaviour, ICameraControls
    {
        [HideInInspector]
        public Camera cameraComponent;

        [SerializeField]
        private float zoomSpeed = 0.5f;

        [SerializeField]
        private float spinSpeed = 0.5f;

        private const float maxZoomOut = 2500f;

        [SerializeField]
        private bool dragging = false;
        [SerializeField]
        private bool rotatingAroundPoint = false;
        public bool HoldingInteraction => (dragging || rotatingAroundPoint);

        private const float rotationSpeed = 50.0f;

        private const float minAngle = -89f;
        private const float maxAngle = 89f;
        private const float speedFactor = 50.0f;

        private float maxClickDragDistance = 5000.0f;
        private float maxTravelDistance = 20000.0f;

        private const float rotationSensitivity = 20.0f;

        private const float floorOffset = 1.8f;

        private Vector3 startMouseDrag;

        [SerializeField]
        private Vector3 cameraOffsetForTargetLocation = new Vector3(100,100,200);

        private float scrollDelta;
#if UNITY_WEBGL && !UNITY_EDITOR
        float webGLScrollMultiplier = 100.0f;
#endif

        private float moveSpeed;

        private float deceleration = 10.0f;
        private Vector3 dragMomentum = Vector3.zero;
        private float maxMomentum = 1000.0f;

        private bool firstPersonModifier = false;

        public bool LockFunctions = false;

        private Vector3 zoomDirection;
        private Vector3 rotatePoint;

        private Vector2 currentRotation;

        public delegate void FocusPointChanged(Vector3 pointerPosition);
        public static FocusPointChanged focusingOnTargetPoint;

        private Plane worldPlane = new Plane(Vector3.up, new Vector3(0, Constants.ZERO_GROUND_LEVEL_Y, 0));

        private IAction rotateActionMouse;
        private IAction dragActionMouse;
        private IAction zoomScrollActionMouse;
        private IAction zoomDragActionMouse;

        private IAction modifierFirstPersonAction;

        private IAction moveActionKeyboard;
        private IAction rotateActionKeyboard;
        private IAction zoomActionKeyboard;

        private float minUndergroundY = 0.0f;

        private InputActionMap mouseActionMap;
        private InputActionMap keyboardActionMap;

        void Awake()
        {
            cameraComponent = GetComponent<Camera>();
        }

        void Start()
		{
            mouseActionMap = ActionHandler.actions.GodViewMouse;
            keyboardActionMap = ActionHandler.actions.GodViewKeyboard;

            currentRotation = new Vector2(cameraComponent.transform.rotation.eulerAngles.y, cameraComponent.transform.rotation.eulerAngles.x);
			AddActionListeners();
		}

        private void AddActionListeners()
		{
            //Mouse actions
            dragActionMouse = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewMouse.Drag);
			rotateActionMouse = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewMouse.SpinDrag);
            zoomScrollActionMouse = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewMouse.Zoom);
            zoomDragActionMouse = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewMouse.ZoomDrag);

            //Keyboard actions
            moveActionKeyboard = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewKeyboard.MoveCamera);
			rotateActionKeyboard = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewKeyboard.RotateCamera);
            zoomActionKeyboard = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewKeyboard.Zoom);

            //Combination
            modifierFirstPersonAction = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewMouse.FirstPersonModifier);

            //Listeners
            dragActionMouse.SubscribePerformed(Drag);
            dragActionMouse.SubscribeCancelled(Drag);

            rotateActionMouse.SubscribePerformed(SpinDrag);
            rotateActionMouse.SubscribeCancelled(SpinDrag);

            zoomScrollActionMouse.SubscribePerformed(Zoom);

            modifierFirstPersonAction.SubscribePerformed(FirstPersonModifier);
            modifierFirstPersonAction.SubscribeCancelled(FirstPersonModifier);
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
            if (enabled && !ActionHandler.actions.GodViewMouse.enabled)
            {
                ActionHandler.actions.GodViewMouse.Enable();
            }
            else if(!enabled && ActionHandler.actions.GodViewMouse.enabled)
            {
                dragging = false;
                rotatingAroundPoint = false;
                ActionHandler.actions.GodViewMouse.Disable();
            }
        }

        private void FirstPersonModifier(IAction action)
        {
            if (action.Cancelled)
            {
                firstPersonModifier = false;
            }
            else if (action.Performed)
            {
                firstPersonModifier = true;
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
                var zoomPoint = cameraComponent.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000.0f));
                ZoomInDirection(scrollDelta, zoomPoint);
            }
        }

        private void Drag(IAction action)
		{
            if(action.Cancelled)
            {
                dragging = false;
            } 
            else if (action.Performed)
            {
                startMouseDrag = GetMousePositionInWorld();
                dragging = true;
            }
        }

        private void SpinDrag(IAction action)
        {
            if (action.Cancelled)
            {
                rotatingAroundPoint = false;
            }
            else if (action.Performed)
            {
                rotatingAroundPoint = true;
                SetFocusPoint();
            }
        }

        void Update()
		{
            if (dragging)
            {
                if (firstPersonModifier)
                {
                    FirstPersonLook();
                }
                else
                {
                    Dragging();
                }
            }
            else{
                if (rotatingAroundPoint)
                {
                    RotateAroundPoint();
                }
                else if(!BlockedByTextInput())
                {
                    HandleTranslationInput();
                    HandleRotationInput();
				}
                EazeOutDragVelocity();
            }  
  
            LimitPosition();
		}

        /// <summary>
        /// Clamps the camera within the max travel distance bounding box
        /// </summary>
		private void LimitPosition()
		{
            this.transform.position = new Vector3(Mathf.Clamp(this.transform.position.x,-maxTravelDistance, maxTravelDistance), Mathf.Clamp(this.transform.position.y, minUndergroundY, maxZoomOut), Mathf.Clamp(this.transform.position.z, -maxTravelDistance, maxTravelDistance));
		}

		private bool BlockedByTextInput(){
            return EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
        }

		public void MoveAndFocusOnLocation(Vector3 targetLocation, Quaternion targetRotation = new Quaternion())
		{
            cameraComponent.transform.position = targetLocation + cameraOffsetForTargetLocation;
            cameraComponent.transform.LookAt(targetLocation, Vector3.up);

            focusingOnTargetPoint(targetLocation);
        }

        void HandleTranslationInput()
        {         
            moveSpeed = Mathf.Sqrt(cameraComponent.transform.position.y) * speedFactor;

            Vector3 movement = moveActionKeyboard.ReadValue<Vector2>();
            if (movement != null)
            {
                movement.z = movement.y;
                movement.y = 0;
                movement = Quaternion.AngleAxis(cameraComponent.transform.eulerAngles.y, Vector3.up) * movement;
                cameraComponent.transform.position += movement * moveSpeed * Time.deltaTime;
            }
        }

        private void HandleRotationInput()
        {
            currentRotation = new Vector2(cameraComponent.transform.rotation.eulerAngles.y, cameraComponent.transform.rotation.eulerAngles.x);
            if (rotateActionKeyboard != null)
            {
                Vector2 rotationInput = rotateActionKeyboard.ReadValue<Vector2>();
                Vector3 rotation = cameraComponent.transform.rotation.eulerAngles;
                rotation.y += rotationInput.x * rotationSpeed * Time.deltaTime;
                rotation.x += rotationInput.y * rotationSpeed * Time.deltaTime;
                cameraComponent.transform.eulerAngles = rotation;
            }
		}

		private void FirstPersonLook()
		{
            var mouseDelta = Mouse.current.delta.ReadValue();

			//Convert mouse position into local rotations
			currentRotation.x += mouseDelta.x * rotationSensitivity * Time.deltaTime;
			currentRotation.y -= mouseDelta.y * rotationSensitivity * Time.deltaTime;

			//Adjust camera rotation
			cameraComponent.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
        }

		private void ClampRotation()
        {
            cameraComponent.transform.rotation = Quaternion.Euler(new Vector3(
                ClampAngle(cameraComponent.transform.localEulerAngles.x, minAngle, maxAngle),
                cameraComponent.transform.localEulerAngles.y, 
                cameraComponent.transform.localEulerAngles.z));
        }

        /// <summary>
        /// Use a normalized value to set the camera height between floor level and maximum height 
        /// </summary>
        /// <param name="normalizedHeight">Range from 0 to 1</param>
        public void SetNormalizedCameraHeight(float normalizedHeight){
            var newHeight = Mathf.Lerp(minUndergroundY, maxZoomOut, normalizedHeight);
            cameraComponent.transform.position = new Vector3(cameraComponent.transform.position.x, newHeight, cameraComponent.transform.position.z);
        }

        /// <summary>
        /// Returns the normalized 0 to 1 value of the camera height
        /// </summary>
        /// <returns>Normalized 0 to 1 value of the camera height</returns>
        public float GetNormalizedCameraHeight(){
            return Mathf.InverseLerp(minUndergroundY, maxZoomOut, cameraComponent.transform.position.y);
        }

        public float GetCameraHeight()
        {
            return cameraComponent.transform.position.y;
        }

        private void ZoomInDirection(float zoomAmount, Vector3 zoomDirectionPoint)
        {
            var heightSpeed = cameraComponent.transform.position.y; //The higher we are, the faster we zoom
            zoomDirection = (zoomDirectionPoint - cameraComponent.transform.position).normalized;
            cameraComponent.transform.Translate(zoomDirection * zoomSpeed * zoomAmount * heightSpeed, Space.World);
            focusingOnTargetPoint.Invoke(zoomDirectionPoint);
        }

        private void Dragging()
		{
			dragMomentum = (GetMousePositionInWorld() - startMouseDrag);

			if (dragMomentum.magnitude > 0.1f)
				transform.position -= dragMomentum;

            //Filter out extreme swings
            if (dragMomentum.magnitude > maxMomentum) dragMomentum = Vector3.ClampMagnitude(dragMomentum, maxMomentum); 
		}

		private void EazeOutDragVelocity()
		{
			//Slide forward in dragged direction
			dragMomentum = Vector3.Lerp(dragMomentum, Vector3.zero, Time.deltaTime * deceleration);
			if (dragMomentum.magnitude > 0.1f)
			{
				this.transform.position -= dragMomentum;
			}
		}

		public Vector3 GetMousePositionInWorld(Vector3 optionalPositionOverride = default)
        {
            var pointerPosition = Mouse.current.position.ReadValue();
            if (optionalPositionOverride != default) pointerPosition = optionalPositionOverride;

            var screenRay = cameraComponent.ScreenPointToRay(pointerPosition);            
            worldPlane.Raycast(screenRay, out float distance);

            var samplePoint = screenRay.GetPoint(Mathf.Min(maxClickDragDistance, distance));
            samplePoint.y = Constants.ZERO_GROUND_LEVEL_Y;

            return samplePoint;
        }

        private void RotateAroundPoint()
		{
			var mouseDelta = Mouse.current.delta.ReadValue();

			var previousPosition = cameraComponent.transform.position;
			var previousRotation = cameraComponent.transform.rotation;

			cameraComponent.transform.RotateAround(rotatePoint, cameraComponent.transform.right, -mouseDelta.y * spinSpeed);
			cameraComponent.transform.RotateAround(rotatePoint, Vector3.up, mouseDelta.x * spinSpeed);

			if (cameraComponent.transform.position.y < minUndergroundY)
			{
				//Do not let the camera go beyond the rotationpoint height
				cameraComponent.transform.position = previousPosition;
				cameraComponent.transform.rotation = previousRotation;
			}

			currentRotation = new Vector2(cameraComponent.transform.rotation.eulerAngles.y, cameraComponent.transform.rotation.eulerAngles.x);

			focusingOnTargetPoint.Invoke(rotatePoint);
		}

		private void SetFocusPoint()
		{
			var pointerPosition = Mouse.current.position.ReadValue();

			RaycastHit hit;
			var screenRay = cameraComponent.ScreenPointToRay(pointerPosition);

			//Determine the point we will spin around
			if (Transformable.lastSelectedTransformable != null)
			{
				rotatePoint = Transformable.lastSelectedTransformable.transform.position;
			}
			else if (Physics.Raycast(screenRay, out hit))
			{
				rotatePoint = hit.point;
				focusingOnTargetPoint(rotatePoint);
			}
			else
			{
				rotatePoint = GetMousePositionInWorld();
				focusingOnTargetPoint(rotatePoint);
			}
		}

		private float ClampAngle(float angle, float from, float to)
        {
            if (angle < 0f) angle = 360 + angle;
            if (angle > 180f) return Mathf.Max(angle, 360 + from);
            return Mathf.Min(angle, to);
        }

		public bool UsesActionMap(InputActionMap actionMap)
		{
            return (actionMap == mouseActionMap || actionMap == keyboardActionMap);
		}
	}
}