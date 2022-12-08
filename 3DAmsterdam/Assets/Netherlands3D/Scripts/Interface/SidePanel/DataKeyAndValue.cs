using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Netherlands3D.Interface.SidePanel
{
    public class DataKeyAndValue : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI keyText;

        [SerializeField]
        private TextMeshProUGUI valueText;

		public TextMeshProUGUI ValueText { get => valueText; }

		public void SetTexts(string key, string value)
        {
            keyText.text = key;
            ValueText.text = value;
        }
    }
}