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
using TMPro;
namespace Netherlands3D
{
    public class Snapshot : MonoBehaviour
    {
        [SerializeField] private int width = 1920;
        [SerializeField] private int height = 1080;

        [SerializeField] private Camera snapshotCamera;

        private Graphic[] panelGraphics;
        private Canvas[] canvases;

        private Texture2D screenShot;
        private RenderTexture screenshotRenderTexture;

        [SerializeField] private string fileType;
        [SerializeField] private string fileName;

        private const string resolutionSeparator = "×";

        public Toggle snapshotUI;
        private bool snapshotPreferenceUI = true;
        public Toggle snapshotNavigation;
        private bool snapshotPreferenceNavigation;
        public Toggle snapshotMainMenu;
        private bool snapshotPreferenceMainMenu;

        public TextMeshProUGUI snapshotResolution;
        public TextMeshProUGUI snapshotFileType;
        public TextMeshProUGUI snapshotName;

        private bool takeScreenshotOnNextFrame;
        private IEnumerator screenshotCoroutine;

        /// <summary>
        /// JS plugins are the *.jslib files found in the project.
        /// </summary>
        [DllImport("__Internal")]
        private static extern void DownloadFile(byte[] array, int byteLength, string fileName);

        private bool ignoredFirstStart = false;

		private void Awake()
		{
            panelGraphics = this.GetComponentsInChildren<Graphic>();
            canvases = FindObjectsOfType<Canvas>();
            screenshotCoroutine = Screenshotting();
#if UNITY_EDITOR
            screenshotCoroutine = ScreenshottingEditor();
#endif
        }

        private void OnEnable()
        {
            if (!ignoredFirstStart)
            {
                ignoredFirstStart = true;
                return;
            }

            SaveCurrentToggleStates();
            EnablePanelGraphics(true);
            UpdateFields();            
        }

        private void EnablePanelGraphics(bool enabled = true)
        {
            foreach (var renderer in panelGraphics) renderer.enabled = enabled;
		}

        public void SaveSettings()
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

            SaveCurrentToggleStates();

            //Align snapshot camera with our own active camera
            snapshotCamera.transform.position = CameraModeChanger.Instance.ActiveCamera.transform.position;
            snapshotCamera.transform.rotation = CameraModeChanger.Instance.ActiveCamera.transform.rotation;
        }

        /// <summary>
        /// Store current state of toggle elements
        /// </summary>
        private void SaveCurrentToggleStates()
        {
            // After taking a screenshot the toggles for UI that the user set get saved
            snapshotPreferenceUI = snapshotUI.isOn;
            snapshotPreferenceNavigation = snapshotNavigation.isOn;
            snapshotPreferenceMainMenu = snapshotMainMenu.isOn;
        }

        private IEnumerator Screenshotting()
        {
            while (true)
            {
                EnablePanelGraphics(false);

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

                    Analytics.SendEvent("Snapshot", fileType, $"{width}x{height}");
    
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
                EnablePanelGraphics(false);

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
            snapshotCamera.fieldOfView = CameraModeChanger.Instance.ActiveCamera.fieldOfView;
            snapshotCamera.orthographic = CameraModeChanger.Instance.ActiveCamera.orthographic;
            snapshotCamera.orthographicSize = CameraModeChanger.Instance.ActiveCamera.orthographicSize;
            RenderTexture.active = screenshotRenderTexture;

            //Hide these panel renderers
            Graphic[] allGraphics = GetComponentsInChildren<Graphic>();
            foreach (var graphic in allGraphics) graphic.enabled = false;

            // Calls events on the camera related to rendering
            snapshotCamera.Render();

            // Resets canvas
            foreach (var canvas in canvases)
            {
                canvas.worldCamera = null;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();

            // Resets variables
            snapshotCamera.targetTexture = null;
            RenderTexture.active = null;
            snapshotNavigation.isOn = true;
            snapshotMainMenu.isOn = true;

            // Cleanup textures
            Destroy(screenshotRenderTexture);
            Destroy(screenShot);

            //Show this panel again
            foreach (var graphic in allGraphics) graphic.enabled = true;

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
            snapshotMainMenu.isOn = snapshotPreferenceMainMenu;
        }


        /// <summary>
        /// Saves the immediate view with user defined parameters
        /// </summary>
        public void TakeScreenshot()
        {
            SaveSettings();

            // Allows the camera to see what is on the canvas if user wants to see UI
            if (snapshotUI.isOn)
            {
                foreach (var canvas in canvases)
                {
                    canvas.worldCamera = snapshotCamera;
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.planeDistance = snapshotCamera.nearClipPlane + 0.1f;
                }
            }
            gameObject.SetActive(true);
            takeScreenshotOnNextFrame = true;
            StartCoroutine(screenshotCoroutine);
        }
    }
}