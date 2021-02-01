using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amsterdam3D.CameraMotion;

namespace Amsterdam3D.Interface
{
    public class SaveFirstPersonLocation : MonoBehaviour
    {
        [SerializeField]
        private PlaceCustomObject firstPersonLocationPlacer;

        void Start()
        {
            //todo, make these event handlers all a seperate monobehaviour
            CameraModeChanger.Instance.OnFirstPersonModeEvent += EnableObject;
            CameraModeChanger.Instance.OnGodViewModeEvent += DisableObject;
            gameObject.SetActive(false);
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
            Vector3 currentCameraPosition = CameraModeChanger.Instance.ActiveCamera.transform.position;
            Quaternion currentCameraRotation = CameraModeChanger.Instance.ActiveCamera.transform.rotation;

            firstPersonLocationPlacer.SpawnNewObjectAtPointer("Camera positie");
            FirstPersonLocation firstPersonLocation = firstPersonLocationPlacer.SpawnedObject.GetComponent<FirstPersonLocation>();
            firstPersonLocation.WorldPointerFollower.WorldPosition = currentCameraPosition;
            firstPersonLocation.savedRotation = currentCameraRotation;
            firstPersonLocation.waitingForClick = false;
        }
    }
}