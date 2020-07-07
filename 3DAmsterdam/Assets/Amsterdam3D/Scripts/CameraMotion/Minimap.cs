using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ConvertCoordinates;
using UnityEngine.EventSystems;

namespace Amsterdam3D.Interface
{
    public class Minimap : MonoBehaviour, IPointerClickHandler, IDragHandler
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

        private void Start()
        {
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
            RotatePointerByCameraDirection();
        }

        private void PositionPointer()
        {
            var posX = Mathf.InverseLerp(bottomLeftUnityCoordinates.x, topRightUnityCoordinates.x, Camera.main.transform.position.x);
            var posY = Mathf.InverseLerp(bottomLeftUnityCoordinates.z, topRightUnityCoordinates.z, Camera.main.transform.position.z);
            minimapPointer.anchorMin = minimapPointer.anchorMax = new Vector3(posX, posY, 0);
        }

        private void RotatePointerByCameraDirection()
        {
            direction.z = Camera.main.transform.eulerAngles.y;
            minimapPointer.localEulerAngles = direction * -1.0f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            ConvertClickToMovePosition(eventData);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            ConvertClickToMovePosition(eventData);
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

            var interpolatedWidth = mapImage.rect.width * 0.5f;
            var interpolatedHeight = mapImage.rect.height * 0.5f;

            localPositionInRectTransform = new Vector2(
                Mathf.InverseLerp(-interpolatedWidth, interpolatedWidth, localPositionInRectTransform.x),
                Mathf.InverseLerp(-interpolatedHeight, interpolatedHeight, localPositionInRectTransform.y)
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
            Camera.main.transform.position = new Vector3(clickedWorldPositionX, Camera.main.transform.position.y, clickedWorldPositionZ);
        }
    }
}