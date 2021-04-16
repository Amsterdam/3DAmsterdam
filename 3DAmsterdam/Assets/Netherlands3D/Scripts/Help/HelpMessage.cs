using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Help
{
    public class HelpMessage : MonoBehaviour
    {
        public static HelpMessage instance;
        [SerializeField]
        private Text textMessageField;

        void Awake()
        {
            instance = this;
            gameObject.SetActive(false);
        }

        public void Show(string textMessage)
        {   
            instance.textMessageField.text = textMessage;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}