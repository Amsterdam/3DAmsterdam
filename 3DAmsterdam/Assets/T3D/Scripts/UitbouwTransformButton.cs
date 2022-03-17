using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UitbouwTransformButton : MonoBehaviour, IPointerDownHandler
{
    public bool IsDragging { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsDragging = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
            IsDragging = false;
    }
}
