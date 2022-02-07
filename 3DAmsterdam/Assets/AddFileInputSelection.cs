using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

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


        void Awake()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            AddFileInput(fileInputName, fileExtentions);
            gameObject.AddComponent<DrawHTMLOverCanvas>().AlignObjectID(fileInputName);
#endif
        }
    }
}
