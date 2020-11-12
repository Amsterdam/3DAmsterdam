using Amsterdam3D.JavascriptConnection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amsterdam3D.CameraMotion;
public class CanvasSettings : MonoBehaviour
{
    private CanvasScaler canvasScaler;
    private string canvasScaleFactorKey = "canvasScaleFactor";

    [SerializeField]
    private Slider canvasScaleSlider;

    //static scale we can request through class
    public static float canvasScale = 1.0f;

    private float referenceWidth = 1920.0f;

    [SerializeField]
    private GameObject mainMenu;

    [SerializeField]
    private GameObject layers;

    void Start()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        if (PlayerPrefs.HasKey(canvasScaleFactorKey))
        {
            canvasScale = PlayerPrefs.GetFloat(canvasScaleFactorKey, 1.0f);
            canvasScaler.scaleFactor = canvasScale;
            canvasScaleSlider.value = canvasScale;
        }
        else{
            SetPreferableScale();
        }

        CameraModeChanger.Instance.OnFirstPersonModeEvent += DisableObject;
    }

    void DisableObject() 
    {
        layers.SetActive(false);
        mainMenu.SetActive(false);
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
        canvasScale = Mathf.Clamp(Screen.width / referenceWidth, 1.0f, 2.0f);
        canvasScaler.scaleFactor = canvasScale;
        canvasScaleSlider.value = canvasScale;
        JavascriptMethodCaller.SetInterfaceScale(canvasScale);
    }

    /// <summary>
    /// Change the canvas scale and store the value in the persistent PlayerPrefs
    /// </summary>
    /// <param name="scaleFactor"></param>
    public void ChangeCanvasScale(float scaleFactor)
    {
        canvasScale = scaleFactor;
        canvasScaler.scaleFactor = canvasScale;
        PlayerPrefs.SetFloat(canvasScaleFactorKey, canvasScale);
        JavascriptMethodCaller.SetInterfaceScale(canvasScale);
    }
}
