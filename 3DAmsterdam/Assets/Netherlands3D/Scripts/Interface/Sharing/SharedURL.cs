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

    [SerializeField]
    private WebGLCopyAndPaste copyPasteWrapper;

    public void CopiedText()
    {
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
