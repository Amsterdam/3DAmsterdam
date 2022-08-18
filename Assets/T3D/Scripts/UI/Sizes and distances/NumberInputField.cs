using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Netherlands3D.ObjectInteraction;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    [RequireComponent(typeof(Distance))]
    public class NumberInputField : MonoBehaviour
    {
        public Distance Distance { get; private set; }

        public delegate void DistanceInputOverrideEventHandler(NumberInputField source, float distance);
        public event DistanceInputOverrideEventHandler DistanceInputOverride;
        public delegate void DeleteButtonPressedEventHandler(NumberInputField source);
        public event DeleteButtonPressedEventHandler DeleteButtonPressed;

        [SerializeField]
        private Button deleteButton;

        public bool IsSelected
        {
            get
            {
                if (EventSystem.current.currentSelectedGameObject)
                {
                    return EventSystem.current.currentSelectedGameObject.GetComponentInParent<NumberInputField>() == this;
                }
                return false;
            }
        }

        private void Awake()
        {
            Distance = GetComponent<Distance>();
        }

        //called by input field OnChange event in inspector
        public void ProcessUserInput(string manualInput)
        {
            if (TryParseUserString(manualInput, out float distance, out string units))
            {
                //print("ïnput: " + distance + units);
                //switch (units.ToLower())
                //{
                //    case "dm":
                //        distance *= 10;
                //        break;
                //    case "cm":
                //        distance *= 100;
                //        break;
                //    case "mm":
                //        distance *= 1000;
                //        break;
                //}
                print("input: " + distance + "m");
                distance /= 100;
                DistanceInputOverride?.Invoke(this, distance);
            }
        }

        private bool TryParseUserString(string inputString, out float distance, out string units)
        {
            units = "cm";
            return float.TryParse(inputString, out distance);
        }

        //called by the inspector
        public void DeleteLabel()
        {
            DeleteButtonPressed?.Invoke(this);
        }

        public void EnableDeleteButton(bool enable)
        {
            deleteButton.gameObject.SetActive(enable);
        }

        public void SetInteractable(bool interactable)
        {
            Distance.SetInteractable(interactable);
        }

        //private bool TryParseUserString(string inputString, out float distance, out string units)
        //{
        //    Regex distRex = new Regex("(?<dist>\\d+)\\s*(?<unit>|m|dm|cm|mm)", RegexOptions.IgnoreCase);

        //    Match m = distRex.Match(inputString);
        //    print(inputString);

        //    if (m.Success)
        //    {
        //        units = m.Groups["unit"].Value;
        //        distance = float.Parse(m.Groups["dist"].Value);

        //        return true;
        //    }

        //    distance = 0;
        //    units = "m";
        //    return false;
        //}
    }
}
