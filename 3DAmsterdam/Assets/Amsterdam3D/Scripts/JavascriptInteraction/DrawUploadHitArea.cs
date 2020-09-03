using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Amsterdam3D.JavascriptConnection
{
    public class DrawUploadHitArea : JavascriptMethodCaller
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    private void OnEnable()
    {
        DisplayOBJUploadButtonHitArea(true);
    }
    private void OnDisable()
    {
        DisplayOBJUploadButtonHitArea(false);
    }
#endif
    }
}