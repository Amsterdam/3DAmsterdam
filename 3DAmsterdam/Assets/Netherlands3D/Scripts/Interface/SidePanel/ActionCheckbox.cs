using Netherlands3D.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Netherlands3D.Interface.SidePanel
{
    public class ActionCheckbox : MonoBehaviour
    {
        [SerializeField]
        private Toggle checkboxToggle;

        private Action<bool> checkAction;

        [SerializeField]
        private TextMeshProUGUI checkboxText;

        private void Start()
        {
            gameObject.AddComponent<AnalyticsClickTrigger>();
        }

        public void Select(bool checkedBox)
        {
            if (checkAction != null) checkAction.Invoke(checkedBox);
        }

        public void SetAction(string title, bool checkedBox, Action<bool> action)
        {
            gameObject.name = title;
            checkboxToggle.isOn = checkedBox;
            checkboxText.text = title;
            checkAction = action;
        }
    }
}