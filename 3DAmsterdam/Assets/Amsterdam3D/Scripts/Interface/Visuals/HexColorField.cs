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
        if (hexText.Substring(0,1) != "#"){
            hexText = "#" + hexText;
            inputTextField.caretPosition = 1;
        }

        inputTextField.text = hexText.ToUpper();

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
