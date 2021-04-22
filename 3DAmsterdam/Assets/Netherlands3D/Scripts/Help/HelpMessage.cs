using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Help
{
    public class HelpMessage : MonoBehaviour
    {
        public static HelpMessage Instance;
        [SerializeField]
        private Text textMessageField;

        [SerializeField]
        private float messageTime = 5.0f;

        [SerializeField]
        private float fadeOutTime = 1.0f;

        void Awake()
        {
            Instance = this;
            gameObject.SetActive(false);
        }

        public void Show(string textMessage)
        {   
            Instance.textMessageField.text = textMessage;

            gameObject.SetActive(true);

            StopAllCoroutines(); //Stop any running fade counters
            StartCoroutine(WaitForFadeOut());
        }

        private IEnumerator WaitForFadeOut()
        {
            yield return new WaitForSeconds(messageTime);
            textMessageField.CrossFadeAlpha(0, fadeOutTime, false);
            yield return new WaitForSeconds(fadeOutTime);
            Hide();
		}

        public void Hide()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
    }
}