using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    public class Warning : MonoBehaviour
    {
        [SerializeField]
        private Text bodyText;

        /// <summary>
        /// Leave empty to show the default message
        /// </summary>
        /// <param name="message">Custom warning message</param>
        public void SetMessage(string message = "")
        {
            if (message != "")
            {
                bodyText.text = message;
            }
        }

        /// <summary>
        /// Closing the panel using own method to destroy itself
        /// </summary>
        public void Close()
        {
            Destroy(gameObject);
        }
    }
}