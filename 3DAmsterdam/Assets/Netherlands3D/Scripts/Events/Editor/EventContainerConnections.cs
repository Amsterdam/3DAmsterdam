using UnityEngine;
using UnityEditor;
using System;

namespace Netherlands3D.Events
{
    [CustomEditor(typeof(EventContainer<>),true)]
    public class EventContainerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            base.OnInspectorGUI();


        }
    }
}