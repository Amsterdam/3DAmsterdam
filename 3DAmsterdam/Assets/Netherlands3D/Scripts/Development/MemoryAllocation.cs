using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Netherlands3D.MemoryManagement
{
    /// <summary>
    /// This class forces allocation of the WebGL build heap
    /// Scene loads force a full GC, so we start with our emptied min. heap size
    /// </summary>
    public class MemoryAllocation : MonoBehaviour
    {
        [SerializeField]
        private int allocateMemory = 400;

        [SerializeField]
        private string allocVariableName = "prealloc=";

        void Awake()
        {
            StartCoroutine(AllocateSequence());
        }

        private IEnumerator AllocateSequence()
        {
            if (Application.absoluteURL.Contains(allocVariableName))
            {
                allocateMemory = int.Parse(Application.absoluteURL.Split('=')[1]);
                Allocate(allocateMemory);
                Debug.Log($"Preallocated {allocateMemory}mb memory");

                yield return new WaitForEndOfFrame();
            }

            Debug.Log($"Loading main scene..");
            SceneManager.LoadScene(1,LoadSceneMode.Single);
        }

        private void Allocate(int megabyte)
        {
            byte[] buffer = new byte[megabyte * 1024 * 1024];
            buffer = null;
        }
    }
}