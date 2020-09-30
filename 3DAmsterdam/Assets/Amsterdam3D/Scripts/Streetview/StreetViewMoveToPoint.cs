using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Amsterdam3D.CameraMotion
{

    public class StreetViewMoveToPoint : MonoBehaviour
    {
        private bool moveToStreet = false;
        private bool canClick = false;
        private bool instanCam = false;
       // public GameObject streetcamButton;
      //  public GameObject GodviewButton;

        private Vector3 endPos;
        private Vector3 mousePos;

        public Camera FPSCam;
       // public CameraControls camcontroller;

        private float stopHeight = 60f;
        private float lerpSpeed = 3f;
        private Quaternion Endrotation;

        private Vector3 oldPosition;
        private Quaternion oldRotation;

        public UnityEvent OnMoved;

        public UnityEvent OnReverseMoved;
        public RotationEvent OnMovedRotation;


        private void Awake()
        {
            
        }

        private void LateUpdate()
        {
        
            /*   
            // de stoppositie is waar geklikt is met een bepaalde afstand van de grond (y-axis)
                endPos = new Vector3(mousePos.x, mousePos.y + 1.8f, mousePos.z);
                Endrotation = Quaternion.Euler(0, GodViewCam.transform.rotation.eulerAngles.y, 0);
                canClick = false;
                moveToStreet = true;
                oldPosition = GodViewCam.transform.position;
                oldRotation = GodViewCam.transform.rotation;
                FPSCam.transform.position = GodViewCam.transform.position;
                FPSCam.transform.rotation = GodViewCam.transform.rotation;
                FPSCam.gameObject.SetActive(true);
                GodViewCam.gameObject.SetActive(false);
                streetcamButton.SetActive(false);
                GodviewButton.SetActive(true);
            StartCoroutine(MoveToPosition(FPSCam.transform, endPos, Endrotation)); */


        }

        public GameObject EnableFPSCam() 
        {
            FPSCam.gameObject.SetActive(true);
            return FPSCam.gameObject;
        }
        
        public void StreetCam()
        {

            canClick = true;
            instanCam = true;
        }




       public IEnumerator MoveToPositionReverse(Transform objectToMove ,Vector3 position, Quaternion rotation) 
        {
            while(objectToMove.position.y < position.y - 0.1f)
            {
                objectToMove.position = Vector3.Lerp(objectToMove.position, position, lerpSpeed * Time.deltaTime);
                objectToMove.rotation = Quaternion.Lerp(objectToMove.rotation, rotation, lerpSpeed * Time.deltaTime);
                yield return null;
            }
            OnReverseMoved.Invoke();
        }

      public  IEnumerator MoveToPosition(Transform objectToMove, Vector3 position, Quaternion rotation)
        {
            //small hack to make sure quaterion.lerp actually does something
            if (rotation.eulerAngles.x == 0 && rotation.eulerAngles.y == 0 && rotation.eulerAngles.z == 0) 
            {
                rotation = Quaternion.Euler(0.1f, 0.1f, 0.1f);
            }
            
            while (objectToMove.position.y > position.y + 0.1f)
            {
                objectToMove.position = Vector3.Lerp(objectToMove.position, position, lerpSpeed * Time.deltaTime);
                objectToMove.rotation = Quaternion.Lerp(objectToMove.rotation, rotation, lerpSpeed * Time.deltaTime);
                yield return null;
            }
            moveToStreet = false;
            instanCam = false;
            OnMoved.Invoke();
            OnMovedRotation.Invoke(rotation);
        }


    }



    [Serializable]
    public class RotationEvent : UnityEvent<Quaternion>
    {

    }
}