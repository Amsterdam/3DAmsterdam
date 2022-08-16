using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Cameras;

namespace Netherlands3D.Cameras
{
    public class GodViewButton : MonoBehaviour
    {
        void Start()
        {
            ServiceLocator.GetService<CameraModeChanger>().CameraModeChangedEvent += Instance_CameraModeChangedEvent;

            //ServiceLocator.GetService<CameraModeChanger>().OnFirstPersonModeEvent += EnableObject;
            //ServiceLocator.GetService<CameraModeChanger>().OnGodViewModeEvent += DisableObject;
            gameObject.SetActive(false);
        }

        private void Instance_CameraModeChangedEvent(object source, CameraMode newMode)
        {
            if (newMode == CameraMode.StreetView)
                EnableObject();
            else if(newMode == CameraMode.GodView || newMode == CameraMode.TopDown)
                DisableObject();
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
}