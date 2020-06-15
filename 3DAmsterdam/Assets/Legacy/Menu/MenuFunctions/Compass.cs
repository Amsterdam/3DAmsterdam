using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Interface
{
    public class Compass : MonoBehaviour
    {
        private Vector3 direction;

        void Update()
        {
            ChangeImageForwardToCameraForward();
        }

        private void ChangeImageForwardToCameraForward()
        {
            direction.z = Camera.main.transform.eulerAngles.y;
            transform.localEulerAngles = direction;
        }
    }
}