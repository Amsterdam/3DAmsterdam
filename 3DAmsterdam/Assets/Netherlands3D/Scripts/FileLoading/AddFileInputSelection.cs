using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.JavascriptConnection
{
    public class AddFileInputSelection : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void AddFileInput(string inputName, string fileExtentions);

        [Tooltip("HTML DOM ID")]
        [SerializeField]
        private string fileInputName = "fileInput";

        [Tooltip("Allowed file input selections")]
        [SerializeField]
        private string fileExtentions = ".csv";

        private Button button;

        void Start()
        {
            button = GetComponent<Button>();
            name = fileInputName;
#if !UNITY_EDITOR && UNITY_WEBGL
            AddFileInput(fileInputName, fileExtentions);
            gameObject.AddComponent<DrawHTMLOverCanvas>().AlignObjectID(fileInputName);
#endif
        }

        /// <summary>
        /// If the click is registered from the HTML overlay side, this method triggers the onclick events on the button
        /// </summary>
        public void ClickNativeButton()
        {
            if(button){ 
                Debug.Log("Invoked native Unity button click event on " + this.gameObject.name);
                button.onClick.Invoke();
            }
        }
    }
}
