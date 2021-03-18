using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.SidePanel
{
    public class ActionCheckbox : MonoBehaviour
    {
        [SerializeField]
        private Toggle checkboxToggle;

        private Action<bool> checkAction;

        [SerializeField]
        private Text checkboxText;

        public void Select(bool checkedBox)
        {
            if (checkAction != null) checkAction.Invoke(checkedBox);
        }

        public void SetAction(string title, bool checkedBox, Action<bool> action)
        {
            checkboxToggle.isOn = checkedBox;
            checkboxText.text = title;
            checkAction = action;
        }
    }
}