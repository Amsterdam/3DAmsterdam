using Netherlands3D.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneLensFlare : MonoBehaviour
{
    [SerializeField]
    private Transform sun;

    [SerializeField]
    private Transform sunHalo;
    [SerializeField]
    private float haloScale = 1.0f;

    [SerializeField]
    private float offset = 0.01f;

    void LateUpdate()
    {
        this.transform.rotation = sun.rotation;
        this.transform.position = CameraModeChanger.Instance.ActiveCamera.transform.position;

        sunHalo.transform.localPosition = -Vector3.forward * (CameraModeChanger.Instance.ActiveCamera.farClipPlane + (CameraModeChanger.Instance.ActiveCamera.farClipPlane*offset));
        sunHalo.transform.localScale = Vector3.one * CameraModeChanger.Instance.ActiveCamera.farClipPlane * haloScale;
    }
}
