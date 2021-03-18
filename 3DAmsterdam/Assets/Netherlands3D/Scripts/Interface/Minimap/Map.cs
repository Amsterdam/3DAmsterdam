using Netherlands3D.JavascriptConnection;
using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Interface.Minimap
{
    public class Map : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Vector3 dragOffset;

        [SerializeField]
        private MapTiles mapTiles;

        private RectTransform rectTransform;

        [SerializeField]
        private float hoverResizeSpeed = 1.0f;

        [SerializeField]
        private RectTransform dragTarget;
        [SerializeField]
        private RectTransform navigation;

        private Vector2 defaultSize;
        [SerializeField]
        private Vector2 hoverSize;

        private bool dragging = false;

        private bool interactingWithMap = false;

        private void Awake()
        {
            rectTransform = this.GetComponent<RectTransform>();
            defaultSize = rectTransform.sizeDelta;
            navigation.gameObject.SetActive(false);
        }

		private void OnEnable()
		{
            StartCoroutine(InitializeMapTiles());
        }

        IEnumerator InitializeMapTiles(){
            while (!Config.activeConfiguration)
            {
                yield return new WaitForEndOfFrame();
            }

            mapTiles.Initialize(rectTransform, dragTarget);
        }


        void LateUpdate()
        {
            if(!interactingWithMap && mapTiles.Initialized)
            {
                mapTiles.CenterMapOnPointer();
            }
        }

        private void StartedMapInteraction()
        {
            interactingWithMap = true;
            navigation.gameObject.SetActive(true);

            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.POINTER);

            StopAllCoroutines();
            StartCoroutine(HoverResize(hoverSize));
        }

        private void StoppedMapInteraction()
        {
            interactingWithMap = false;
            navigation.gameObject.SetActive(false);

            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.AUTO);

            StopAllCoroutines();
            StartCoroutine(HoverResize(defaultSize));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragging = true;

            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.GRABBING);

            dragOffset = dragTarget.position - Input.mousePosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            mapTiles.ClampInViewBounds(Input.mousePosition + dragOffset);
            mapTiles.LoadTilesInView();
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
                mapTiles.ZoomIn();
            }
            else if (eventData.scrollDelta.y < 0)
            {
                mapTiles.ZoomOut();
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