using Netherlands3D.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerspectiveSwitch : MonoBehaviour
{
    [SerializeField]
    private Image perspectiveIcon, ortographicIcon;

    void Start(){
        CameraModeChanger.Instance.OnGodViewModeEvent += EnableObject;
        CameraModeChanger.Instance.OnFirstPersonModeEvent += DisableObject;
    }

    private void EnableObject()
    {
        gameObject.SetActive(true);
    }
    private void DisableObject()
    {
        gameObject.SetActive(false);
    }

    public void ToggleCameraPerspective()
    {
        bool cameraIsPerspective = CameraModeChanger.Instance.TogglePerspective();

        perspectiveIcon.gameObject.SetActive(!cameraIsPerspective);
        ortographicIcon.gameObject.SetActive(cameraIsPerspective);
    }
}
