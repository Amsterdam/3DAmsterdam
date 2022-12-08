using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Netherlands3D.Interface
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField]
        private Image progressBar;
        [SerializeField]
        private TextMeshProUGUI progressStepText;

        /// <summary>
        /// Sets the accompanying message to explain the progress bar status
        /// </summary>
        /// <param name="message">For example "15%" or "Loading data.."</param>
        public void SetMessage(string message)
        {
            progressStepText.text = message;
        }

        /// <summary>
        /// Sets the fill amount of the progressbar
        /// </summary>
        /// <param name="normalisedPercentage">Value from 0.0 to 1.0</param>
        public void Percentage(float normalisedPercentage){
            progressBar.fillAmount = normalisedPercentage;
        }
    }
}