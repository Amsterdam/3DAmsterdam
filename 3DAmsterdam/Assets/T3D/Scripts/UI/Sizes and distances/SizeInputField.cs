using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw
{
    public enum SizeType
    {
        Width,
        Height,
        Depth,
        Area
    }

    public class SizeInputField : MonoBehaviour
    {
        public bool UpdateOnce;
        private bool hasUpdatedText;


        private InputField inputField;
        [SerializeField]
        private SizeType size;
        private SaveableFloat value;
        private string valueKey;
        //[SerializeField]
        //private string unitText = "m";

        private void Awake()
        {
            inputField = GetComponent<InputField>();
            valueKey = GetType().Namespace + GetType().ToString() + "." + size.ToString();
        }

        private void Start()
        {
            value = new SaveableFloat(valueKey, SessionSaver.LoadPreviousSession);
            LoadData();
        }

        private void LoadData()
        {
            float delta = 0;
            switch (size)
            {
                case SizeType.Width:
                    delta = (value.Value / 100) - RestrictionChecker.ActiveUitbouw.Width;
                    RestrictionChecker.ActiveUitbouw.MoveWall(WallSide.Left, delta / 2);
                    RestrictionChecker.ActiveUitbouw.MoveWall(WallSide.Right, delta / 2);
                    break;
                case SizeType.Height:
                    delta = (value.Value / 100) - RestrictionChecker.ActiveUitbouw.Height;
                    RestrictionChecker.ActiveUitbouw.MoveWall(WallSide.Top, delta);
                    break;
                case SizeType.Depth:
                    delta = (value.Value / 100) - RestrictionChecker.ActiveUitbouw.Depth;
                    RestrictionChecker.ActiveUitbouw.MoveWall(WallSide.Front, delta);
                    break;
            }
        }

        private void Update()
        {
            if (hasUpdatedText == false && UpdateOnce && RestrictionChecker.ActiveUitbouw != null)
            {
                SetText();
                hasUpdatedText = true;
            }

            if (!UpdateOnce && !inputField.isFocused)
                SetText();
        }

        void SetText()
        {
            switch (size)
            {
                case SizeType.Width:
                    value.SetValue(RestrictionChecker.ActiveUitbouw.Width * 100);
                    break;
                case SizeType.Height:
                    value.SetValue(RestrictionChecker.ActiveUitbouw.Height * 100);
                    break;
                case SizeType.Depth:
                    value.SetValue(RestrictionChecker.ActiveUitbouw.Depth * 100);
                    break;
                case SizeType.Area:
                    value.SetValue(RestrictionChecker.ActiveUitbouw.Area);
                    break;
            }

            if (inputField != null)
            {
                if (size == SizeType.Area)
                    inputField.text = value.Value.ToString("F2");
                else
                    inputField.text = value.Value.ToString("F0");
            }
            else
            {
                foreach (var textObject in GetComponentsInChildren<Text>())
                {
                    textObject.text = value.Value.ToString("F0");
                }
            }
        }

        // called by input field event in inspector
        public void ManualWidthOverride(string input)
        {
            if (IsValidInput(input, out float delta))
            {
                RestrictionChecker.ActiveUitbouw.MoveWall(WallSide.Left, delta / 2 / 100);
                RestrictionChecker.ActiveUitbouw.MoveWall(WallSide.Right, delta / 2 / 100);
            }
        }

        // called by input field event in inspector
        public void ManualHeightOverride(string input)
        {
            if (IsValidInput(input, out float delta))
            {
                RestrictionChecker.ActiveUitbouw.MoveWall(WallSide.Top, delta / 100);
            }
        }

        // called by input field event in inspector
        public void ManualDepthOverride(string input)
        {
            if (IsValidInput(input, out float delta))
            {
                RestrictionChecker.ActiveUitbouw.MoveWall(WallSide.Front, delta / 100);
            }
        }

        private bool IsValidInput(string input, out float delta)
        {
            delta = 0;
            if (float.TryParse(input, out float amount))
            {
                if (amount <= 0)
                {
                    print("enter a positive number");
                    return false;
                }
                delta = amount - value.Value;
                return true;
            }
            return false;
        }
    }
}
