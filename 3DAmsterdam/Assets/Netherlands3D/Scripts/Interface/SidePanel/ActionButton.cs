using Netherlands3D.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Netherlands3D.Interface.SidePanel
{
    public class ActionButton : MonoBehaviour
    {
        private Action<string> clickAction;

        [SerializeField]
        private TextMeshProUGUI buttonText;

        [SerializeField]
        private Button button;

        private void Start()
        {
            gameObject.AddComponent<AnalyticsClickTrigger>();
        }

        public void Select()
        {
            if (clickAction != null) clickAction.Invoke("");
        }

        public void ToggleButtonInteraction(bool interactable)
        {
            button.interactable = interactable;
        }

        public void SetAction(string title, Action<string> action)
        {
            gameObject.name = title;
            buttonText.text = title;
            clickAction = action;
        }
    }
}