using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SharedURL : MonoBehaviour
{
    [SerializeField]
    private InputField inputField;

    [SerializeField]
    private RectTransform copiedText;


    public void CopyCurrentURLToClipboard()
    {
        #if !UNITY_EDITOR && UNITY_WEBGL
            WebGLCopyAndPasteAPI.PassCopyToBrowser(inputField.text);
        #else
            GUIUtility.systemCopyBuffer = inputField.text;
        #endif
        copiedText.gameObject.SetActive(false);
        copiedText.gameObject.SetActive(true);
    }

    public void ShowURL(string url)
    {
        inputField.text = url;
    }
}
