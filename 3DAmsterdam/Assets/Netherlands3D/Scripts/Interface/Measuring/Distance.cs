using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    public class Distance : WorldPointFollower
    {
        [SerializeField]
        private Text distanceText;

        public void DrawDistance(float distance, string suffix)
        {
            distanceText.text = "~"+ distance.ToString("F2") + suffix;
        }

		public void ResetInput()
		{
            var input = GetComponentInChildren<InputField>();
            if(input) input.text = distanceText.text;
        }
	}
}
