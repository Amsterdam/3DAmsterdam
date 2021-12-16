using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Events.GenericListeners
{
    public class LookAtPosition : MonoBehaviour
    {
        [SerializeField]
        private Vector3Event lookAtPosition;

        [SerializeField]
        private Vector3 offset = Vector3.down;

        void Awake()
        {
            lookAtPosition.started.AddListener(LookAt);
        }

        void LookAt(Vector3 position)
        {
            this.transform.position = position + offset;
            this.transform.LookAt(position);
        }
    }
}