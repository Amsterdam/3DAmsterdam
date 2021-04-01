﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Netherlands3D.Interface
{
    public class TooltipDialog : MonoBehaviour
    {
        private Animator animator;
        private Text tooltiptext;
        private RectTransform rectTransform;
        private ContentSizeFitter contentSizeFitter;

        private bool pivotRight = false;

        #region Singleton
        private static TooltipDialog instance;
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

        private void Awake()
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
            rectTransform.position = Mouse.current.position.ReadValue();
            
            //Swap pivot based on place in screen (to try to stay in the screen horizontally)
            if(!pivotRight && rectTransform.position.x > Screen.width * 0.9f)
            {
                pivotRight = true;
                rectTransform.pivot = Vector2.right;
            }
            else if(pivotRight && rectTransform.position.x <= Screen.width * 0.9f)
            {
                pivotRight = false;
                rectTransform.pivot = Vector2.zero;
            }
        }

        public void ShowMessage(string message = "Tooltip"){
            //Restart our animator
            gameObject.transform.SetAsLastSibling(); //Make sure we are in front of all the UI

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
