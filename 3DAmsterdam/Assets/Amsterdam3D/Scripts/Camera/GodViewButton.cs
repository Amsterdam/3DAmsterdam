using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amsterdam3D.CameraMotion;
public class GodViewButton : MonoBehaviour
{
    void Start()
    {
        CameraModeChanger.Instance.OnFirstPersonModeEvent += EnableObject;
        CameraModeChanger.Instance.OnGodViewModeEvent += DisableObject;
        gameObject.SetActive(false);
    }
    public void EnableObject() 
    {
        gameObject.SetActive(true);
    }

    public void DisableObject() 
    {
        gameObject.SetActive(false);
    }
}
