using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Netherlands3D.Help
{
    public class HelpMessage : MonoBehaviour
    {
        public static HelpMessage Instance;
        [SerializeField] private TextMeshProUGUI textMessageField;
        [SerializeField] private TextMeshProUGUI textMessageFieldFront;

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

            onShowHelpMessage.AddListenerStarted(Show);
        }

        public static void Show(string textMessage)
        {
            if (Instance == null) return;

            Instance.allowHideViaInteraction = false;
            Instance.oneFrameHasPassed = false; 
            Instance.textMessageField.text = textMessage;
            Instance.textMessageFieldFront.text = textMessage;

            Instance.gameObject.SetActive(true);
            Instance.StopAllCoroutines(); //Stop any running fade counters
            Instance.StartCoroutine(Instance.StartWaitForAllowHide());
            Instance.StartCoroutine(Instance.WaitForFadeOut());
        }

        public IEnumerator StartWaitForAllowHide()
        {
            yield return new WaitForEndOfFrame();
            oneFrameHasPassed = true;
            yield return new WaitForSeconds(minimumMessageTime);
            allowHideViaInteraction = true;
        }

        public IEnumerator WaitForFadeOut()
        {
            yield return new WaitForSeconds(messageTime);
            yield return FadeOut();
		}

        public IEnumerator FadeOut()
        {
            textMessageField.CrossFadeAlpha(0, fadeOutTime, false);
            textMessageFieldFront.CrossFadeAlpha(0, fadeOutTime, false);
            yield return new WaitForSeconds(fadeOutTime);
            gameObject.SetActive(false);
        }

        public static void Hide(bool instantHide = false)
        {
            if (Instance && Instance.gameObject.activeSelf && Instance.oneFrameHasPassed && (Instance.allowHideViaInteraction || instantHide))
            {
                Instance.StopAllCoroutines();
                if (instantHide)
                {
                    Instance.gameObject.SetActive(false);
                }
                else
                {
                    Instance.StartCoroutine(Instance.FadeOut());
                }
            }
        }
    }
}