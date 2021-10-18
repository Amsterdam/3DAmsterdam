using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Netherlands3D.Events
{
    [CustomEditor(typeof(StringEvent))]
    public class TestStringEventEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = (StringEvent)target;

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Trigger", GUILayout.Height(40)))
                {
                    //script.unityEvent?.Invoke(script.testData);
                }
            }
        }
    }
}