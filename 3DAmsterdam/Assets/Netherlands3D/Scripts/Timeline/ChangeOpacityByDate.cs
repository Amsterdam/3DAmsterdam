using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeOpacityByDate : MonoBehaviour
{
    private DateTime appearDateTime = new DateTime(2000,1,1);
    private DateTime disappearDateTime = DateTime.MaxValue;
    public DateTime AppearDateTime { get => appearDateTime; set => appearDateTime = value; }
    public DateTime DisappearDateTime { get => disappearDateTime; set => disappearDateTime = value; }
    public Color BaseColor { get => baseColor; set => baseColor = value; }

    private float opacityDefault = 1.0f;
    private float opacityBefore = 0.5f;
    private float opacityAfter = 1.0f;

    private Material material;
    private Renderer renderer;
    private Color baseColor;

    public void ApplyBaseColor(Color color)
    {
        renderer = this.GetComponent<MeshRenderer>();
        material = renderer.material;
        color.a = opacityDefault;
        material.color = color;
    }

    public void TimeChanged(DateTime newTime)
    {
        Color color = material.color;
        color.a = ((newTime > AppearDateTime) && (newTime < DisappearDateTime)) ? opacityAfter : opacityBefore;
        renderer.enabled = (color.a > 0);

        material.color = color;
    }

    internal void SetOpacityRange(float opacityDefault, float opacityBefore, float opacityAfter)
    {
        this.opacityDefault = opacityDefault;
        this.opacityBefore = opacityBefore;
        this.opacityAfter = opacityAfter;
    }
}
