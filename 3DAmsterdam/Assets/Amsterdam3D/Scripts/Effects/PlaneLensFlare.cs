using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneLensFlare : MonoBehaviour
{
    [SerializeField]
    private SunVisuals sunVisuals;

    [SerializeField]
    private GameObject flareVisualPlane;

    void LateUpdate()
    {
        flareVisualPlane.SetActive(sunVisuals.Day);

        if (sunVisuals.Day){
            this.transform.rotation = sunVisuals.SunDirectionalLight.transform.rotation;
            this.transform.position = CameraModeChanger.Instance.ActiveCamera.transform.position;
        }
    }
}
