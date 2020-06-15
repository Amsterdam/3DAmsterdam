using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Interface
{
    public class Compass : MonoBehaviour
    {
        private Vector3 direction;

        void LateUpdate()
        {
            RotateByCameraDirection();
        }

        private void RotateByCameraDirection()
        {
            direction.z = Camera.main.transform.eulerAngles.y;
            transform.localEulerAngles = direction;
        }
    }
}