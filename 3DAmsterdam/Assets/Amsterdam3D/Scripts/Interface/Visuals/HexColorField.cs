using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class HexColorField : ColorSelector
{
    [SerializeField]
    private InputField inputTextField;

    public void ChangedHexInput(string currentInputText)
    {
        var hexText = currentInputText;
        hexText = hexText.Replace("#", "").Trim();
        hexText = "#" + hexText;
        
        inputTextField.text = hexText.ToUpper();

        if (inputTextField.text.Length == 1)
            inputTextField.caretPosition = 1;

        ChangeHexToColor(inputTextField.text);
    }
    public void ResetIfEmpty(string inputString)
    {
        if(inputTextField.text.Length < inputTextField.characterLimit)
        {
            inputTextField.text = inputTextField.text.PadRight(inputTextField.characterLimit, '0');
        }
        ChangeHexToColor(inputTextField.text);
    }

    private void ChangeHexToColor(string hexColor){
        if (!inputTextField.isFocused) return;

        if (hexColor.Length == inputTextField.characterLimit && ColorUtility.TryParseHtmlString(hexColor, out Color convertedColor)){
            selectedNewColor.Invoke(convertedColor, this);
            return;
        }       
    }

    public override void ChangeColorInput(Color inputColor)
    {
        if(!inputTextField.isFocused)
          inputTextField.text = "#" + ColorUtility.ToHtmlStringRGB(inputColor);
    }
}
