using Amsterdam3D.CameraMotion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public class ProfilerDataRecorder : MonoBehaviour
{
    int _count = 0;

    [SerializeField]
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
            Instance.StartRecording();
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(profilerStopKey))
        {
            Instance.StopRecording();
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
            
            // Generate the file path
            string filepath = Application.persistentDataPath + "/" + fileName + _count;
            // Set the log file and enable the profiler
            Profiler.logFile = filepath;
            Profiler.enableBinaryLog = true;
#endif
            Profiler.enabled = true;
            if (isBenchmarkToggled)
            {
                // Count 300 frames and execute commands that together form the 'Benchmark'
                // Wrapping code with Profiler.BeginSample and EndSample is entirely optional, it makes debugging easier
                // case 300 is the last one that is interractable
                // The last line should always call StopRecording() or else the profiler will continue recording endlessly
                for (int i = 1; i <= 301; ++i)
                {
                    yield return new WaitForEndOfFrame();

                    switch (i)
                    {
                        case 1:
                            Profiler.BeginSample("B.Frame1");
                            CameraModeChanger.Instance.ActiveCamera.transform.position = new Vector3(1000, 150, 1000);
                            Profiler.EndSample();
                            break;
                        case 280:
                            Profiler.BeginSample("B.Frame280");
                            CameraModeChanger.Instance.ActiveCamera.transform.position = new Vector3(1500, 300, -3900);
                            Profiler.EndSample();
                            break;
                        case 300:
                            Profiler.BeginSample("B.FrameEND");
                            Instance.StopRecording();
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
                // This loop is used by classes that call this instance by themselves for individual testing
                // For features that take over 300 frames this only works in Editor mode
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
