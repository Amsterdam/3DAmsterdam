using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw.BoundaryFeatures;
using UnityEngine;
using UnityEngine.UI;

public class BoundaryFeatureLabelInfo : MonoBehaviour
{
    [SerializeField]
    private Text featureName, featureSize; 

    public void SetInfo(BoundaryFeature bf)
    {
        featureName.text = bf.DisplayName;
        featureSize.text = FormatSize(bf.Size);
    }

    private string FormatSize(Vector2 size)
    {
        var x = Mathf.RoundToInt(size.x * 100f);
        var y = Mathf.RoundToInt(size.y * 100f);
        return x + " x " + y;
    }
}
