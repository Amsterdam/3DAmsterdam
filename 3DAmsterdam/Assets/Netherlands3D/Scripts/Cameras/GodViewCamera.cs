using Netherlands3D.JavascriptConnection;
using Netherlands3D.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Netherlands3D.InputHandler;
using UnityEngine.InputSystem;
using Netherlands3D.ObjectInteraction;
using System.Collections.Generic;
using Netherlands3D.Interface;
using Netherlands3D.Settings;
using System.Collections;
using TMPro;

namespace Netherlands3D.Cameras
{
    public class GodViewCamera : MonoBehaviour, ICameraControls
    {
        [HideInInspector]
        public Camera cameraComponent;

        [SerializeField]
        private float zoomSpeed = 0.5f;

        [SerializeField]
        private float spinSpeed = 0.5f;

        private float farClipPlane = 6000;

        [SerializeField]
        private float firstPersonModifierSpinSpeed = 0.5f;

        private const float minOrtographicZoom = 20f;
        private float maxZoomOut = 2500f;
        private float minUndergroundY = -30f;

        [SerializeField]
        private bool dragging = false;
        [SerializeField]
        private bool rotatingAroundPoint = false;
        public bool HoldingInteraction => (dragging || rotatingAroundPoint);

        private const float rotationSpeed = 50.0f;
        private const float speedFactor = 50.0f;

        private float maxClickDragDistance = 5000.0f;
        private float maxTravelDistance = 20000.0f;

        private Vector3 startMouseDrag;

        private float startFov = 60.0f;
        private float mobileFov = 40.0f;

        [SerializeField]
        private Vector3 cameraOffsetForTargetLocation = new Vector3(100, 100, 200);

        private float scrollDelta;

        private float moveSpeed;

        private float deceleration = 10.0f;
        private Vector3 dragMomentum = Vector3.zero;
        private float maxMomentum = 1000.0f;

        private bool firstPersonModifier = false;
        private bool rotateAroundModifier = false;

        public bool LockFunctions = false;

        private Vector3 zoomDirection;
        private Vector3 rotatePoint;
        private Vector3 localOffsetVector;
        private Vector2 currentRotation;

        public delegate void FocusPointChanged(Vector3 pointerPosition);
        public static FocusPointChanged focusingOnTargetPoint;

        private Plane worldPlane;

        private IAction pointerPosition;
        private IAction rotateActionMouse;
        private IAction dragActionMouse;
        private IAction zoomScrollActionMouse;

        private IAction modifierFirstPersonAction;
        private IAction modifierRotateAroundAction;

        private IAction moveActionKeyboard;
        private IAction rotateActionKeyboard;
        private IAction zoomActionKeyboard;
        private IAction moveHeightActionKeyboard;

        private IAction flyActionGamepad;

        private IAction secondTouch;


        List<InputActionMap> availableActionMaps;

        [SerializeField]
        private float resetTransitionSpeed = 0.5f;
        private Coroutine resetNorthTransition;

		void Awake()
        {
            cameraComponent = GetComponent<Camera>();
            startFov = cameraComponent.fieldOfView;

            farClipPlane = cameraComponent.farClipPlane;
        }

        void Start()
        {
            if (ApplicationSettings.Instance.IsMobileDevice)
            {
                cameraComponent.fieldOfView = mobileFov;
                maxZoomOut = 670.0f;
                minUndergroundY = 100.0f;
            }

            worldPlane = new Plane(Vector3.up, new Vector3(0, Config.activeConfiguration.zeroGroundLevelY, 0));

            availableActionMaps = new List<InputActionMap>()
            {
                ActionHandler.actions.GodViewMouse,
                ActionHandler.actions.GodViewKeyboard
            };

            currentRotation = new Vector2(cameraComponent.transform.rotation.eulerAngles.y, cameraComponent.transform.rotation.eulerAngles.x);
            AddActionListeners();
        }

        private void AddActionListeners()
        {
            //Mouse actions
            pointerPosition = ActionHandler.instance.GetAction(ActionHandler.actions.Selector.Position);

            dragActionMouse = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewMouse.Drag);
            rotateActionMouse = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewMouse.SpinDrag);
            zoomScrollActionMouse = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewMouse.Zoom);

            //Keyboard actions
            moveActionKeyboard = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewKeyboard.MoveCamera);
            rotateActionKeyboard = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewKeyboard.RotateCamera);
            zoomActionKeyboard = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewKeyboard.Zoom);
            moveHeightActionKeyboard = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewKeyboard.MoveCameraHeight);

            //Gamepad
            flyActionGamepad = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewKeyboard.Fly);

            //Combination
            modifierFirstPersonAction = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewMouse.FirstPersonModifier);
            modifierRotateAroundAction = ActionHandler.instance.GetAction(ActionHandler.actions.GodViewMouse.RotateAroundModifier);

            //Listeners
            dragActionMouse.SubscribePerformed(Drag);
            dragActionMouse.SubscribeCancelled(Drag);

            rotateActionMouse.SubscribePerformed(SpinDrag);
            rotateActionMouse.SubscribeCancelled(SpinDrag);

            zoomScrollActionMouse.SubscribePerformed(Zoom);

            modifierFirstPersonAction.SubscribePerformed(FirstPersonModifier);
            modifierFirstPersonAction.SubscribeCancelled(FirstPersonModifier);

            modifierRotateAroundAction.SubscribePerformed(RotateAroundModifier);
            modifierRotateAroundAction.SubscribeCancelled(RotateAroundModifier);
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
            else if (!enabled && ((!rotatingAroundPoint && !dragging) || Selector.Instance.GetActiveInteractable()) && ActionHandler.actions.GodViewMouse.enabled)
            {
                dragging = false;
                dragMomentum = Vector3.zero;
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

        private void RotateAroundModifier(IAction action)
        {
            if (action.Cancelled)
            {
                rotateAroundModifier = false;
            }
            else if (action.Performed)
            {
                rotateAroundModifier = true;
            }
        }

        private void Zoom(IAction action)
        {
            scrollDelta = ActionHandler.actions.GodViewMouse.Zoom.ReadValue<Vector2>().y;

            if (scrollDelta != 0)
            {
                var mousePointerPosition = Mouse.current.position.ReadValue();
                var zoomPoint = cameraComponent.ScreenToWorldPoint(new Vector3(mousePointerPosition.x, mousePointerPosition.y, 1000.0f));
                ZoomInDirection(scrollDelta, zoomPoint);
            }
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
                startMouseDrag = GetPointerPositionInWorld();
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
                CheckRotatingAround();

                if (firstPersonModifier)
                {
                    FirstPersonLook();
                }
                else if (rotatingAroundPoint)
                {
                    RotateAroundPoint();
                }
                else
                {
                    Dragging();
                }
            }
            else
            {
                if (rotatingAroundPoint)
                {
                    RotateAroundPoint();
                }
                else if (!BlockedByTextInput())
                {
                    HandleTranslationInput();
                    HandleRotationInput();
                    HandleFly();
                }
                if (ActionHandler.actions.GodViewMouse.enabled)
                {
                    EazeOutDragVelocity();
                }
            }

            Clamping();
            ClipRangeByDistance();
            //ClampRotation(); //clamping doesnt work because we rotate around point, moving the camera
        }

        /// <summary>
        /// To avoid Z-fighting issues when looking from a distance increase near clipplane distance based on camera height
        /// </summary>
        private void ClipRangeByDistance()
        {
            if(cameraComponent.orthographic)
            {
                cameraComponent.farClipPlane = cameraComponent.transform.position.y + 2000;
            }
            else{
                cameraComponent.farClipPlane = farClipPlane;
            }
            cameraComponent.nearClipPlane = Mathf.Max(0.3f, cameraComponent.transform.position.y / 100f);
        }

        void CheckRotatingAround()
        {
            if (rotateAroundModifier && !rotatingAroundPoint)
            {
                rotatingAroundPoint = true;
                SetFocusPoint();
            }
            else if (!rotateAroundModifier && rotatingAroundPoint)
            {
                rotatingAroundPoint = false;
            }
        }

        /// <summary>
        /// Clamps the camera within the max travel distance bounding box
        /// </summary>
		private void Clamping()
        {
            //Make sure orto cameras only look down, and do not go too low
            if (cameraComponent.orthographic)
            {
                cameraComponent.transform.rotation = Quaternion.LookRotation(Vector3.down,cameraComponent.transform.up);
                this.transform.position = new Vector3(
                    this.transform.position.x, 
                    Mathf.Clamp(cameraComponent.orthographicSize, 100, maxZoomOut), 
                    this.transform.position.z
                );
            }

            this.transform.position = new Vector3(
                Mathf.Clamp(this.transform.position.x, -maxTravelDistance, maxTravelDistance), 
                Mathf.Clamp(this.transform.position.y, minUndergroundY, maxZoomOut), 
                Mathf.Clamp(this.transform.position.z, -maxTravelDistance, maxTravelDistance)
            );
        }

        private bool BlockedByTextInput() {
            var currentSelected = EventSystem.current.currentSelectedGameObject;
            return currentSelected != null && (currentSelected.GetComponent<InputField>() || currentSelected.GetComponent<TMP_InputField>());
        }

        public void MoveAndFocusOnLocation(Vector3 targetLocation, Quaternion targetRotation = new Quaternion())
        {
            cameraComponent.transform.position = targetLocation + cameraOffsetForTargetLocation;
            cameraComponent.transform.LookAt(targetLocation, Vector3.up);

            focusingOnTargetPoint(targetLocation);
        }

        void HandleTranslationInput()
        {
            moveSpeed = Mathf.Sqrt(Mathf.Abs(cameraComponent.transform.position.y)) * speedFactor;
            var heightchange = moveHeightActionKeyboard.ReadValue<float>();
            
            Vector3 movement = moveActionKeyboard.ReadValue<Vector2>();
            if (movement == Vector3.zero && heightchange == 0) return;

            movement.z = movement.y;
            movement.y = heightchange * 0.1f;
            movement = Quaternion.AngleAxis(cameraComponent.transform.eulerAngles.y, Vector3.up) * movement;
            cameraComponent.transform.position += movement * moveSpeed * Time.deltaTime;
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

        private void HandleFly()
        {
            Vector2 val = flyActionGamepad.ReadValue<Vector2>();

            if (val == Vector2.zero) return;

            var newpos = cameraComponent.transform.position += cameraComponent.transform.forward.normalized * val.y * moveSpeed * Time.deltaTime * 0.3f;
            newpos += cameraComponent.transform.right * val.x * moveSpeed * Time.deltaTime * 0.1f;

            if (newpos.y < Config.activeConfiguration.zeroGroundLevelY + 20) newpos.y = Config.activeConfiguration.zeroGroundLevelY + 20;
            cameraComponent.transform.position = newpos;
        }

        private void FirstPersonLook()
        {
            var mouseDelta = Mouse.current.delta.ReadValue();

            //Convert mouse position into local rotations
            currentRotation.x -= mouseDelta.x * firstPersonModifierSpinSpeed * ApplicationSettings.settings.rotateSensitivity;
            currentRotation.y += mouseDelta.y * firstPersonModifierSpinSpeed * ApplicationSettings.settings.rotateSensitivity;

            //Adjust camera rotation
            cameraComponent.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
        }

        /// <summary>
        /// Use a normalized value to set the camera height between floor level and maximum height 
        /// </summary>
        /// <param name="normalizedHeight">Range from 0 to 1</param>
        public void SetNormalizedCameraHeight(float normalizedHeight) {
            var newHeight = Mathf.Lerp(minUndergroundY, maxZoomOut, normalizedHeight);
            cameraComponent.transform.position = new Vector3(cameraComponent.transform.position.x, newHeight, cameraComponent.transform.position.z);
        }

        /// <summary>
        /// Returns the normalized 0 to 1 value of the camera height
        /// </summary>
        /// <returns>Normalized 0 to 1 value of the camera height</returns>
        public float GetNormalizedCameraHeight() {
            return Mathf.InverseLerp(minUndergroundY, maxZoomOut, cameraComponent.transform.position.y);
        }

        //Return value representing camera height in borth ortographic as default 
        public float GetCameraHeight()
        {
            return (cameraComponent.orthographic) ? cameraComponent.orthographicSize : cameraComponent.transform.position.y;
        }

        private void ZoomInDirection(float zoomAmount, Vector3 zoomDirectionPoint)
        {
            var cameraHeight = cameraComponent.transform.position.y; //The higher we are, the faster we zoom       

            //Inverse zoom multiplier when we are below the zoompoint
            if (zoomDirectionPoint.y > cameraComponent.transform.position.y) zoomAmount *= -1;

            if (cameraComponent.orthographic)
            {
                cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize - cameraComponent.orthographicSize * zoomAmount * zoomSpeed, minOrtographicZoom, maxZoomOut);

                //An ortographic camera moves towards the zoom direction point in its own 2D plane
                if (cameraComponent.transform.position.y < maxZoomOut)
                {
                    var localPointPosition = cameraComponent.transform.InverseTransformPoint(zoomDirectionPoint);
                    localPointPosition.z = 0;
                    cameraComponent.transform.Translate(localPointPosition * zoomSpeed * zoomAmount);
                }
            }
            else{
                //A perspective camera moves its position towards to zoom direction point
                zoomDirection = (zoomDirectionPoint - cameraComponent.transform.position).normalized;
                cameraComponent.transform.Translate(zoomDirection * zoomSpeed * zoomAmount * cameraHeight, Space.World);
            }

            focusingOnTargetPoint.Invoke(zoomDirectionPoint);
        }

        private void Dragging()
        {
            if (!ActionHandler.actions.GodViewMouse.enabled) return;

            dragMomentum = (GetPointerPositionInWorld() - startMouseDrag);

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
        public Ray GetMainPointerRay()
        {
            var pointerPosition = this.pointerPosition.ReadValue<Vector2>();
            return cameraComponent.ScreenPointToRay(pointerPosition);
        }
        public Vector3 GetPointerPositionInWorld(Vector3 optionalPositionOverride = default)
        {
            var pointerPosition = this.pointerPosition.ReadValue<Vector2>();
            if (optionalPositionOverride != default) pointerPosition = optionalPositionOverride;

            var screenRay = cameraComponent.ScreenPointToRay(pointerPosition);
            worldPlane.Raycast(screenRay, out float distance);

            var samplePoint = screenRay.GetPoint(Mathf.Min(maxClickDragDistance, distance));
            samplePoint.y = Config.activeConfiguration.zeroGroundLevelY;

            return samplePoint;
        }

        /// <summary>
        /// Resets the camera rotation so it points north
        /// The general modifier key input is used to optionaly also reset the camera so it looks down
        /// </summary>
        public void ResetNorth(bool resetTopDown = false) {

            if (resetNorthTransition != null) StopCoroutine(resetNorthTransition);
            resetNorthTransition = StartCoroutine(TransitionResetToNorth(resetTopDown));
        }

        private IEnumerator TransitionResetToNorth(bool resetTopDown = false) {

            Quaternion newTargetRotation;

            //If modifier is used, also make camera to look down
            if (resetTopDown)
            {
                newTargetRotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            }
            else {
                newTargetRotation = Quaternion.Euler(cameraComponent.transform.eulerAngles.x, 0, cameraComponent.transform.eulerAngles.z);
            }

            //Transition north, with camera looking down
            while (Quaternion.Angle(cameraComponent.transform.rotation, newTargetRotation) > 0.01f)
            {
                cameraComponent.transform.rotation = Quaternion.Lerp(cameraComponent.transform.rotation, newTargetRotation, resetTransitionSpeed);
                yield return new WaitForEndOfFrame();
            }

            //Round it up
            cameraComponent.transform.rotation = newTargetRotation;
            resetNorthTransition = null;
        }

        private void RotateAroundPoint()
        {
            var mouseDelta = Mouse.current.delta.ReadValue();

            var previousPosition = cameraComponent.transform.position;
            var previousRotation = cameraComponent.transform.rotation;

            if(!cameraComponent.orthographic)
                cameraComponent.transform.RotateAround(rotatePoint, cameraComponent.transform.right, -mouseDelta.y * spinSpeed * ApplicationSettings.settings.rotateSensitivity);
            cameraComponent.transform.RotateAround(rotatePoint, Vector3.up, mouseDelta.x * spinSpeed * ApplicationSettings.settings.rotateSensitivity);

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
            if (Physics.Raycast(screenRay, out hit))
            {
                rotatePoint = hit.point;
                focusingOnTargetPoint(rotatePoint);
            }
            else
            {
                rotatePoint = GetPointerPositionInWorld();
                focusingOnTargetPoint(rotatePoint);
            }
        }

        public bool UsesActionMap(InputActionMap actionMap)
        {
            return availableActionMaps.Contains(actionMap);
        }

        public void ToggleOrtographic(bool ortographicOn)
        {
            if (ortographicOn)
            {
                var twoDimensionalUp = cameraComponent.transform.up;
                twoDimensionalUp.y = 0;

                var worldViewCenter = GetPointerPositionInWorld(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                cameraComponent.transform.localEulerAngles = new Vector3(0, cameraComponent.transform.localEulerAngles.y, cameraComponent.transform.localEulerAngles.z);
                localOffsetVector = cameraComponent.transform.InverseTransformPoint(worldViewCenter);

                //Shift camera to this point, but keep same height
                cameraComponent.transform.position = new Vector3(worldViewCenter.x,cameraComponent.transform.position.y, worldViewCenter.z);
                cameraComponent.transform.rotation = Quaternion.LookRotation(cameraComponent.transform.up, twoDimensionalUp);

                //Set orto size based on camera height (to get a similar fov)
                cameraComponent.orthographicSize = cameraComponent.transform.position.y;

                print("Ortographic");
            }
            else{
                var worldViewCenter = GetPointerPositionInWorld(new Vector3(Screen.width / 2, Screen.height / 2, 0));

                cameraComponent.transform.position = worldViewCenter;
                cameraComponent.transform.localEulerAngles = new Vector3(0, cameraComponent.transform.localEulerAngles.y, cameraComponent.transform.localEulerAngles.z);
                cameraComponent.transform.Translate(-localOffsetVector, Space.Self);
                cameraComponent.transform.LookAt(worldViewCenter);
                print("Perspective");                
            }

            cameraComponent.orthographic = ortographicOn;
		}
	}
}