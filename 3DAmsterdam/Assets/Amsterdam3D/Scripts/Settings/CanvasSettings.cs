using Amsterdam3D.JavascriptConnection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasSettings : MonoBehaviour
{
    private CanvasScaler canvasScaler;
    private string canvasScaleFactorKey = "canvasScaleFactor";

    [SerializeField]
    private Slider canvasScaleSlider;

    private float referenceWidth = 1920.0f;

    void Start()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        if (PlayerPrefs.HasKey(canvasScaleFactorKey))
        {
            canvasScaler.scaleFactor = PlayerPrefs.GetFloat(canvasScaleFactorKey, 1.0f);
            canvasScaleSlider.value = canvasScaler.scaleFactor;
        }
        else{
            SetPreferableScale();
        }
    }

    [ContextMenu("Clear the stored Canvas settings PlayerPrefs")]
    public void ClearPlayerPrefs(){
        PlayerPrefs.DeleteKey(canvasScaleFactorKey);
    }

    /// <summary>
    /// Scale up the canvas when we have a screen width high DPI like a 4k monitor or ultrawide.
    /// </summary>
    private void SetPreferableScale()
    {
        canvasScaler.scaleFactor = Mathf.Clamp(Screen.width / referenceWidth, 0.0f, 2.0f);
        canvasScaleSlider.value = canvasScaler.scaleFactor;
        JavascriptMethodCaller.SetInterfaceScale(canvasScaler.scaleFactor);
    }

    /// <summary>
    /// Change the canvas scale and store the value in the persistent PlayerPrefs
    /// </summary>
    /// <param name="scaleFactor"></param>
    public void ChangeCanvasScale(float scaleFactor)
    {
        canvasScaler.scaleFactor = scaleFactor;
        PlayerPrefs.SetFloat(canvasScaleFactorKey, scaleFactor);
        JavascriptMethodCaller.SetInterfaceScale(canvasScaler.scaleFactor);
    }
}
