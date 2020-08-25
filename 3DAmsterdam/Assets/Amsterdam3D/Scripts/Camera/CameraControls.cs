using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;
using System;
using Amsterdam3D.FreeShape;
using ConvertCoordinates;

namespace Amsterdam3D.CameraMotion
{
    public class CameraControls : MonoBehaviour
    {
        private Camera camera;

        public float zoomSpeed = 0.5f;
        private const float maxZoomOut = 2500f;

        private const float rotationSpeed = 1f;

        private const float minAngle = -10f;
        private const float maxAngle = 89f;
        private const float speedFactor = 0.5f;

        private float momentumBreak = 10.0f;
        public float dragFactor = 2f;
        private const float rotationSensitivity = 5f;

        private const float floorOffset = 1.8f;

        private Vector3 startMouseDrag;

        [SerializeField]
        private Vector3 cameraOffsetForTargetLocation = new Vector3(100,100,200);

        private float scrollDelta;
  
        private float moveSpeed;
        private float mouseHorizontal;
        private float mouseVertical;


        private bool translationModifier = false;
        private bool firstPersonModifier = false;

        private bool canUseFunction = true;

        public bool LockFunctions = false;

		private bool interactionOverruled = false;

        private Vector3 zoomPoint;
        private Vector3 zoomDirection;

        private Vector3 dragOrigin;
        private Vector3 moveDirection;
        private Vector3 rotatePoint;

        private Vector2 currentRotation;

        public delegate void FocusPointChanged(Vector3 pointerPosition);
        public static FocusPointChanged focusPointChanged;

        private Plane worldPlane = new Plane(Vector3.up, new Vector3(0, Constants.ZERO_GROUND_LEVEL_Y, 0));

        void Awake()
        {
            camera = GetComponent<Camera>();
        }

        void Start()
        {
            currentRotation = new Vector2(camera.transform.rotation.eulerAngles.y, camera.transform.rotation.eulerAngles.x);
        }

        void Update()
		{
			if (InteractionOverruled()) return;

			// checkt of de muis oven een UI element zit (zo ja, dan kunnen bepaalde functies niet gebruikt worden)
			if (EventSystem.current.IsPointerOverGameObject() || LockFunctions)
			{
				canUseFunction = false;
			}
			else
			{
				canUseFunction = true;
			}

            translationModifier = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            firstPersonModifier = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (!Input.GetKey(KeyCode.Space))
            {
                HandleTranslationInput();
                HandleRotationInput();

                if (canUseFunction)
                {
                    Zooming();
                    Dragging();
                    FocusPoint();
                }
            }
		}

		private bool InteractionOverruled()
		{
			if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject() || ObjectManipulation.manipulatingObject)
			{
				interactionOverruled = true;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				interactionOverruled = false;
			}
			return interactionOverruled;
		}

		public void MoveAndFocusOnLocation(Vector3 targetLocation)
		{
            Camera.main.transform.position = targetLocation + cameraOffsetForTargetLocation;
            Camera.main.transform.LookAt(targetLocation, Vector3.up);

            focusPointChanged(targetLocation);
        }

        public void ChangedPointFromMinimap(string latLong)
        {
            string[] coordinates = latLong.Split(',');
            var lat = double.Parse(coordinates[0]);
            var lon = double.Parse(coordinates[1]);

            var convertedCoordinate = CoordConvert.WGS84toUnity(lon, lat);
            this.transform.position = new Vector3(convertedCoordinate.x, this.transform.position.y, convertedCoordinate.z);
        }

        void HandleTranslationInput()
        {         
            moveSpeed = Mathf.Sqrt(camera.transform.position.y) * speedFactor;

            if (!translationModifier)
            {
                // de directie wordt gelijk gezet aan de juiste directie plus hoeveel de camera gedraaid is
                moveDirection = Quaternion.AngleAxis(camera.transform.eulerAngles.y, Vector3.up) * Vector3.forward;

                // vooruit/achteruit bewegen (gebaseerd op rotatie van camera)
                if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) camera.transform.position += MoveForward();
                if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) camera.transform.position -= MoveBackward();

                // zijwaarts bewegen (gebaseerd op rotatie van camera)
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) camera.transform.position -= MoveLeft();
                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) camera.transform.position += MoveRight();
            }
        }
        private Vector3 MoveRight()
        {
            return camera.transform.right * moveSpeed;
        }
        private Vector3 MoveLeft()
        {
            return camera.transform.right * moveSpeed;
        }
        private Vector3 MoveBackward()
        {
            return moveDirection * moveSpeed;
        }
        private Vector3 MoveForward()
        {
            return moveDirection * moveSpeed;
        }

        private void HandleRotationInput()
        {
            currentRotation = new Vector2(camera.transform.rotation.eulerAngles.y, camera.transform.rotation.eulerAngles.x);

            if ((translationModifier && Input.GetKey(KeyCode.LeftArrow)) || Input.GetKey(KeyCode.Q))
            {
                camera.transform.RotateAround(camera.transform.position, Vector3.up, -rotationSpeed);
            }
            else if ((translationModifier && Input.GetKey(KeyCode.RightArrow)) || Input.GetKey(KeyCode.E)){
                camera.transform.RotateAround(camera.transform.position, Vector3.up, rotationSpeed);
            }

            if ((translationModifier && Input.GetKey(KeyCode.UpArrow)) || (!translationModifier && Input.GetKey(KeyCode.R)))
            {
                camera.transform.RotateAround(camera.transform.position, camera.transform.right, -rotationSpeed);
            }
            if ((translationModifier && Input.GetKey(KeyCode.DownArrow)) || (!translationModifier && Input.GetKey(KeyCode.F))) 
            { 
                camera.transform.RotateAround(camera.transform.position, camera.transform.right, rotationSpeed);
            }
            //Middle mouse rotation
            if (firstPersonModifier && Input.GetMouseButton(0))
            {
                mouseHorizontal = Input.GetAxis("Mouse X");
                mouseVertical = Input.GetAxis("Mouse Y");

                // berekent rotatie van camera gebaseerd op beweging van muis
                currentRotation.x += mouseHorizontal * rotationSensitivity;
                currentRotation.y -= mouseVertical * rotationSensitivity;

                // de rotatie blijft tussen de 0 en 360 graden
                currentRotation.x = Mathf.Repeat(currentRotation.x, 360f);

                // zorgt dat de rotatie niet verder kan dan de min en max angle
                currentRotation.y = Mathf.Clamp(currentRotation.y, minAngle, maxAngle);

                //// de rotatie van de camera wordt aangepast
                camera.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
            }

            ClampRotation();
        }

        private void ClampRotation()
        {
            camera.transform.rotation = Quaternion.Euler(new Vector3(
                ClampAngle(camera.transform.eulerAngles.x, minAngle, maxAngle),
                camera.transform.eulerAngles.y, camera.transform.eulerAngles.z));
        }

        /// <summary>
        /// Use a normalized value to set the camera height between floor level and maximum height 
        /// </summary>
        /// <param name="normalizedHeight">Range from 0 to 1</param>
        public void SetNormalizedCameraHeight(float normalizedHeight){
            var newHeight = Mathf.Lerp(floorOffset, maxZoomOut, normalizedHeight);
            camera.transform.position = new Vector3(camera.transform.position.x, newHeight, camera.transform.position.z);

            ClampToGround();
        }

        /// <summary>
        /// Returns the normalized 0 to 1 value of the camera height
        /// </summary>
        /// <returns>Normalized 0 to 1 value of the camera height</returns>
        public float GetNormalizedCameraHeight(){
            return Mathf.InverseLerp(floorOffset, maxZoomOut, camera.transform.position.y);
        }

        public float GetCameraHeight()
        {
            return camera.transform.position.y;
        }

        private void ClampToGround()
        {
            Debug.DrawRay(camera.transform.position, Vector3.down * floorOffset, Color.red);
            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position, Vector3.down * floorOffset, out hit)){ 
                if (hit.distance < floorOffset)
                {
                    camera.transform.position = hit.point + new Vector3(0, floorOffset, 0);
                }
            }
        }

        private Vector3 mouseZoomStart;
        private Vector3 mouseZoomTarget;
        private float mouseZoomSpeed = 0.01f;
        private void Zooming()
        {
            if (translationModifier)
            {
                if (Input.GetKey(KeyCode.R))
                {
                    var zoomPoint = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000.0f));
                    ZoomInDirection(Time.deltaTime, zoomPoint);
                }
                else if (Input.GetKey(KeyCode.F))
                {
                    var zoomPoint = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000.0f));
                    ZoomInDirection(-Time.deltaTime, zoomPoint);
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                startMouseDrag = Input.mousePosition;
                mouseZoomTarget = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000.0f));
                mouseZoomStart = camera.transform.position;
            }
            else if (Input.GetMouseButton(1))
            {
                camera.transform.position = mouseZoomStart;
                ZoomInDirection(-(Input.mousePosition.y - startMouseDrag.y) * mouseZoomSpeed, mouseZoomTarget);
            }
            else
            {
                scrollDelta = Input.GetAxis("Mouse ScrollWheel");
                if (scrollDelta != 0)
                {
                    var zoomPoint = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000.0f));
                    ZoomInDirection(scrollDelta, zoomPoint);
                }
            }   
        }

        private void ZoomInDirection(float zoomAmount, Vector3 zoomDirectionPoint)
        {
            var heightSpeed = camera.transform.position.y; //The higher we are, the faster we zoom
            zoomDirection = (zoomDirectionPoint - camera.transform.position).normalized;

            camera.transform.Translate(zoomDirection * zoomSpeed * zoomAmount * heightSpeed, Space.World);
        }

        private Vector3 dragMomentum = Vector3.zero;
        private void Dragging()
        {
            if (!translationModifier && !firstPersonModifier)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    startMouseDrag = GetMousePositionInWorld();
                }
                else if (Input.GetMouseButton(0))
                {
                    dragMomentum = (GetMousePositionInWorld() - startMouseDrag);
                    transform.position -= dragMomentum;
                }
                else {
                    //Slide forward in dragged direction
                    dragMomentum = Vector3.Lerp(dragMomentum, Vector3.zero, Time.deltaTime * momentumBreak);
                    if (dragMomentum.magnitude > 0.0f)
                    {
                        this.transform.position -= dragMomentum;
                    }
                }
            }
        }

        private Vector3 GetMousePositionInWorld()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);            
            worldPlane.Raycast(ray, out float distance);
            return ray.GetPoint(distance);
        }

        private void FocusPoint()
        {
            RaycastHit hit;
            var ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Input.GetMouseButtonDown(2))
            {
                //Check if a collider is under our mouse, if not get a point on NAP~0
                if (Physics.Raycast(ray, out hit))
                {
                    rotatePoint = hit.point;
                    focusPointChanged(rotatePoint);
                }
                else if (new Plane(Vector3.up, new Vector3(0.0f, Constants.ZERO_GROUND_LEVEL_Y, 0.0f)).Raycast(ray, out float enter)){
                    rotatePoint = ray.GetPoint(enter);
                    focusPointChanged(rotatePoint);
                }
            }

            if (Input.GetMouseButton(2))
            {
                var mouseX = Input.GetAxis("Mouse X");
                var mouseY = Input.GetAxis("Mouse Y");

                camera.transform.RotateAround(rotatePoint, camera.transform.right, -mouseY * 5f);
                camera.transform.RotateAround(rotatePoint, Vector3.up, mouseX * 5f);

                currentRotation = new Vector2(camera.transform.rotation.eulerAngles.y, camera.transform.rotation.eulerAngles.x);
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