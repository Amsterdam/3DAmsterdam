using Netherlands3D.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface
{
    public class Compass : MonoBehaviour
    {
        private Vector3 direction = Vector3.zero;

        [SerializeField]
        private bool counterRotate = false;
        
        void Update()
        {
            direction.z = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.eulerAngles.y;
            if (counterRotate)
                direction.z *= -1.0f;

            transform.localEulerAngles = direction;
        }

        public void ResetCameraToNorth()
        {
            print("Reset camera to north");
            var resetEuler = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.eulerAngles;
            resetEuler.y = 0;
            ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.eulerAngles = resetEuler;
        }
    }
}