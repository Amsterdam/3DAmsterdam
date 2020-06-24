using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Amsterdam3D.Interface
{
    public class TooltipDialog : MonoBehaviour
    {
        private Animator animator;
        private Text tooltiptext;
        private RectTransform rectTransform;
        private ContentSizeFitter contentSizeFitter;

        #region Singleton
        public static TooltipDialog instance;
        public static TooltipDialog Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new System.Exception("No tooltip object instance found. Make it is active in your scene.");
                }

                return instance;
            }
        }
        #endregion

        private void Start()
        {
            instance = this;

            contentSizeFitter = GetComponent<ContentSizeFitter>();
            tooltiptext = GetComponentInChildren<Text>();
            rectTransform = GetComponent<RectTransform>();
            Hide();
        }

        private void Update()
        {
            FollowPointer();
        }

        private void FollowPointer()
        {
            rectTransform.position = Input.mousePosition;
            rectTransform.position = Input.mousePosition;
        }

        public void ShowMessage(string message = "Tooltip"){
            //Restart our animator
            gameObject.SetActive(false);
            gameObject.SetActive(true);

            tooltiptext.text = message;
            StartCoroutine(FitContent());
        }

        private IEnumerator FitContent(){
            yield return new WaitForEndOfFrame();
            contentSizeFitter.enabled = false;
            contentSizeFitter.enabled = true;
        }

        public void Hide(){
            gameObject.SetActive(false);
        }
    }
}
