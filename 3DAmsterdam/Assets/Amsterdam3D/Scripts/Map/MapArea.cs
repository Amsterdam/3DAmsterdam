using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapArea : MonoBehaviour, IBeginDragHandler,IDragHandler, IScrollHandler
{
    private Vector3 dragOffset;
    private Map map;

    private void Start()
    {
        map = GetComponentInChildren<Map>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragOffset = map.transform.position - Input.mousePosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        map.transform.position = Input.mousePosition + dragOffset;
        map.LoadTilesInView();
    }

    public void OnScroll(PointerEventData eventData)
    {
        if(eventData.scrollDelta.y > 0)
        {
            map.ZoomIn();
        }
        else if (eventData.scrollDelta.y < 0)
        {
            map.ZoomOut();
        }
    }
}
