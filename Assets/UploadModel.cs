using System.Collections;
using System.IO;
using UnityEngine;
using WebGLFileUploader;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace WebGLFileUploaderExample
{
    /// <summary>
    /// File Upload example.
    /// </summary>
    public class UploadModel : MonoBehaviour, IDragHandler
    {
        private int x, y, w, h;
        private bool isVisible;
        // Use this for initialization
        void Start()
        {
            Debug.Log("WebGLFileUploadManager.getOS: " + WebGLFileUploadManager.getOS);
            Debug.Log("WebGLFileUploadManager.isMOBILE: " + WebGLFileUploadManager.IsMOBILE);
            Debug.Log("WebGLFileUploadManager.getUserAgent: " + WebGLFileUploadManager.GetUserAgent);

            WebGLFileUploadManager.SetDebug(true);
            if (
#if UNITY_WEBGL && !UNITY_EDITOR
                    WebGLFileUploadManager.IsMOBILE 
#else
                    Application.isMobilePlatform
#endif
            )
            {
                isVisible = WebGLFileUploadManager.Show(false);
                WebGLFileUploadManager.SetDescription("Select image files (.png|.jpg|.gif)");

            }
            else
            {
                isVisible = WebGLFileUploadManager.Show(true);
                WebGLFileUploadManager.SetDescription("Drop image files (.png|.jpg|.gif) here");
            }
            WebGLFileUploadManager.SetImageEncodeSetting(true);
            WebGLFileUploadManager.SetAllowedFileName("\\.(png|jpe?g|gif)$");
            WebGLFileUploadManager.SetImageShrinkingSize(1280, 960);
            WebGLFileUploadManager.onFileUploaded += OnFileUploaded;

            if (allowDrag)
                ShowHTMLOverlayButton();
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            WebGLFileUploadManager.onFileUploaded -= OnFileUploaded;
            WebGLFileUploadManager.Dispose();
        }

        /// <summary>
        /// Raises the file uploaded event.
        /// </summary>
        /// <param name="result">Uploaded file infos.</param>
        private void OnFileUploaded(UploadedFileInfo[] result)
        {
            if (result.Length == 0)
            {
                Debug.Log("File upload Error!");
            }
            else
            {
                Debug.Log("File upload success! (result.Length: " + result.Length + ")");
            }

            foreach (UploadedFileInfo file in result)
            {
                if (file.isSuccess)
                {
                    Debug.Log("file.filePath: " + file.filePath + " exists:" + File.Exists(file.filePath));

                    Texture2D texture = new Texture2D(2, 2);
                    byte[] byteArray = File.ReadAllBytes(file.filePath);
                    texture.LoadImage(byteArray);
                    gameObject.GetComponent<Renderer>().material.mainTexture = texture;

                    Debug.Log("File.ReadAllBytes:byte[].Length: " + byteArray.Length);

                    break;
                }
            }
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene("WebGLFileUploaderExample");
#else
            Application.LoadLevel("WebGLFileUploaderExample");
#endif
        }

        /// <summary>
        /// Raises the switch button overlay state button click event.
        /// </summary>
        public void ShowHTMLOverlayButton()
        {
            //WebGLFileUploadManager.Show(false, !WebGLFileUploadManager.IsOverlay);
            RecalculatePositionAndSize();
            isVisible = WebGLFileUploadManager.Show(false, true, x, y, w, h);
            WebGLFileUploadManager.UpdateButtonPosition(x, y, w, h);
        }

        private void RecalculatePositionAndSize()
        {
            var r = GetComponent<RectTransform>();

            //set anchor and pivot to left top, as this is where the HTML button is anchored
            r.anchorMin = new Vector2(0, 1);
            r.anchorMax = new Vector2(0, 1);
            r.pivot = new Vector2(0, 1);

            x = (int)transform.position.x;
            y = (int)transform.position.y;

            w = (int)r.sizeDelta.x;
            h = (int)r.sizeDelta.y;
        }

        public void SetX(string input)
        {
            x = int.Parse(input);
        }
        public void SetY(string input)
        {
            y = int.Parse(input);
        }
        public void SetW(string input)
        {
            w = int.Parse(input);
        }
        public void SetH(string input)
        {
            h = int.Parse(input);
        }

        public bool allowDrag;
        public InputField xField, yField, wField, hField;
        public void OnDrag(PointerEventData eventData)
        {
            if (allowDrag)
            {
                transform.position = Input.mousePosition;

                xField.text = transform.position.x.ToString();
                yField.text = transform.position.y.ToString();
                SetW(wField.text);
                SetH(hField.text);
                var r = GetComponent<RectTransform>();
                r.sizeDelta = new Vector2(w, h);
            }
            ShowHTMLOverlayButton();
        }

        private void Update()
        {
            if (isVisible && allowDrag)
            {
                RecalculatePositionAndSize();
                WebGLFileUploadManager.UpdateButtonPosition(x, y, w, h);
            }
        }
    }
}
