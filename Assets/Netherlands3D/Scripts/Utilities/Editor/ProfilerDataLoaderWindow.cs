#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Profiling;

public class ProfilerDataLoaderWindow : EditorWindow
{
    // This script works entirely in the editor and can be used by going to Window/Analysis/ProfilerDataLoader

    static List<string> cachedFilePaths;
    static int chosenIndex = -1;
    static string[] buttonNames;

    [MenuItem("Window/Analysis/ProfilerDataLoader")]
    static void Init()
    {
        ProfilerDataLoaderWindow window = (ProfilerDataLoaderWindow)EditorWindow.GetWindow(typeof(ProfilerDataLoaderWindow));
        window.minSize = new Vector2(510, 200);
        window.Show();

        ReadProfilerDataFiles();
    }

    static void ReadProfilerDataFiles()
    {
        // make sure the profiler releases the file handle
        // to any of the files we're about to load in
        Profiler.logFile = "";

        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.raw");
        cachedFilePaths = new List<string>();
        buttonNames = new string[filePaths.Length];

        // we want to ignore all of the binary
        // files that end in .data. The Profiler
        // will figure that part out
        Regex test = new Regex(".data$");

        for (int i = 0; i < filePaths.Length; i++)
        {
            string thisPath = filePaths[i];

            Match match = test.Match(thisPath);

            if (!match.Success)
            {
                // not a binary file, add it to the list
                Debug.Log("Found file: " + thisPath);
                cachedFilePaths.Add(thisPath);
                buttonNames[i] = filePaths[i].Substring(Application.persistentDataPath.Length + 1, filePaths[i].Length - Application.persistentDataPath.Length - 4 - 1);
            }
        }

        chosenIndex = -1;
    }

    void OnGUI()
    {
        if (GUILayout.Button("Find Files"))
        {
            ReadProfilerDataFiles();
        }

        if (cachedFilePaths == null)
            return;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Files");

        EditorGUILayout.BeginHorizontal();
        

        // create some styles to organize the buttons, and show
        // the most recently-selected button with red text
        GUIStyle defaultStyle = new GUIStyle(GUI.skin.button);
        defaultStyle.fixedWidth = 100f;

        GUIStyle highlightedStyle = new GUIStyle(defaultStyle);
        highlightedStyle.normal.textColor = Color.red;

        for (int i = 0; i < cachedFilePaths.Count; ++i)
        {

            // list 5 items per row
            if (i % 5 == 0)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }

            GUIStyle thisStyle = null;

            if (chosenIndex == i)
            {
                thisStyle = highlightedStyle;
            }
            else
            {
                thisStyle = defaultStyle;
            }

            if (GUILayout.Button(buttonNames[i], thisStyle))
            {
                Profiler.AddFramesFromFile(cachedFilePaths[i]);

                chosenIndex = i;
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Open File Location"))
        {
            string path = EditorUtility.OpenFilePanel("Choose file to load", Application.persistentDataPath, "*");
            if(path != "")
            {
                Profiler.AddFramesFromFile(path);
            }
        }
    }
}

#endif