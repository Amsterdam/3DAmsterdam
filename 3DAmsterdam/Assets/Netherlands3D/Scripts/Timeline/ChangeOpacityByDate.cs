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

    private void Awake()
    {
        material = this.GetComponent<MeshRenderer>().material;
        if(BaseColor==null) BaseColor = material.color;
    }

    public void TimeChanged(DateTime newTime)
    {
        Color color = BaseColor;
        color.a = (newTime > ObjectDateTime) ? opacityAfter : opacityBefore;
        material.color = color;
    }
}
