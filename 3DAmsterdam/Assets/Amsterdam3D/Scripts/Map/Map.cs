using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Map : MonoBehaviour, IBeginDragHandler,IDragHandler,IPointerClickHandler, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
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
    private RectTransform pointer;
    [SerializeField]
    private RectTransform navigation;

    private Vector2 defaultSize;
    [SerializeField]
    private Vector2 hoverSize;

    private void Start()
    {
        rectTransform = this.GetComponent<RectTransform>();
        defaultSize = rectTransform.sizeDelta;
        navigation.gameObject.SetActive(false);

        mapTiles.Initialize(rectTransform, dragTarget);
    }

    void Update()
    {
        PositionPointer();
    }

    private void PositionPointer()
    {
        var posX = Mathf.InverseLerp(mapTiles.BottomLeftUnityCoordinates.x, mapTiles.TopRightUnityCoordinates.x, Camera.main.transform.position.x);
        var posY = Mathf.InverseLerp(mapTiles.BottomLeftUnityCoordinates.z, mapTiles.TopRightUnityCoordinates.z, Camera.main.transform.position.z);

        pointer.localPosition = new Vector3(posX * 256 * 3.0f * mapTiles.transform.localScale.x, posY * 256 * 3.0f * mapTiles.transform.localScale.y, 0.0f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragOffset = dragTarget.position - Input.mousePosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragTarget.position = Input.mousePosition + dragOffset;
        //map.LoadTilesInView();
    }
    public void OnScroll(PointerEventData eventData)
    {
        if (eventData.scrollDelta.y > 0)
        {
            mapTiles.ZoomIn();
            pointer.localScale = Vector3.one / dragTarget.localScale.x;
        }
        else if (eventData.scrollDelta.y < 0)
        {
            mapTiles.ZoomOut();
            pointer.localScale = Vector3.one / dragTarget.localScale.x;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked mimimap at:" + eventData.position);
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
