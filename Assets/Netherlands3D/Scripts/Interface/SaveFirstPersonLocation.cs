using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Cameras;

namespace Netherlands3D.Interface
{
    public class SaveFirstPersonLocation : MonoBehaviour
    {
        [SerializeField]
        private PlaceCustomObject firstPersonLocationPlacer;

        void Start()
        {
            //todo, make these event handlers all a seperate monobehaviour
            ServiceLocator.GetService<CameraModeChanger>().CameraModeChangedEvent += Instance_CameraModeChangedEvent;
            gameObject.SetActive(false);
        }

        private void Instance_CameraModeChangedEvent(object source, CameraMode newMode)
        {
            if (newMode == CameraMode.GodView || newMode == CameraMode.TopDown)
                DisableObject();
            else if (newMode == CameraMode.StreetView)
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

        public void SaveCurrentPosition()
        {
            Vector3 currentCameraPosition = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position;
            Quaternion currentCameraRotation = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.rotation;

            //Use the same spawner we use to manualy place the camera point, but skip the waiting for a click part.
            firstPersonLocationPlacer.SpawnNewObjectAtPointer("Camera positie");
            FirstPersonLocation firstPersonLocation = firstPersonLocationPlacer.SpawnedObject.GetComponent<FirstPersonLocation>();
            firstPersonLocation.waitingForClick = false;

            //Set properties to current camera position
            firstPersonLocation.WorldPointerFollower.WorldPosition = currentCameraPosition;
            firstPersonLocation.savedRotation = currentCameraRotation;
            firstPersonLocationPlacer.SpawnedObject.SetActive(false);
        }
    }
}