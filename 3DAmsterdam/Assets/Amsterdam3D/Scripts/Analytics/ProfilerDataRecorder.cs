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
    [Tooltip("Look up Unity hotkeys before changing these values")]
    private KeyCode profilerRecordKey = KeyCode.H;
    [SerializeField]
    [Tooltip("Look up Unity hotkeys before changing these values")]
    private KeyCode profilerStopKey = KeyCode.G;

    Coroutine savingProfilingData;

    public static ProfilerDataRecorder Instance;

    public bool recording = false;

    void Start()
    {
        Instance = this;
        Profiler.enabled = false;
        Profiler.logFile = "";
    }

    public void StartRecording()
    {
        if(!recording)
        {
            savingProfilingData = StartCoroutine(SaveProfilerData());
        }
    }

    public void StopRecording()
    {
        if(savingProfilingData != null)
        {
            StopCoroutine(savingProfilingData);
        }
        Profiler.logFile = "";
        Profiler.enableBinaryLog = false;
        Profiler.enabled = false; 
        recording = false;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(profilerRecordKey))
        {
            StartRecording();
        }
        else if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(profilerStopKey))
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
            if(filePaths.Length != 0)
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
            recording = true;

            // Count 300 frames
            for (int i = 0; i < 300; ++i)
            {

                yield return new WaitForEndOfFrame();

                // Workaround to keep the Profiler working
                if (!Profiler.enabled)
                    Profiler.enabled = true;
            }
        }
    }
}
