using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class MemoryManagement
{
    static MemoryManagement()
    {
        //Reserve a bit more memory at start. (Default is 32M)
        //Please note that this value can automatically multiply, up to the browser max of ~2Gig
        PlayerSettings.WebGL.emscriptenArgs = "-s TOTAL_MEMORY=55MB";
    }
}
