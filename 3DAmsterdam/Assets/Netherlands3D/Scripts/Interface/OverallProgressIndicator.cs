using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D
{
    public class OverallProgressIndicator : MonoBehaviour
    {
        private static Image fillImage;
        private float fadeOutSpeed = 1.0f;

        void Awake()
        {
            fillImage = GetComponent<Image>();
            Show(false);
        }

        /// <summary>
        /// Shows a general progress spinner on screen
        /// </summary>
        /// <param name="showIndicator">Show indicator on screen</param>
        public static void Show(bool showIndicator)
        {
            fillImage.gameObject.SetActive(showIndicator);
        }
    }
}