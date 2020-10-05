using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField]
        private Image progressBar;
        [SerializeField]
        private Text progressStepText;

        public void SetMessage(string message)
        {
            progressStepText.text = message;
        }

        /// <summary>
        /// Sets the amount the bar is filled
        /// </summary>
        /// <param name="normalisedPercentage">Value from 0.0 to 1.0</param>
        public void Percentage(float normalisedPercentage){
            progressBar.fillAmount = normalisedPercentage;
        }
    }
}