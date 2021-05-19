using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
class EnableThreads
{
    static EnableThreads()
    {
        PlayerSettings.WebGL.threadsSupport = true;
    }
}