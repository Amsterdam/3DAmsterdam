using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeOpacityByDate : MonoBehaviour
{
    private DateTime objectDateTime = new DateTime(2000,1,1);
    public DateTime ObjectDateTime { get => objectDateTime; set => objectDateTime = value; }
    public Color BaseColor { get => baseColor; set => baseColor = value; }

    public float opacityBefore = 0.3f;
    public float opacityAfter = 0.8f;

    private Material material;

    private Color baseColor;

    public void ApplyBaseColor(Color color)
    {
        material = this.GetComponent<MeshRenderer>().material;
        color.a = opacityBefore;
        material.color = color;
    }

    public void TimeChanged(DateTime newTime)
    {
        Color color = material.color;
        color.a = (newTime > ObjectDateTime) ? opacityAfter : opacityBefore;
        material.color = color;
    }
}
