using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DistanceAppearance : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Text[] texts;
    private Image[] images;
    private float zoomMultiplier = 1f;
    private float notHoveringImageAlpha = 0.3f;
    private float notHoveringTextAlpha = 1f;
    private float fadeStartDistance = 25f;
    private float fadeEndDistance = 30f;
    private float textTargetAlpha = 1f;
    private float imageTargetAlpha = 1f;

    private bool isHovering;

    private void Awake()
    {
        texts = GetComponentsInChildren<Text>();
        images = GetComponentsInChildren<Image>();
        SetTargetTransparency(notHoveringTextAlpha, notHoveringImageAlpha);
    }

    private void SetTransparency(float textAlpha, float imageAlpha)
    {
        foreach (var text in texts)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, textAlpha);
        }

        foreach (var image in images)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, imageAlpha);
        }
    }

    private void SetTargetTransparency(float newTextTargetAlpha, float newImageTargetAlpha)
    { 
        textTargetAlpha = newTextTargetAlpha;
        imageTargetAlpha = newImageTargetAlpha;
        SetTransparency(textTargetAlpha * zoomMultiplier, imageTargetAlpha * zoomMultiplier);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    private void Update()
    {
        var dist = Vector3.Distance(RotateCamera.CameraTargetPoint, ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position);

        zoomMultiplier = Mathf.InverseLerp(fadeEndDistance, fadeStartDistance, dist);

        if (isHovering || GetComponent<NumberInputField>().IsSelected)
        {
            SetTargetTransparency(1f, 1f);
        }
        else
        {
            SetTargetTransparency(notHoveringTextAlpha, notHoveringImageAlpha);
        }
    }
}
