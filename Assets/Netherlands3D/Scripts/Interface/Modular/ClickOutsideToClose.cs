using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Interface.Modular
{
    public class ClickOutsideToClose : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Additional objects that can be clicked without closing this UI item.")]
        private GameObject[] additionalGameObjects;

        private int numberOfClicksToIgnore = 0;

        void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                this.gameObject.SetActive(numberOfClicksToIgnore > 0 || ClickingSelfOrChild());

                if (numberOfClicksToIgnore > 0)
                    numberOfClicksToIgnore--;
            }
        }

        public void IgnoreClicks(int ignore = 0)
        {
            numberOfClicksToIgnore = ignore;
        }

        private bool ClickingSelfOrChild()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
                return false;

            //Check self and children
            if (EventSystem.current.currentSelectedGameObject)
                Debug.Log("Clicking on " + EventSystem.current.currentSelectedGameObject.name, EventSystem.current.currentSelectedGameObject);

            RectTransform[] rectTransforms = GetComponentsInChildren<RectTransform>();
            foreach (RectTransform rectTransform in rectTransforms)
            {
                if (EventSystem.current.currentSelectedGameObject == rectTransform.gameObject)
                {
                    return true;
                };
            }

            //Check the additional list we set manualy
            foreach (GameObject otherGameObject in additionalGameObjects)
            {
                if (EventSystem.current.currentSelectedGameObject == otherGameObject)
                {
                    return true;
                };
            }

            return false;
        }
    }
}