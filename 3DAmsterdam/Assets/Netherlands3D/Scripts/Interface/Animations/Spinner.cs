using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface.Animations
{
    public class Spinner : MonoBehaviour
    {
        [SerializeField]
        private float speed = 1.0f;
    
        void Update()
        {
            this.transform.Rotate(0, 0, speed * Time.deltaTime);
        }
    }
}