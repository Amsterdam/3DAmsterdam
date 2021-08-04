using Netherlands3D.JavascriptConnection;
using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Interface.Minimap
{
    public class MapViewer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Vector3 dragOffset;

        [SerializeField]
        private RectTransform mapTiles;

        private WMTSMap wmtsMap;

        private RectTransform rectTransform;

        [SerializeField]
        private float hoverResizeSpeed = 1.0f;

        [SerializeField]
        private RectTransform navigation;

        private Vector2 defaultSize;
        [SerializeField]
        private Vector2 hoverSize;

        private bool dragging = false;

        private bool interactingWithMap = false;

        [SerializeField]
        private float maxZoomScale = 6.0f;

        private float minZoomScale = 0.0f;
        private float zoomScale = 0.0f;

        private void Awake()
        {
            zoomScale = minZoomScale;
            wmtsMap = mapTiles.GetComponent<WMTSMap>();
            rectTransform = this.GetComponent<RectTransform>();

            defaultSize = rectTransform.sizeDelta;
            navigation.gameObject.SetActive(false);
        }

        private void StartedMapInteraction()
        {
            interactingWithMap = true;
            navigation.gameObject.SetActive(true);
            wmtsMap.CenterPointerInView = false;
            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.POINTER);

            StopAllCoroutines();
            StartCoroutine(HoverResize(hoverSize));
        }

        private void StoppedMapInteraction()
        {
            interactingWithMap = false;
            navigation.gameObject.SetActive(false);
            wmtsMap.CenterPointerInView = true;
            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.AUTO);

            StopAllCoroutines();
            StartCoroutine(HoverResize(defaultSize));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragging = true;

            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.GRABBING);

            dragOffset = mapTiles.position - Input.mousePosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            mapTiles.transform.position = Input.mousePosition + dragOffset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;

            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.POINTER);
        }
        public void OnScroll(PointerEventData eventData)
        {
            if (eventData.scrollDelta.y > 0)
            {
                ZoomIn();
            }
            else if (eventData.scrollDelta.y < 0)
            {
                ZoomOut();
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            StartedMapInteraction();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!dragging)
            {
                StoppedMapInteraction();
            }
        }

        public void ZoomIn(bool useMousePosition = true)
        {
            if (zoomScale < maxZoomScale)
            {
                zoomScale++;         
                ZoomTowardsLocation(useMousePosition);
                wmtsMap.Zoomed((int)zoomScale);
            }
        }

        public void ZoomOut(bool useMousePosition = true)
        {
            if (zoomScale > minZoomScale)
            {
                zoomScale--;
                ZoomTowardsLocation(useMousePosition);
                wmtsMap.Zoomed((int)zoomScale);
            }
        }
        private void ZoomTowardsLocation(bool useMouse = true)
        {
            var zoomTarget = Vector3.zero;
            if (useMouse)
            {
                zoomTarget = Input.mousePosition;
            }
            else
            {
                zoomTarget = rectTransform.position + new Vector3(-rectTransform.sizeDelta.x * 0.5f, rectTransform.sizeDelta.y * 0.5f);
            }

            ScaleMapOverOrigin(zoomTarget, Vector3.one * Mathf.Pow(2.0f, zoomScale));
        }

        public void ScaleMapOverOrigin(Vector3 scaleOrigin, Vector3 newScale)
        {
            var targetPosition = mapTiles.position;
            var origin = scaleOrigin;
            var newOrigin = targetPosition - origin;
            var relativeScale = newScale.x / mapTiles.localScale.x;
            var finalPosition = origin + newOrigin * relativeScale;

            mapTiles.localScale = newScale;
            mapTiles.position = finalPosition;
        }

        IEnumerator HoverResize(Vector2 targetScale)
        {
            while (Vector2.Distance(targetScale, transform.localScale) > 0.01f)
            {
                rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, targetScale, hoverResizeSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}