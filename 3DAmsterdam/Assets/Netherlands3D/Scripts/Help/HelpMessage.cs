using Netherlands3D.Events;
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
        private float messageTime = 10.0f;

        [SerializeField]
        private float minimumMessageTime = 2.0f;

        [SerializeField]
        private float fadeOutTime = 1.0f;

        private bool allowHideViaInteraction = false;
        private bool oneFrameHasPassed = false;

        [Header("Listen to")]
        [SerializeField]
        private StringEvent onShowHelpMessage;

        void Awake()
        {
            Instance = this;
            gameObject.SetActive(false);

            onShowHelpMessage.started.AddListener(Show);
        }

        public void Show(string textMessage)
        {
            allowHideViaInteraction = false;
            oneFrameHasPassed = false; 

            Instance.textMessageField.text = textMessage;

            gameObject.SetActive(true);

            StopAllCoroutines(); //Stop any running fade counters
            StartCoroutine(StartWaitForAllowHide());
            StartCoroutine(WaitForFadeOut());
        }

        private IEnumerator StartWaitForAllowHide()
        {
            yield return new WaitForEndOfFrame();
            oneFrameHasPassed = true;
            yield return new WaitForSeconds(minimumMessageTime);
            allowHideViaInteraction = true;
        }

        private IEnumerator WaitForFadeOut()
        {
            yield return new WaitForSeconds(messageTime);
            yield return FadeOut();
		}

        private IEnumerator FadeOut()
        {
            textMessageField.CrossFadeAlpha(0, fadeOutTime, false);
            yield return new WaitForSeconds(fadeOutTime);
            gameObject.SetActive(false);
        }

        public void Hide(bool instantHide = false)
        {
            if (gameObject.activeSelf && oneFrameHasPassed && (allowHideViaInteraction || instantHide))
            {
                StopAllCoroutines();
                if (instantHide)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    StartCoroutine(FadeOut());
                }
            }
        }
    }
}