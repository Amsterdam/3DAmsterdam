using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SharedURL : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private RectTransform copiedText;

    private WebGLCopyAndPaste copyPasteWrapper;

    public void CopiedText()
    {
        copyPasteWrapper = FindObjectOfType<WebGLCopyAndPaste>();
        if(!copyPasteWrapper) return;

        //Highlight our text for visual feedback the text was selected
        inputField.Select();

        GUIUtility.systemCopyBuffer = inputField.text;
#if !UNITY_EDITOR && UNITY_WEBGL
        copyPasteWrapper.GetClipboard("c");
#endif
    
        //Trigger animator start
        copiedText.gameObject.SetActive(false);
        copiedText.gameObject.SetActive(true);
    }

    public void ShowURL(string url)
    {
        inputField.text = url;
    }
}
