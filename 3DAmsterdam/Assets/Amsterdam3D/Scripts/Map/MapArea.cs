using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapArea : MonoBehaviour, IBeginDragHandler,IDragHandler, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 dragOffset;
    private Map map;

    private RectTransform rectTransform;

    [SerializeField]
    private float hoverResizeSpeed = 1.0f;

    [SerializeField]
    private RectTransform pointer;
    [SerializeField]
    private RectTransform navigation;

    private Vector2 defaultSize;
    [SerializeField]
    private Vector2 hoverSize;

    private void Awake()
    {
        rectTransform = this.GetComponent<RectTransform>();
    }

    private void Start()
    {
        map = GetComponentInChildren<Map>();
        defaultSize = rectTransform.sizeDelta;
        navigation.gameObject.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragOffset = map.transform.position - Input.mousePosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        map.transform.position = Input.mousePosition + dragOffset;

        //pointer //Move pointer to right position

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
