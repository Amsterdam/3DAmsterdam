using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Events.GenericListeners
{
    public class ObjectEventListener : MonoBehaviour
    {
        [SerializeField]
        private ObjectEvent objectEvent;

        [SerializeField]
        private ObjectValueUnityEvent onEvent;

        void Awake()
        {
            objectEvent.started.AddListener(ObjectReceived);
        }

        void ObjectReceived(Object receivedObject)
        {
            onEvent.Invoke(receivedObject);
        }
    }
}