using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class ActionCheckbox : MonoBehaviour
    {
        private Action<bool> checkAction;

        [SerializeField]
        private Text checkboxText;

        public void Select(bool checkedBox)
        {
            if (checkAction != null) checkAction.Invoke(checkedBox);
        }

        public void SetAction(string title, Action<bool> action)
        {
            checkboxText.text = title;
            checkAction = action;
        }
    }
}