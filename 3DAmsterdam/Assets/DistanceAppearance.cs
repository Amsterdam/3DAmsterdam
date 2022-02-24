using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DistanceAppearance : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image[] images;

    private void Awake()
    {
        images = GetComponentsInChildren<Image>();
        SetTransparency(0.3f);
    }

    private void SetTransparency(float alpha)
    {
        foreach(var image in images)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetTransparency(1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetTransparency(0.3f);
    }
}
