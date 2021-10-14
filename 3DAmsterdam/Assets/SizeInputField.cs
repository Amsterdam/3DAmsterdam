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
        private InputField inputField;
        [SerializeField]
        private SizeType size;
        private float value = 0f;
        //[SerializeField]
        //private string unitText = "m";

        private void Awake()
        {
            inputField = GetComponent<InputField>();
        }

        private void Update()
        {
            if (!inputField.isFocused)
                SetText();
        }

        void SetText()
        {
            switch (size)
            {
                case SizeType.Width:
                    value = RestrictionChecker.ActiveUitbouw.Width;
                    break;
                case SizeType.Height:
                    value = RestrictionChecker.ActiveUitbouw.Height;
                    break;
                case SizeType.Depth:
                    value = RestrictionChecker.ActiveUitbouw.Depth;
                    break;
                case SizeType.Area:
                    value = RestrictionChecker.ActiveUitbouw.Area;
                    break;
            }

            inputField.text = value.ToString("F2");
        }

        // called by input field event in inspector
        public void ManualWidthOverride(string input)
        {
            if (IsValidInput(input, out float delta))
            {
                RestrictionChecker.ActiveUitbouw.MoveWall(WallSide.Left, delta/2);
                RestrictionChecker.ActiveUitbouw.MoveWall(WallSide.Right, delta/2);
            }
        }

        // called by input field event in inspector
        public void ManualHeightOverride(string input)
        {
            if (IsValidInput(input, out float delta))
            {
                RestrictionChecker.ActiveUitbouw.MoveWall(WallSide.Top, delta);
            }
        }

        // called by input field event in inspector
        public void ManualDepthOverride(string input)
        {
            if (IsValidInput(input, out float delta))
            {
                RestrictionChecker.ActiveUitbouw.MoveWall(WallSide.Front, delta);
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
                delta = amount - value;
                return true;
            }
            return false;
        }
    }
}
