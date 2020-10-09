using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amsterdam3D.CameraMotion;
public class GodviewButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CameraModeChanger.instance.OnFirstPersonModeEvent += EnableObject;
        CameraModeChanger.instance.OnGodViewModeEvent += DisableObject;
        gameObject.SetActive(false);
        Button button = GetComponent<Button>();
        button.onClick.AddListener(CameraModeChanger.instance.GodViewMode);
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
