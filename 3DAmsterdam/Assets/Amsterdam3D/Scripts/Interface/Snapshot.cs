using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Snapshot : MonoBehaviour
{
    [SerializeField]
    private int width = 1920;
    [SerializeField]
    private int height = 1080;

    [SerializeField]
    private Camera snapshotCamera;

    [SerializeField]
    private RawImage screenshot;

    [SerializeField]
    private RenderTexture screenshotRenderTexture;

    void OnEnable()
    {
       /* Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        snapshotCamera.Render();
        RenderTexture.active = screenshotRenderTexture;
        screenshotRenderTexture.tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        snapshotCamera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(screenshotRenderTexture);
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = "Screenshot";
        System.IO.File.WriteAllBytes(filename, bytes);*/
    }

    
}
