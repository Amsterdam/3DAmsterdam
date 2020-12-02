using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerFollower : MonoBehaviour
{
    void Update()
    {
        if (CameraModeChanger.Instance.CameraMode == CameraMode.GodView)
            this.transform.position = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
    }    
}
