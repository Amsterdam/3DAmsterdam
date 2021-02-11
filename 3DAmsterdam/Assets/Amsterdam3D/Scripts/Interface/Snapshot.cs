using Amsterdam3D.CameraMotion;
using System;
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
    private Texture2D screenShot;

    [SerializeField]
    private RenderTexture screenshotRenderTexture;


    private bool takeScreenshotOnNextFrame;
    private IEnumerator screenshotCoroutine;




    private void OnEnable()
    {
        snapshotCamera = transform.GetComponent<Camera>();
        snapshotCamera.transform.position = CameraModeChanger.Instance.ActiveCamera.transform.position;
        snapshotCamera.transform.rotation = CameraModeChanger.Instance.ActiveCamera.transform.rotation;
        screenshotCoroutine = Screenshotting();
    }

    private IEnumerator Screenshotting()
    {
        while(true)
        {
            // Helps with making sure the camera is ready to render
            yield return new WaitForEndOfFrame();
            if (takeScreenshotOnNextFrame)
            {
                takeScreenshotOnNextFrame = false;

                if (screenshotRenderTexture == null)
                {
                    // Creates off-screen render texture that can be rendered into
                    screenshotRenderTexture = new RenderTexture(width, height, 24);
                    screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
                }

                snapshotCamera.targetTexture = screenshotRenderTexture;
                // Calls events on the camera related to rendering
                snapshotCamera.Render();
     
                RenderTexture.active = screenshotRenderTexture;
                screenShot.ReadPixels(new Rect(0, 0, width, height),0, 0);

                // Resets variables
                snapshotCamera.targetTexture = null;
                RenderTexture.active = null;

                byte[] bytes = screenShot.EncodeToPNG();
                string filename = "Screenshot" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss" )+ ".png";
                System.IO.File.WriteAllBytes(filename, bytes);

                // Exits out of loop
                StopCoroutine(screenshotCoroutine);
                gameObject.SetActive(false);
            }
        }

    }
    /// <summary>
    /// Saves the immediate view as a .png in the 3DAmsterdam folder
    /// </summary>
    public void TakeScreenshot()
    {
        gameObject.SetActive(true);
        takeScreenshotOnNextFrame = true;
        StartCoroutine(screenshotCoroutine);
    }
}
