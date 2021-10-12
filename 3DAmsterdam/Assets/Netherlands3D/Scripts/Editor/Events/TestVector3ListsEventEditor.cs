using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Vector3ListEvent))]
public class TestVector3ListsEventEditor : Editor
{    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (Vector3ListEvent)target;

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Trigger", GUILayout.Height(40)))
            {
                script.unityEvent?.Invoke(script.testData);
            }
        }
    }
}