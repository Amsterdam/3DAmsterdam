using Netherlands3D.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface
{
    public class Compass : MonoBehaviour
    {
        private Vector3 direction = Vector3.zero;
        void Update()
        {
            direction.z = CameraModeChanger.Instance.ActiveCamera.transform.eulerAngles.y;
            transform.localEulerAngles = direction * -1.0f;
        }
    }
}