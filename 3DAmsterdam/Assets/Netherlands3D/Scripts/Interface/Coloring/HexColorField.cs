using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Netherlands3D.Interface.Coloring
{
    public class HexColorField : ColorSelector
    {
        [SerializeField]
        private TMP_InputField inputTextField;

        public void ChangedHexInput(string currentInputText)
        {
            var hexText = currentInputText;

            //just reset to white if we clear the field
            if (hexText == "") hexText = "#FFFFFF";

            //always make sure with have a hash prefix
            hexText = hexText.Replace("#", "").Trim();
            hexText = "#" + hexText;

            inputTextField.text = hexText.ToUpper();

            if (inputTextField.text.Length == 1)
                inputTextField.caretPosition = 1;

            ChangeHexToColor(inputTextField.text);
        }

        private void ChangeHexToColor(string hexColor)
        {
            if (!inputTextField.isFocused) return;

            if (ColorUtility.TryParseHtmlString(hexColor, out Color convertedColor))
            {
                selectedNewColor.Invoke(convertedColor, this);
                return;
            }
        }

        public override void ChangeColorInput(Color inputColor)
        {
            if (!inputTextField.isFocused)
                inputTextField.text = "#" + ColorUtility.ToHtmlStringRGB(inputColor);
        }
    }
}