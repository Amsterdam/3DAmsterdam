using Amsterdam3D.CameraMotion;
using ConvertCoordinates;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Amsterdam3D.Interface
{
    public class Minimap : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private Vector3 direction;

        Vector3 bottomLeftUnityCoordinates, topRightUnityCoordinates, centerPosition, startPosition;
       
        [SerializeField]
        private RectTransform minimapPointer;

        [SerializeField]
        private RectTransform mapImage;

        private const float bottomLeftLat = 52.261480f;
        private const float bottomLeftLong = 4.727386f;
        private const float topRightLat = 52.454227f;
        private const float topRightLong = 5.108260f;

        [SerializeField]
        private float scaleSpeed = 10.0f;

        private Vector2 defaultScale;

        [SerializeField]
        private float zoomMargin = 60;
        private Vector2 zoomScreenScale;

        private bool holdAndZoom = false;

        [SerializeField]
        private bool moveCameraOnDrag = false;

        private void Start()
        {
            defaultScale = transform.localScale;
            startPosition = transform.localPosition;
            CalculateMapCoordinates();
        }

        private void CalculateMapCoordinates()
        {
            bottomLeftUnityCoordinates = CoordConvert.WGS84toUnity(bottomLeftLong, bottomLeftLat);
            topRightUnityCoordinates = CoordConvert.WGS84toUnity(topRightLong, topRightLat);
        }

        void LateUpdate()
        {
            AlignPointerToCamera();
        }

        private void AlignPointerToCamera()
        {
            PositionPointer();
            RotatePointerToCameraLookDirection();
        }

        private void PositionPointer()
        {
            var posX = Mathf.InverseLerp(bottomLeftUnityCoordinates.x, topRightUnityCoordinates.x, CameraModeChanger.Instance.ActiveCamera.transform.position.x);
            var posY = Mathf.InverseLerp(bottomLeftUnityCoordinates.z, topRightUnityCoordinates.z, CameraModeChanger.Instance.ActiveCamera.transform.position.z);
            minimapPointer.anchorMin = minimapPointer.anchorMax = new Vector3(posX, posY, 0);
        }

        private void RotatePointerToCameraLookDirection()
        {
            direction.z = CameraModeChanger.Instance.ActiveCamera.transform.eulerAngles.y;
            minimapPointer.localEulerAngles = direction * -1.0f;
        }

        IEnumerator TransitionToScale(Vector2 targetScale)
        {
            while(Vector2.Distance(targetScale,transform.localScale) > 0.01f)
            {
                transform.localScale = Vector2.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
                yield return null;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != 0) return;

            if(!holdAndZoom)
            {
                holdAndZoom = true;

                zoomScreenScale.x = zoomScreenScale.y = (Screen.height-zoomMargin) / mapImage.rect.height;
                                
                StopAllCoroutines();
                StartCoroutine(TransitionToScale(zoomScreenScale));
            }

            ConvertClickToMovePosition(eventData);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != 0) return;

            ConvertClickToMovePosition(eventData);            
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != 0) return;

            ConvertClickToMovePosition(eventData);

            holdAndZoom = false;
            StopAllCoroutines();
            StartCoroutine(TransitionToScale(defaultScale));
        }

        /// <summary>
        /// Convert the position we clicked on the map, to the move location for our camera.
        /// </summary>
        /// <param name="eventData">The pointer event data containing our click position</param>
        private void ConvertClickToMovePosition(PointerEventData eventData)
        {
            Vector2 localPositionInRectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(mapImage, eventData.position, eventData.pressEventCamera, out localPositionInRectTransform))
                return;

            var interpolatedWidth = mapImage.rect.width;
            var interpolatedHeight = mapImage.rect.height;

            localPositionInRectTransform = new Vector2(
                Mathf.InverseLerp(-interpolatedWidth, 0.0f, localPositionInRectTransform.x),
                Mathf.InverseLerp(0.0f, interpolatedHeight, localPositionInRectTransform.y)
            );
            MoveToLocationOnMap(localPositionInRectTransform);
        }
        /// <summary>
        /// Move the main camera to where we clicked on this map
        /// </summary>
        /// <param name="locationOnRectTransform">The click location of this recttransform</param>
        public void MoveToLocationOnMap(Vector3 locationOnRectTransform){
            var clickedWorldPositionX = Mathf.Lerp(bottomLeftUnityCoordinates.x, topRightUnityCoordinates.x, locationOnRectTransform.x);
            var clickedWorldPositionZ = Mathf.Lerp(bottomLeftUnityCoordinates.z, topRightUnityCoordinates.z, locationOnRectTransform.y);
            CameraModeChanger.Instance.ActiveCamera.transform.position = new Vector3(clickedWorldPositionX, CameraModeChanger.Instance.ActiveCamera.transform.position.y, clickedWorldPositionZ);
        }
    }
}