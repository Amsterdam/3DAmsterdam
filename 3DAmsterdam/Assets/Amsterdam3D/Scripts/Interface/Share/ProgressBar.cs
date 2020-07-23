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

        public void Percentage(float normalisedPercentage){
            progressBar.fillAmount = normalisedPercentage;
        }
    }
}