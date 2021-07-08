using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.SidePanel
{
    public class DataKeyAndValue : MonoBehaviour
    {
        [SerializeField]
        private Text keyText;

        [SerializeField]
        private Text valueText;

		public Text ValueText { get => valueText; }

		public void SetTexts(string key, string value)
        {
            keyText.text = key;
            ValueText.text = value;
        }
    }
}