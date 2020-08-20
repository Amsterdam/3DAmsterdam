using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapArea : MonoBehaviour, IBeginDragHandler,IDragHandler,IPointerClickHandler, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 dragOffset;
    private Map map;

    private RectTransform rectTransform;

    [SerializeField]
    private float hoverResizeSpeed = 1.0f;

    [SerializeField]
    private RectTransform dragTarget;
    [SerializeField]
    private RectTransform pointer;
    [SerializeField]
    private RectTransform navigation;

    private Vector2 defaultSize;
    [SerializeField]
    private Vector2 hoverSize;
    [SerializeField]
    private Vector3 bottomLeftUnityCoordinates, topRightUnityCoordinates;

    private void Awake()
    {
        rectTransform = this.GetComponent<RectTransform>();
        CalculateMapCoordinates();

        map = GetComponentInChildren<Map>();
        map.SetMapAreas(rectTransform,dragTarget);
        defaultSize = rectTransform.sizeDelta;
        navigation.gameObject.SetActive(false);
    }

    void Update()
    {
        PositionPointer();
    }
    private void CalculateMapCoordinates()
    {
        bottomLeftUnityCoordinates = CoordConvert.RDtoUnity(new Vector3(Constants.MINIMAP_RD_BOTTOMLEFT_X, Constants.MINIMAP_RD_BOTTOMLEFT_Y, 0.0f));
        topRightUnityCoordinates = CoordConvert.RDtoUnity(new Vector3(Constants.MINIMAP_RD_BOTTOMLEFT_X + Constants.MINIMAP_BASE_SPAN, Constants.MINIMAP_RD_BOTTOMLEFT_Y + Constants.MINIMAP_BASE_SPAN, 0.0f));
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        dragOffset = dragTarget.position - Input.mousePosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragTarget.position = Input.mousePosition + dragOffset;
        map.LoadTilesInView();
    }
    public void OnScroll(PointerEventData eventData)
    {
        if (eventData.scrollDelta.y > 0)
        {
            map.ZoomIn();
        }
        else if (eventData.scrollDelta.y < 0)
        {
            map.ZoomOut();
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked mimimap at:" + eventData.position);
    }
    private void PositionPointer()
    {
        var posX = Mathf.InverseLerp(bottomLeftUnityCoordinates.x, topRightUnityCoordinates.x, Camera.main.transform.position.x);
        var posY = Mathf.InverseLerp(bottomLeftUnityCoordinates.z, topRightUnityCoordinates.z, Camera.main.transform.position.z);

        pointer.anchorMin = pointer.anchorMax = new Vector3(posX, posY, 0);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        navigation.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(HoverResize(hoverSize));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        navigation.gameObject.SetActive(false);

        StopAllCoroutines();
        StartCoroutine(HoverResize(defaultSize));
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
