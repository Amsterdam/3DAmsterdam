using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class ProfilerDataRecorder : MonoBehaviour
{
    int _count = 0;

    [SerializeField]
    [Header("The name given to saved Profiler recordings when in Editor mode")]
    public string fileName = "FileName";

    [Header("Holding Ctrl + your designated button starts/stops recording")]
    [SerializeField]
    [Tooltip("Look up Unity AND Google Chrome hotkeys before changing these values")]
    private KeyCode profilerRecordKey = KeyCode.Space;

    [SerializeField]
    [Tooltip("Look up Unity Google Chrome hotkeys before changing these values")]
    private KeyCode profilerStopKey = KeyCode.Y;

    Coroutine savingProfilingData;

    public static ProfilerDataRecorder Instance;

    public bool recording = false;

    private bool isBenchmarkToggled = false;

    // The way I would use this array is first define a version of the Benchmark
    // Version 1.0 of [Name of what we are testing]
    // Then have written down what elements the array contains
    // GameObject[0] = Toggle of Building layer
    // GameObject[1] = Snapshot
    // etc.
    public GameObject[] benchmarkItems;

    void Start()
    {
        Instance = this;
        Profiler.enabled = false;
        Profiler.logFile = "";
    }
    /// <summary>
    /// Tells the profiler to start recording performance
    /// </summary>
    public void StartRecording()
    {
        // Avoids starting multiple coroutines
        if (!recording)
        {
            recording = true;
            savingProfilingData = StartCoroutine(SaveProfilerData());
        }
    }
    /// <summary>
    /// Tells the profiler to stop recording performance
    /// </summary>
    public void StopRecording()
    {
        if (savingProfilingData != null)
        {
            StopCoroutine(savingProfilingData);
        }
        Profiler.enabled = false;
        recording = false;
        isBenchmarkToggled = false;
        Profiler.logFile = "";
        Profiler.enableBinaryLog = false;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(profilerRecordKey))
        {
            isBenchmarkToggled = true;
            StartRecording();
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(profilerStopKey))
        {
            StopRecording();
        }
    }

    IEnumerator SaveProfilerData()
    {
        while (true)
        {
#if UNITY_EDITOR
            // Checks for existing files with the same name and makes sure no overwrites happen
            string[] filePaths = Directory.GetFiles(Application.persistentDataPath, fileName + "*");
            if (filePaths.Length != 0)
            {
                int prefix = Application.persistentDataPath.Length + fileName.Length;
                int suffix = ".raw".Length;
                for (int i = 0; i < filePaths.Length; i++)
                {
                    int fileNumber = Convert.ToInt32(filePaths[i].Substring(prefix + 1, filePaths[i].Length - prefix - suffix - 1));
                    if (_count < fileNumber)
                    {
                        _count = fileNumber;
                    }
                }
                _count++;
            }
            else
            {
                _count = 0;
            }

            // Tell the profiler to save your recording and where to save it
            string filepath = Application.persistentDataPath + "/" + fileName + _count;
            Profiler.logFile = filepath;
            Profiler.enableBinaryLog = true;
#endif
            Profiler.enabled = true;
            if (isBenchmarkToggled)
            {
                // Count 300 frames (MAX) and execute commands that together form the 'Benchmark'
                // You can make the Benchmark 10 frames if you want to
                // Wrapping code with Profiler.BeginSample and EndSample is entirely optional, it makes debugging easier
                // case 300 is the last one that is interractable
                // The last line should always call StopRecording() or else the profiler will continue recording endlessly while overwriting older data
                for (int i = 1; i <= 301; ++i)
                {
                    yield return new WaitForEndOfFrame();

                    switch (i)
                    {
                        // Within this switch case is where programming of the Benchmark should be made
                        // When choosing a case number just use your FPS as a reference (300fps = 1 second of Benchmarking)
                        // Example #1
                        //case 1:
                        //    Profiler.BeginSample("B.DisableLayers");
                        //    benchmarkItems[0].GetComponent<Toggle>().isOn = false;
                        //    benchmarkItems[1].GetComponent<Toggle>().isOn = false;
                        //    Profiler.EndSample();
                        //    break;

                        // Example #2
                        //case 2:
                        //    Profiler.BeginSample("B.MoveToUnloadedArea");
                        //    CameraModeChanger.Instance.ActiveCamera.transform.position = new Vector3(5000, 200, -6500);
                        //    Profiler.EndSample();
                        //    break;

                        case 300:
                            Profiler.BeginSample("B.FrameEND");
                            StopRecording();
                            Profiler.EndSample();
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                // Count 300 frames
                // This loop is used by classes that call this instance by themselves for individual feature testing
                // For features that take over 300 frames this only works in Editor mode and not on WebGL
                for (int i = 1; i <= 301; ++i)
                {
                    yield return new WaitForEndOfFrame();

                    // Workaround to keep the Profiler working
                    if (!Profiler.enabled)
                        Profiler.enabled = true;
                }
            }
        }
    }
}