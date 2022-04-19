using Netherlands3D.Cameras;
using Netherlands3D.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D
{
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

        public Toggle snapshotUI;
        private bool snapshotPreferenceUI = true;
        public Toggle snapshotNavigation;
        private bool snapshotPreferenceNavigation;
        public Toggle snapshotLogo;
        private bool snapshotPreferenceLogo = true;
        public Toggle snapshotMainMenu;
        private bool snapshotPreferenceMainMenu;
        public Toggle snapshotLoD;
        private bool snapshotPreferenceLoD;


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

            if (snapshotResolution.text != "")
            {
                // no resolutionSeparator -> × means not a number×number resolution
                if (snapshotResolution.text.Contains(resolutionSeparator))
                {
                    width = Convert.ToInt32(snapshotResolution.text.Substring(0, snapshotResolution.text.IndexOf(resolutionSeparator)));
                    height = Convert.ToInt32(snapshotResolution.text.Substring(snapshotResolution.text.IndexOf(resolutionSeparator) + 1));
                }
                else
                {
                    width = Screen.width;
                    height = Screen.height;
                }
            }

            if (snapshotFileType.text != "")
            {
                fileType = snapshotFileType.text.ToLower();
            }

            // If the user tries to take a snapshot twice without giving it a name the variable won't reset
            // That's why we do it here
            fileName = "";

            if (snapshotName.text != "")
            {
                fileName = snapshotName.text;
            }

            // After taking a screenshot the toggles for UI that the user set get saved
            snapshotPreferenceUI = snapshotUI.isOn;
            snapshotPreferenceNavigation = snapshotNavigation.isOn;
            snapshotPreferenceLogo = snapshotLogo.isOn;
            snapshotPreferenceMainMenu = snapshotMainMenu.isOn;
            snapshotPreferenceLoD = snapshotLoD.isOn;

            snapshotSettings.SetActive(false);

            snapshotCamera = transform.GetComponent<Camera>();
            snapshotCamera.transform.position = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position;
            snapshotCamera.transform.rotation = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.rotation;


            screenshotCoroutine = Screenshotting();
#if UNITY_EDITOR
            screenshotCoroutine = ScreenshottingEditor();
#endif
        }

        private IEnumerator Screenshotting()
        {
            while (true)
            {
                // Helps with making sure the camera is ready to render
                yield return new WaitForEndOfFrame();

                if (takeScreenshotOnNextFrame)
                {
                    takeScreenshotOnNextFrame = false;

                    RenderCameraToTexture();

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

                    switch (fileType)
                    {
                        case "png":
                            bytes = screenShot.EncodeToPNG();
                            break;
                        case "jpg":
                            bytes = screenShot.EncodeToJPG();
                            break;
                        case "raw":
                            bytes = screenShot.GetRawTextureData();
                            break;
                        default:
                            bytes = screenShot.EncodeToPNG();
                            break;
                    }

                    Analytics.SendEvent("Snapshot",
                        new Dictionary<string, object>
                        {
                            { "FileType", fileType }
                        }
                    );

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
            while (true)
            {
                // Helps with making sure the camera is ready to render
                yield return new WaitForEndOfFrame();

                if (takeScreenshotOnNextFrame)
                {
                    takeScreenshotOnNextFrame = false;

                    RenderCameraToTexture();

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
                        switch (fileType)
                        {
                            case "png":
                                bytes = screenShot.EncodeToPNG();
                                break;
                            case "jpg":
                                bytes = screenShot.EncodeToJPG();
                                break;
                            case "raw":
                                bytes = screenShot.GetRawTextureData();
                                break;
                            default:
                                bytes = screenShot.EncodeToPNG();
                                break;
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


        private void RenderCameraToTexture()
        {
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
            snapshotNavigation.isOn = true;
            snapshotLogo.isOn = true;
            snapshotMainMenu.isOn = true;
            snapshotLoD.isOn = true;

            // If no filetype is given, make it a png
            if (fileType == "")
            {
                fileType = "png";
            }
        }

        /// <summary>
        /// Updates the toggles based on the previous snapshot settings
        /// </summary>
        public void UpdateFields()
        {
            snapshotUI.isOn = snapshotPreferenceUI;
            snapshotNavigation.isOn = snapshotPreferenceNavigation;
            snapshotLogo.isOn = snapshotPreferenceLogo;
            snapshotMainMenu.isOn = snapshotPreferenceMainMenu;
            snapshotLoD.isOn = snapshotPreferenceLoD;
        }


        /// <summary>
        /// Saves the immediate view with user defined parameters
        /// </summary>
        public void TakeScreenshot()
        {
            // Allows the camera to see what is on the canvas if user wants to see UI
            if (snapshotUI.isOn)
            {
                responsiveCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                responsiveCanvas.worldCamera = snapshotCamera;
                responsiveCanvas.planeDistance = snapshotCamera.nearClipPlane + 0.1f;
            }
            gameObject.SetActive(true);
            takeScreenshotOnNextFrame = true;
            StartCoroutine(screenshotCoroutine);
        }
    }
}