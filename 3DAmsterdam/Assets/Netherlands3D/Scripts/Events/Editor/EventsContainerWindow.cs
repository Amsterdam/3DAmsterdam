using UnityEngine;
using UnityEditor;
using System;

namespace Netherlands3D.Events
{
    public class EventsContainerWindow : EditorWindow
    {
        static string[] floatEventContainers;
        static string[] intEventContainers;
        static string[] triggerEventContainers;
        static string[] stringEventContainers;
        static string[] vector3EventContainers;
        static string[] Vector3ListEventContainers;
        static string[] Vector3ListsEventContainers;

        static string testData = "";
        static string filterName = "";

        static bool onlyShowEventsWithListeners = false;

        const string playerPrefsTestData = "EventsContainerTestData";

        const float invokeButtonHeight = 20;

        Vector2 scrollPosition;
        // Add menu named "My Window" to the Window menu
        [MenuItem("Netherlands 3D/Event Containers Window")]
        static void Init()
        {
            EventsContainerWindow window = (EventsContainerWindow)EditorWindow.GetWindow(typeof(EventsContainerWindow), false, "Events Container Window");
            window.Show();
        }

		private void OnEnable()
		{
            testData = PlayerPrefs.GetString(playerPrefsTestData);
        }

		void OnGUI()
        {
            GUILayout.Label("Event test data", EditorStyles.boldLabel);

            //Store dummy data for our easy testing
            EditorGUI.BeginChangeCheck();
            testData = EditorGUILayout.TextField("", testData);
            if (EditorGUI.EndChangeCheck())
            {
                PlayerPrefs.SetString(playerPrefsTestData, testData);
            }

            GUILayout.Label("Filter", EditorStyles.boldLabel);
            filterName = EditorGUILayout.TextField("", filterName);

            scrollPosition = GUILayout.BeginScrollView(
                scrollPosition);

            //IntEvent
            if (intEventContainers == null) intEventContainers = AssetDatabase.FindAssets("t:IntEvent");
            if (intEventContainers.Length > 0) GUILayout.Label("IntEvent Containers", EditorStyles.boldLabel);
            foreach (var intEventContainer in intEventContainers)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(intEventContainer);

                if (filterName.Length > 0 && assetPath.IndexOf(filterName, StringComparison.OrdinalIgnoreCase) == -1) continue;

                IntEvent asset = (IntEvent)AssetDatabase.LoadAssetAtPath(assetPath, typeof(IntEvent));

                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(asset, typeof(IntEvent), false);
                if (GUILayout.Button("Trigger", GUILayout.Height(invokeButtonHeight)))
                {
                    int floatInput = int.Parse(testData);
                    asset.started?.Invoke(floatInput);
                }
                GUILayout.EndHorizontal();
            }

            //FloatEvent
            if (floatEventContainers == null) floatEventContainers = AssetDatabase.FindAssets("t:FloatEvent");
            if (floatEventContainers.Length > 0) GUILayout.Label("FloatEvent Containers", EditorStyles.boldLabel);
            foreach (var floatEventContainer in floatEventContainers)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(floatEventContainer);

                if (filterName.Length > 0 && assetPath.IndexOf(filterName, StringComparison.OrdinalIgnoreCase) == -1) continue;

                FloatEvent asset = (FloatEvent)AssetDatabase.LoadAssetAtPath(assetPath, typeof(FloatEvent));

                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(asset, typeof(FloatEvent), false);
                if (GUILayout.Button("Trigger", GUILayout.Height(invokeButtonHeight)))
                {
                    float floatInput = float.Parse(testData);
                    asset.started?.Invoke(floatInput);
                }
                GUILayout.EndHorizontal();
            }

            //TriggerEvent
            if (triggerEventContainers == null) triggerEventContainers = AssetDatabase.FindAssets("t:TriggerEvent");
            if (triggerEventContainers.Length > 0) GUILayout.Label("TriggerEvent Containers", EditorStyles.boldLabel);
            foreach (var triggerEventContainer in triggerEventContainers)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(triggerEventContainer);

                if (filterName.Length > 0 && assetPath.IndexOf(filterName, StringComparison.OrdinalIgnoreCase) == -1) continue;
                
                TriggerEvent asset = (TriggerEvent)AssetDatabase.LoadAssetAtPath(assetPath, typeof(TriggerEvent));

                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(asset, typeof(TriggerEvent), false);
                if (GUILayout.Button("Trigger", GUILayout.Height(invokeButtonHeight)))
                {
                    asset.started?.Invoke();
                }
                GUILayout.EndHorizontal();
            }

            //StringEvent
            if (stringEventContainers == null) stringEventContainers = AssetDatabase.FindAssets("t:StringEvent");
            if (stringEventContainers.Length > 0) GUILayout.Label("StringEvent Containers", EditorStyles.boldLabel);
            foreach (var stringEventContainer in stringEventContainers)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(stringEventContainer);

                if (filterName.Length > 0 && assetPath.IndexOf(filterName, StringComparison.OrdinalIgnoreCase) == -1) continue;

                StringEvent asset = (StringEvent)AssetDatabase.LoadAssetAtPath(assetPath, typeof(StringEvent));
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(asset, typeof(StringEvent), false);
                if (GUILayout.Button("Trigger", GUILayout.Height(invokeButtonHeight)))
                {
                    asset.started?.Invoke(testData);
                }
                GUILayout.EndHorizontal();
            }

            //Vector3Event
            if (vector3EventContainers == null) vector3EventContainers = AssetDatabase.FindAssets("t:Vector3Event");
            if (vector3EventContainers.Length > 0) GUILayout.Label("Vector3Event Containers", EditorStyles.boldLabel);
            foreach (var vector3EventContainer in vector3EventContainers)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(vector3EventContainer);

                if (filterName.Length > 0 && assetPath.IndexOf(filterName, StringComparison.OrdinalIgnoreCase) == -1) continue;

                Vector3Event asset = (Vector3Event)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Vector3Event));
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(asset, typeof(Vector3Event), false);
                if (GUILayout.Button("Trigger", GUILayout.Height(invokeButtonHeight)))
                {
                    var vector3string = testData.Split(',');
                    Vector3 vector3 = new Vector3(
                        float.Parse(vector3string[0]),
                        float.Parse(vector3string[1]),
                        float.Parse(vector3string[2])
                    );
                    asset.started?.Invoke(vector3);
                }
                GUILayout.EndHorizontal();
            }

            //Vector3ListEvent
            if (Vector3ListEventContainers == null) Vector3ListEventContainers = AssetDatabase.FindAssets("t:Vector3ListEvent");
            if (Vector3ListEventContainers.Length > 0) GUILayout.Label("Vector3ListEvent Containers", EditorStyles.boldLabel);
            foreach (var Vector3ListEventContainer in Vector3ListEventContainers)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(Vector3ListEventContainer);

                if (filterName.Length > 0 && assetPath.IndexOf(filterName, StringComparison.OrdinalIgnoreCase) == -1) continue;

                Vector3ListEvent asset = (Vector3ListEvent)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Vector3ListEvent));
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(asset, typeof(Vector3ListEvent), false);
                if (GUILayout.Button("Trigger", GUILayout.Height(invokeButtonHeight)))
                {
                    /*var vector3string = testData.Split(',');
                    Vector3 vector3 = new Vector3(
                        float.Parse(vector3string[0]),
                        float.Parse(vector3string[1]),
                        float.Parse(vector3string[2])
                    );
                    asset.unityEvent?.Invoke(vector3);*/
                }
                GUILayout.EndHorizontal();
            }

            //Vector3ListsEvent
            if (Vector3ListsEventContainers == null) Vector3ListsEventContainers = AssetDatabase.FindAssets("t:Vector3ListsEvent");
            if(Vector3ListsEventContainers.Length>0) GUILayout.Label("Vector3ListsEvent Containers", EditorStyles.boldLabel);
            foreach (var Vector3ListsEventContainer in Vector3ListsEventContainers)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(Vector3ListsEventContainer);

                if (filterName.Length > 0 && assetPath.IndexOf(filterName, StringComparison.OrdinalIgnoreCase) == -1) continue;

                Vector3ListsEvent asset = (Vector3ListsEvent)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Vector3ListsEvent));
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(asset, typeof(Vector3ListsEvent), false);
                if (GUILayout.Button("Trigger", GUILayout.Height(invokeButtonHeight)))
                {
                    /*var vector3string = testData.Split(',');
                    Vector3 vector3 = new Vector3(
                        float.Parse(vector3string[0]),
                        float.Parse(vector3string[1]),
                        float.Parse(vector3string[2])
                    );
                    asset.unityEvent?.Invoke(vector3);*/
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}