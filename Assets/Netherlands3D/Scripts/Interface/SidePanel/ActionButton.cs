using System;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.SidePanel
{
    public class ActionButton : MonoBehaviour
    {
        private Action<string> clickAction;

        [SerializeField]
        private Text buttonText;

        public void Select()
        {
            if (clickAction != null) clickAction.Invoke("");
        }

        public void SetAction(string title, Action<string> action)
        {
            gameObject.name = title;
            buttonText.text = title;
            clickAction = action;
        }
    }
}