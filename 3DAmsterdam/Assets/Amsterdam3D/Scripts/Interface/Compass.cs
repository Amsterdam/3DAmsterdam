using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    private Vector3 direction = Vector3.zero;
    void Update()
    {
        direction.z = CameraModeChanger.instance.CurrentCameraComponent.transform.eulerAngles.y;
        transform.localEulerAngles = direction * -1.0f;
    }
}
