using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    public class Distance : WorldPointFollower
    {
        [SerializeField]
        private TextMeshProUGUI distanceText;

        public void DrawDistance(float distance, string suffix)
        {
            distanceText.text = "~"+ distance.ToString("F2") + suffix;
        }

		public void ResetInput()
		{
            var input = GetComponentInChildren<TMP_InputField>();
            if(input) input.text = distanceText.text;
        }
	}
}
