using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Netherlands3D.ObjectInteraction;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    [RequireComponent(typeof(Distance))]
    public class NumberInputField : MonoBehaviour
    {
        public Distance Distance { get; private set; }
        private InputField inputField;

        public delegate void DistanceInputOverrideEventHandler(NumberInputField source, float distance);
        public event DistanceInputOverrideEventHandler DistanceInputOverride;

        private void Awake()
        {
            Distance = GetComponent<Distance>();
        }

        //called by input field OnChange event in inspector
        public void ProcessUserInput(string manualInput)
        {
            if (TryParseUserString(manualInput, out float distance, out string units))
            {
                switch (units.ToLower())
                {
                    case "dm":
                        distance *= 10;
                        break;
                    case "cm":
                        distance *= 100;
                        break;
                    case "mm":
                        distance *= 1000;
                        break;
                }
                print("ïnput: " + distance + "m");
                DistanceInputOverride?.Invoke(this, distance);
            }
        }

        private bool TryParseUserString(string inputString, out float distance, out string units)
        {
            Regex distRex = new Regex("(?<dist>\\d+)\\s*(?<unit>|m|dm|cm|mm)", RegexOptions.IgnoreCase);

            Match m = distRex.Match(inputString);
            print(inputString);

            if (m.Success)
            {
                units = m.Groups["unit"].Value;
                distance = float.Parse(m.Groups["dist"].Value);

                return true;
            }

            distance = 0;
            units = "m";
            return false;
        }
    }
}
