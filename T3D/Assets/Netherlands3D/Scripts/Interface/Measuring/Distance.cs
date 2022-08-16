using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    public class Distance : WorldPointFollower
    {
        [SerializeField]
        private Text distanceText;
        [SerializeField]
        private InputField inputField;

        public void DrawDistance(float distance, string suffix)
        {
            string s = "~" + distance.ToString("F2") + suffix;
            if (distanceText)
                distanceText.text = s;
            if (inputField)
                inputField.text = s;
        }

        public void DrawDistance(float distance, string suffix, int decimals)
        {
            string s = distance.ToString("F" + decimals) + suffix;
            if (distanceText)
                distanceText.text = s;
            if (inputField)
                inputField.text = s;
        }

        public void ResetInput()
        {
            //var input = GetComponentInChildren<InputField>();
            //if (input) input.text = distanceText.text;
        }

        public void SetInteractable(bool interactable)
        {
            inputField.interactable = interactable;
        }
    }
}
