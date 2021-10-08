using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Events
{
    [CreateAssetMenu(fileName = "TriggerEvent", menuName = "ScriptableObjects/Events/TriggerEvent", order = 0)]
    [System.Serializable]
    public class TriggerEvent : ScriptableObjectEvent
    {
        public UnityEvent floatEvent;
    }
}
