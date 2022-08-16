using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Cameras
{

    public class StreetViewMoveToPoint : MonoBehaviour
    {

        public Camera FPSCam;
        private float lerpSpeed = 5f;

        public UnityEvent OnMoved;

        public UnityEvent OnReverseMoved;
        public RotationEvent OnMovedRotation;
        public bool stopMovement = false;
        public GameObject EnableFPSCam() 
        {
            FPSCam.gameObject.SetActive(true);
            return FPSCam.gameObject;
        }
       public IEnumerator MoveToPositionReverse(Transform objectToMove ,Vector3 position, Quaternion rotation) 
        {
            while(objectToMove.position.y < position.y - 2.0f)
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
            while (objectToMove.position.y > position.y + 2.0f)
            {
                if (Input.GetMouseButtonDown(0) || stopMovement) 
                {
                    objectToMove.position = position;
                    objectToMove.rotation = rotation;
                    stopMovement = false;
                    break;
                }
                objectToMove.position = Vector3.Lerp(objectToMove.position, position, lerpSpeed * Time.deltaTime);
                objectToMove.rotation = Quaternion.Lerp(objectToMove.rotation, rotation, lerpSpeed * Time.deltaTime);
                yield return null;
            }
            
            OnMoved.Invoke();
            OnMovedRotation.Invoke(rotation);
        }
        public void StopMovement()
        {
            stopMovement = true;
        }

    }
    [Serializable]
    public class RotationEvent : UnityEvent<Quaternion>
    {
    }
}