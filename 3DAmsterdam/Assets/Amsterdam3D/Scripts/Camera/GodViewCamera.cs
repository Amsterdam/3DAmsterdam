using Amsterdam3D.JavascriptConnection;
using ConvertCoordinates;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Amsterdam3D.InputHandler;
namespace Amsterdam3D.CameraMotion
{
    public class GodViewCamera : MonoBehaviour, ICameraControls
    {
        [HideInInspector]
        public Camera cameraComponent;

        [SerializeField]
        private float zoomSpeed = 0.5f;
        private const float maxZoomOut = 2500f;

        private Vector3 mouseZoomStart;
        private Vector3 mouseZoomTarget;
        private float mouseZoomSpeed = 0.01f;

        private const float rotationSpeed = 1f;

        private const float minAngle = -89f;
        private const float maxAngle = 89f;
        private const float speedFactor = 0.5f;

        private float maxClickDragDistance = 5000.0f;
        private float maxTravelDistance = 20000.0f;

        private const float rotationSensitivity = 5f;

        private const float floorOffset = 1.8f;

        private Vector3 startMouseDrag;

        [SerializeField]
        private Vector3 cameraOffsetForTargetLocation = new Vector3(100,100,200);

        private float scrollDelta;
  
        private float moveSpeed;
        private float mouseHorizontal;
        private float mouseVertical;

        private float deceleration = 10.0f;
        private Vector3 dragMomentum = Vector3.zero;
        private float maxMomentum = 1000.0f;

        private bool requireMouseClickBeforeDrag = false;
        private bool clickStartedBlocked = false;

        private bool translationModifier = false;
        private bool firstPersonModifier = false;

        private bool canUseMouseRelatedFunctions = true;

        public bool LockFunctions = false;

        private Vector3 zoomDirection;

        private Vector3 dragOrigin;
        private Vector3 rotatePoint;

        private Vector2 currentRotation;

        public delegate void FocusPointChanged(Vector3 pointerPosition);
        public static FocusPointChanged focusingOnTargetPoint;

        private Plane worldPlane = new Plane(Vector3.up, new Vector3(0, Constants.ZERO_GROUND_LEVEL_Y, 0));

        private IAction moveAction;
        private IAction rotateAction;

        private float minUndergroundY = 0.0f;

        void Awake()
        {
            cameraComponent = GetComponent<Camera>();
        }

        void Start()
        {
            currentRotation = new Vector2(cameraComponent.transform.rotation.eulerAngles.y, cameraComponent.transform.rotation.eulerAngles.x);

            moveAction = ActionHandler.instance.GetAction(ActionHandler.actions.GodView.MoveCamera);
            rotateAction = ActionHandler.instance.GetAction(ActionHandler.actions.GodView.RotateCamera);

            ActionHandler.actions.GodView.Enable();
        }

        void Update()
		{
            //Check if we started dragging on the UI
            if (EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
            {
                clickStartedBlocked = true;
            }
            else if (clickStartedBlocked && Input.GetMouseButtonUp(0))
            {
                clickStartedBlocked = false;
            }

            canUseMouseRelatedFunctions = (!clickStartedBlocked && !EventSystem.current.IsPointerOverGameObject() && !ObjectManipulation.manipulatingObject);

           //these shouldn't be needed anymore since unity input system has modifiers built in
            translationModifier = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            firstPersonModifier = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (!Input.GetKey(KeyCode.Space))
            {
                HandleTranslationInput();
                HandleRotationInput();

                if (canUseMouseRelatedFunctions)
                {
                    Zooming();
                    Dragging();
                    RotationAroundPoint();
                }

                ClampRotation();
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

            if (!translationModifier && !BlockedByTextInput() && moveAction != null)
            {
                Vector3 movement = moveAction.ReadValue<Vector2>();
                if (movement != null)
                {
                    movement.z = movement.y;
                    movement.y = 0;
                    movement = Quaternion.AngleAxis(cameraComponent.transform.eulerAngles.y, Vector3.up) * movement;
                    cameraComponent.transform.position += movement * moveSpeed;
                }
            }
        }

        private void HandleRotationInput()
        {
            currentRotation = new Vector2(cameraComponent.transform.rotation.eulerAngles.y, cameraComponent.transform.rotation.eulerAngles.x);
            if (!BlockedByTextInput())
            {
                if (rotateAction != null)
                {
                    Vector2 rotationInput = rotateAction.ReadValue<Vector2>();
                    Vector3 rotation = cameraComponent.transform.rotation.eulerAngles;
                    rotation.y += rotationInput.x * rotationSpeed;
                    rotation.x += rotationInput.y * rotationSpeed;
                    cameraComponent.transform.eulerAngles = rotation;
                }
            }
            
            if (firstPersonModifier && Input.GetMouseButton(0))
			{
				FirstPersonLook();
			}
		}

		private void FirstPersonLook()
		{
			mouseHorizontal = Input.GetAxis("Mouse X");
			mouseVertical = Input.GetAxis("Mouse Y");

			//Convert mouse position into local rotations
			currentRotation.x += mouseHorizontal * rotationSensitivity;
			currentRotation.y -= mouseVertical * rotationSensitivity;

			//Adjust camera rotation
			cameraComponent.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);

            //Require a mouse release, before we register drag events again
            requireMouseClickBeforeDrag = true;
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

        private void ClampToGround()
        {
            Debug.DrawRay(cameraComponent.transform.position, Vector3.down * floorOffset, Color.red);
            RaycastHit hit;
            if (Physics.Raycast(cameraComponent.transform.position, Vector3.down * floorOffset, out hit)){ 
                if (hit.distance < floorOffset)
                {
                    cameraComponent.transform.position = hit.point + new Vector3(0, floorOffset, 0);
                }
            }
        }

        private void Zooming()
        {
            if (translationModifier)
            {
                if ((Input.GetKey(KeyCode.R)) && !BlockedByTextInput())
                {
                    var zoomPoint = cameraComponent.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000.0f));
                    ZoomInDirection(Time.deltaTime, zoomPoint);
                }
                else if ((Input.GetKey(KeyCode.F)) && !BlockedByTextInput())
                {
                    var zoomPoint = cameraComponent.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000.0f));
                    ZoomInDirection(-Time.deltaTime, zoomPoint);
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                startMouseDrag = Input.mousePosition;
                mouseZoomTarget = cameraComponent.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000.0f));
                mouseZoomStart = cameraComponent.transform.position;
            }
            else if (Input.GetMouseButton(1))
            {
                cameraComponent.transform.position = mouseZoomStart;
                ZoomInDirection(-(Input.mousePosition.y - startMouseDrag.y) * mouseZoomSpeed, mouseZoomTarget);
            }
            else
            {
                scrollDelta = Input.GetAxis("Mouse ScrollWheel");
                if (scrollDelta != 0)
                {
                    var zoomPoint = cameraComponent.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000.0f));
                    ZoomInDirection(scrollDelta, zoomPoint);
                }
            }   
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
            if (!translationModifier && !firstPersonModifier)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    requireMouseClickBeforeDrag = false;
                    startMouseDrag = GetMousePositionInWorld();
                    ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.GRABBING);
                }
                else if (Input.GetMouseButton(0) && !requireMouseClickBeforeDrag)
                {
                    dragMomentum = (GetMousePositionInWorld() - startMouseDrag);
                     
                    if(dragMomentum.magnitude > 0.1f)
                        transform.position -= dragMomentum;

                    //Filter out extreme swings
                    if (dragMomentum.magnitude > maxMomentum) dragMomentum = Vector3.zero;
                }
                else if (Input.GetMouseButtonUp(0)){
                    ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.AUTO);
                }
                else {
                    //Slide forward in dragged direction
                    dragMomentum = Vector3.Lerp(dragMomentum, Vector3.zero, Time.deltaTime * deceleration);
                    if (dragMomentum.magnitude > 0.1f)
                    {
                        this.transform.position -= dragMomentum;
                    }
                }
            }
        }

        public Vector3 GetMousePositionInWorld(Vector3 optionalPositionOverride = default)
        {
            var pointerPosition = Input.mousePosition;
            if (optionalPositionOverride != default) pointerPosition = optionalPositionOverride;

            var ray = cameraComponent.ScreenPointToRay(pointerPosition);            
            worldPlane.Raycast(ray, out float distance);

            var samplePoint = ray.GetPoint(Mathf.Min(maxClickDragDistance, distance));
            samplePoint.y = Constants.ZERO_GROUND_LEVEL_Y;

            return samplePoint;
        }

        private void RotationAroundPoint()
        {
            RaycastHit hit;
            var ray = cameraComponent.ScreenPointToRay(Input.mousePosition);

            if (Input.GetMouseButtonDown(2))
            {
                //Check if a collider is under our mouse, if not get a point on NAP~0
                if (Physics.Raycast(ray, out hit))
                {
                    rotatePoint = hit.point;
                    focusingOnTargetPoint(rotatePoint);
                }
                else if (new Plane(Vector3.up, new Vector3(0.0f, Constants.ZERO_GROUND_LEVEL_Y, 0.0f)).Raycast(ray, out float enter)){
                    rotatePoint = ray.GetPoint(enter);
                    focusingOnTargetPoint(rotatePoint);
                }
            }

            if (Input.GetMouseButton(2))
            {
                var mouseX = Input.GetAxis("Mouse X");
                var mouseY = Input.GetAxis("Mouse Y");

                var previousPosition = cameraComponent.transform.position;
                var previousRotation = cameraComponent.transform.rotation;

                cameraComponent.transform.RotateAround(rotatePoint, cameraComponent.transform.right, -mouseY * 5f);
                cameraComponent.transform.RotateAround(rotatePoint, Vector3.up, mouseX * 5f);

                if (cameraComponent.transform.position.y < minUndergroundY )
                {
                    //Do not let the camera go beyond the rotationpoint height
                    cameraComponent.transform.position = previousPosition;
                    cameraComponent.transform.rotation = previousRotation;
                }

                currentRotation = new Vector2(cameraComponent.transform.rotation.eulerAngles.y, cameraComponent.transform.rotation.eulerAngles.x);

                focusingOnTargetPoint.Invoke(rotatePoint);
            }
        }

        private float ClampAngle(float angle, float from, float to)
        {
            if (angle < 0f) angle = 360 + angle;
            if (angle > 180f) return Mathf.Max(angle, 360 + from);
            return Mathf.Min(angle, to);
        }
    }
}