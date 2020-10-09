using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Amsterdam3D.CameraMotion
{

    public class StreetViewMoveToPoint : MonoBehaviour
    {



        private Vector3 endPos;
        private Vector3 mousePos;

        public Camera FPSCam;

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
        }

        public GameObject EnableFPSCam() 
        {
            FPSCam.gameObject.SetActive(true);
            return FPSCam.gameObject;
        }
        



       public IEnumerator MoveToPositionReverse(Transform objectToMove ,Vector3 position, Quaternion rotation) 
        {
            while(objectToMove.position.y < position.y - 0.1f)
            {
                if (Input.GetMouseButtonDown(0)) 
                {
                    objectToMove.rotation = rotation;
                    break;
                }
                
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
                if (Input.GetMouseButton(0)) 
                {
                    objectToMove.position = position;
                    objectToMove.rotation = rotation;
                    break;
                }
                
                objectToMove.position = Vector3.Lerp(objectToMove.position, position, lerpSpeed * Time.deltaTime);
                objectToMove.rotation = Quaternion.Lerp(objectToMove.rotation, rotation, lerpSpeed * Time.deltaTime);
                yield return null;
            }
            OnMoved.Invoke();
            OnMovedRotation.Invoke(rotation);
        }


    }



    [Serializable]
    public class RotationEvent : UnityEvent<Quaternion>
    {

    }
}