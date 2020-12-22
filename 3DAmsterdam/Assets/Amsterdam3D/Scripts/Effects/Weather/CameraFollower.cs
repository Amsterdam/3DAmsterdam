using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    
    void Update()
    {
        this.transform.position = CameraModeChanger.Instance.ActiveCamera.transform.position;
        this.transform.rotation = CameraModeChanger.Instance.ActiveCamera.transform.rotation;
    }
}
