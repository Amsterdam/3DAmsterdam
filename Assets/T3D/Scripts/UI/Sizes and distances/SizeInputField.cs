using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using System.Reflection;

namespace Netherlands3D.T3D.Uitbouw
{
    public enum SizeType
    {
        Width,
        Height,
        Depth,
        Area
    }

    public class SizeInputFieldSaveData : SaveDataContainer
    {
        public float Value;

        public SizeInputFieldSaveData(SizeType type) : base(type.ToString())
        {
        }
    }

    public class SizeInputField : MonoBehaviour
    {
        private InputField inputField;
        [SerializeField]
        private SizeType size;

        private ShapeableUitbouw shapeableUitbouw;
        private SizeInputFieldSaveData saveData;

        public bool IsSelected => EventSystem.current.currentSelectedGameObject == gameObject;

        private void Awake()
        {
            saveData = new SizeInputFieldSaveData(size);
            inputField = GetComponent<InputField>();

            //var Value = new SaveableFloat(GetType().ToString()+ size.ToString()) ;
        }

        private void Start()
        {
            // inputfields are disabled in upload mode, so put this in an if statement
            if (inputField)
            {
                inputField.interactable = ServiceLocator.GetService<T3DInit>().IsEditMode;
            }

            //value = new SaveableFloat(valueKey);

            shapeableUitbouw = RestrictionChecker.ActiveUitbouw as ShapeableUitbouw;

            //in some specific cases the value is not present in the loaded data, so if that is the case (value == 0) don't load the data
            if (SessionSaver.LoadPreviousSession && shapeableUitbouw && saveData.Value > 0)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            float delta;
            switch (size)
            {
                case SizeType.Width:
                    delta = (saveData.Value / 100) - RestrictionChecker.ActiveUitbouw.Width;
                    shapeableUitbouw.MoveWall(WallSide.Left, delta / 2);
                    shapeableUitbouw.MoveWall(WallSide.Right, delta / 2);
                    break;
                case SizeType.Height:
                    delta = (saveData.Value / 100) - RestrictionChecker.ActiveUitbouw.Height;
                    shapeableUitbouw.MoveWall(WallSide.Top, delta);
                    break;
                case SizeType.Depth:
                    delta = (saveData.Value / 100) - RestrictionChecker.ActiveUitbouw.Depth;
                    shapeableUitbouw.MoveWall(WallSide.Front, delta);
                    break;
            }
        }

        private void Update()
        {
            if (RestrictionChecker.ActiveUitbouw != null) // Only set text if uitbouw exists
            {
                if (!inputField || !inputField.isFocused) //Text can be set because manual override is not possible, and when input field is not focused, since the user is not inputting in this input field
                {
                    SetText();
                }
            }
            if (shapeableUitbouw)
                SetWallColors(size, IsSelected);
        }

        private void SetWallColors(SizeType size, bool highlight)
        {
            switch (size)
            {
                case SizeType.Width:
                    shapeableUitbouw.SetWallHighlightActive(WallSide.Left, highlight);
                    shapeableUitbouw.SetWallHighlightActive(WallSide.Right, highlight);
                    break;
                case SizeType.Height:
                    shapeableUitbouw.SetWallHighlightActive(WallSide.Top, highlight);
                    shapeableUitbouw.SetWallHighlightActive(WallSide.Bottom, highlight);
                    break;
                case SizeType.Depth:
                    shapeableUitbouw.SetWallHighlightActive(WallSide.Front, highlight);
                    shapeableUitbouw.SetWallHighlightActive(WallSide.Back, highlight);
                    break;
            }
        }

        void SetText()
        {
            switch (size)
            {
                case SizeType.Width:
                    saveData.Value = RestrictionChecker.ActiveUitbouw.Width * 100;
                    break;
                case SizeType.Height:
                    saveData.Value = RestrictionChecker.ActiveUitbouw.Height * 100;
                    break;
                case SizeType.Depth:
                    saveData.Value = RestrictionChecker.ActiveUitbouw.Depth * 100;
                    break;
                case SizeType.Area:
                    saveData.Value = RestrictionChecker.ActiveUitbouw.Area;
                    break;
            }

            if (inputField != null)
            {
                if (size == SizeType.Area)
                    inputField.text = saveData.Value.ToString("F2");
                else
                    inputField.text = saveData.Value.ToString("F0");
            }
            else
            {
                foreach (var textObject in GetComponentsInChildren<Text>())
                {
                    textObject.text = saveData.Value.ToString("F0");
                }
            }
        }

        // called by input field event in inspector
        public void ManualWidthOverride(string input)
        {
            if (IsValidInput(input, out float delta))
            {
                shapeableUitbouw.MoveWall(WallSide.Left, delta / 2 / 100);
                shapeableUitbouw.MoveWall(WallSide.Right, delta / 2 / 100);
            }
        }

        // called by input field event in inspector
        public void ManualHeightOverride(string input)
        {
            if (IsValidInput(input, out float delta))
            {
                shapeableUitbouw.MoveWall(WallSide.Top, delta / 100);
            }
        }

        // called by input field event in inspector
        public void ManualDepthOverride(string input)
        {
            if (IsValidInput(input, out float delta))
            {
                shapeableUitbouw.MoveWall(WallSide.Front, delta / 100);
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
                delta = amount - saveData.Value;
                return true;
            }
            return false;
        }
    }
}
