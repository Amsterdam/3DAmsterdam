using Netherlands3D.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerspectiveSwitch : MonoBehaviour
{
    [SerializeField]
    private Image perspectiveIcon, ortographicIcon;

    public void ToggleCameraPerspective()
    {
        bool cameraIsPerspective = CameraModeChanger.Instance.CurrentCameraControls.ToggleCameraPerspective();

        perspectiveIcon.gameObject.SetActive(!cameraIsPerspective);
        ortographicIcon.gameObject.SetActive(cameraIsPerspective);
    }
}
