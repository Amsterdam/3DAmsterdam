using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GizmoDragButton : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Toggle Toggle { get; private set; }
    public bool IsDragging { get; private set; }

    private void Awake()
    {
        Toggle = GetComponent<Toggle>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Toggle.isOn = true;
        IsDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;
    }
}
