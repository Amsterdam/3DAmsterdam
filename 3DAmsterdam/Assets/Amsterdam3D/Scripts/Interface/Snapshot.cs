 using Amsterdam3D.CameraMotion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
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
    private Texture2D screenShot;

    [SerializeField]
    private RenderTexture screenshotRenderTexture;

    [SerializeField]
    private String fileType;

    [SerializeField]
    private String fileName;

    private const string resolutionSeparator = "×";

    public GameObject snapshotSettings;
    public Toggle snapshotMainMenu;
    public Text snapshotResolution;
    public Text snapshotFileType;
    public Text snapshotName;

    public Canvas responsiveCanvas;
    private bool takeScreenshotOnNextFrame;
    private IEnumerator screenshotCoroutine;

    [DllImport("__Internal")]
    private static extern void DownloadFile(byte[] array, int byteLength, string fileName);

    private void OnEnable()
    {

        if(snapshotResolution.text != "")
        {
            // no resolutionSeparator -> × means not a number×number resolution
            if (snapshotResolution.text.Contains(resolutionSeparator))
            {
                width = Convert.ToInt32(snapshotResolution.text.Substring(0, snapshotResolution.text.IndexOf(resolutionSeparator)));
                height = Convert.ToInt32(snapshotResolution.text.Substring(snapshotResolution.text.IndexOf(resolutionSeparator)+1));
            }
            else
            {
                width = Screen.width;
                height = Screen.height;
            }
        }

        if(snapshotFileType.text != "")
        {
            fileType = snapshotFileType.text.ToLower();
        }

        // If the user tries to take a snapshot twice without giving it a name the variable won't reset
        // That's why we do it here
        fileName = "";

        if(snapshotName.text != "")
        {
            fileName = snapshotName.text;
        }

        snapshotSettings.SetActive(false);

        snapshotCamera = transform.GetComponent<Camera>();
        snapshotCamera.transform.position = CameraModeChanger.Instance.ActiveCamera.transform.position;
        snapshotCamera.transform.rotation = CameraModeChanger.Instance.ActiveCamera.transform.rotation;


        screenshotCoroutine = Screenshotting();
#if UNITY_EDITOR
        screenshotCoroutine = ScreenshottingEditor();
#endif
    }

    private IEnumerator Screenshotting()
    {
        while(true)
        {
            // Helps with making sure the camera is ready to render
            yield return new WaitForEndOfFrame();   
            // A trick to make sure the UI gets rendered to the screenshot
            yield return new WaitForEndOfFrame();
            if (takeScreenshotOnNextFrame)
            {
                takeScreenshotOnNextFrame = false;
                
                // Creates off-screen render texture that can be rendered into
                screenshotRenderTexture = new RenderTexture(width, height, 24);
                screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);

                snapshotCamera.targetTexture = screenshotRenderTexture;
                RenderTexture.active = screenshotRenderTexture;

                // Calls events on the camera related to rendering
                snapshotCamera.Render();          
                
                // Resets canvas
                responsiveCanvas.worldCamera = null;
                responsiveCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                screenShot.Apply();

                // Resets variables
                snapshotCamera.targetTexture = null;
                RenderTexture.active = null;
                snapshotMainMenu.isOn = true;


                // If no filetype is given, make it a png
                if (fileType == "")
                {
                    fileType = ".png";
                }

                // Default filename
                if (fileName == "")
                {
                    fileName = "Screenshot_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "." + fileType;
                }
                else
                {
                    fileName = fileName + "." + fileType;
                }

                byte[] bytes = null;
                if (fileType == "png")
                {
                    bytes = screenShot.EncodeToPNG();
                }
                else if (fileType == "jpg")
                {
                    bytes = screenShot.EncodeToJPG();
                }
                else if (fileType == "raw")
                {
                    bytes = screenShot.GetRawTextureData();
                }

                DownloadFile(bytes, bytes.Length, fileName);

                // Exits out of loop
                StopCoroutine(screenshotCoroutine);
                gameObject.SetActive(false);
            }
        }

    }


#if UNITY_EDITOR

    private IEnumerator ScreenshottingEditor()
    {
        while(true)
        {
            // Helps with making sure the camera is ready to render
            yield return new WaitForEndOfFrame();
            // A trick to make sure the UI gets rendered to the screenshot
            yield return new WaitForEndOfFrame();
            if (takeScreenshotOnNextFrame)
            {
                takeScreenshotOnNextFrame = false;

                // Creates off-screen render texture that can be rendered into
                screenshotRenderTexture = new RenderTexture(width, height, 24);
                screenShot = new Texture2D(width, height , TextureFormat.RGB24, false);
                    
                snapshotCamera.targetTexture = screenshotRenderTexture;
                RenderTexture.active = screenshotRenderTexture;

                // Calls events on the camera related to rendering
                snapshotCamera.Render();

                // Resets canvas
                responsiveCanvas.worldCamera = null;
                responsiveCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                screenShot.Apply();

                // Resets variables
                snapshotCamera.targetTexture = null;
                RenderTexture.active = null;
                snapshotMainMenu.isOn = true;

                // If no filetype is given, make it a png
                if (fileType == "")
                {
                    fileType = "png";
                }

                // Default filename
                if (fileName == "")
                {
                    fileName = "Screenshot_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                }

                // Window for user to input desired path/name/filetype
                string path = EditorUtility.SaveFilePanel("Save texture as PNG", "", fileName, fileType);

                // If user pressed cancel nothing happens
                if (path.Length != 0)
                {
                    byte[] bytes = null;
                    if (fileType == "png")
                    {
                        bytes = screenShot.EncodeToPNG();
                    }
                    else if (fileType == "jpg")
                    {
                        bytes = screenShot.EncodeToJPG();
                    }
                    else if (fileType == "raw")
                    {
                        bytes = screenShot.GetRawTextureData();
                    }
                    File.WriteAllBytes(path, bytes);
                }

                // Exits out of loop
                StopCoroutine(screenshotCoroutine);
                gameObject.SetActive(false);  
            }
        }

    }
#endif

    /// <summary>
    /// Saves the immediate view with user defined parameters
    /// </summary>
    public void TakeScreenshot()
    {
        responsiveCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        responsiveCanvas.worldCamera = snapshotCamera;
        gameObject.SetActive(true);
        takeScreenshotOnNextFrame = true;
        StartCoroutine(screenshotCoroutine);
    }
}
