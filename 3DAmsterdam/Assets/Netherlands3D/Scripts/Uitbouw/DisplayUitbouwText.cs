using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw
{
    public class DisplayUitbouwText : MonoBehaviour
    {
        [SerializeField]
        private Text widthText;
        [SerializeField]
        private Text heightText;
        [SerializeField]
        private Text depthText;
        [SerializeField]
        private Text areaText;

        private Uitbouw uitbouw;

        private void Start()
        {
            uitbouw = HandleMetaDataUpdates.Uitbouw;
            SetUitbouwText(uitbouw.Width, uitbouw.Height, uitbouw.Depth, uitbouw.Area);
        }

        private void SetUitbouwText(float width, float height, float depth, float area)
        {
            widthText.text = $"{width.ToString("F2")} m";
            heightText.text = $"{height.ToString("F2")} m";
            depthText.text = $"{depth.ToString("F2")} m";
            areaText.text = $"{area.ToString("F2")} m²";
        }
    }
}
