using Netherlands3D.BAG;
using Netherlands3D.Cameras;
using Netherlands3D.Core.Colors;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.SidePanel
{ 
    public class SideTabPanel : MonoBehaviour
    {
        public static SideTabPanel Instance = null;
        private TransformPanel transformPanel;

        [SerializeField]
        private RectTransform movePanelRectTransform;

        [SerializeField]
        private TabItem startingActiveTab;


        [Header("Animation")]
        [SerializeField]
        private float animationSpeed = 5.0f;
        [SerializeField]
        private float collapsedShift = 300;
        private Coroutine panelAnimation;
        public bool open = true;
        

		void Start()
		{
            //Find the transformpanel
            transformPanel = FindObjectOfType<TransformPanel>();

            if (Instance == null)
			{
				Instance = this;
			}

            //Open/closed at start
            if (startingActiveTab)
            {
                startingActiveTab.OpenTab(true);
            }
            else{
                movePanelRectTransform.anchoredPosition = Vector3.right * collapsedShift;
            }
        }


        /// <summary>
        /// Slide the panel open (if it is closed)
        /// </summary>
        /// <param name="title">Title to show on top of the panel</param>
        public void OpenPanel()
        {
            Debug.Log("hello open");

            open = true;

            if (panelAnimation != null) StopCoroutine(panelAnimation);

            if(this.gameObject.activeInHierarchy)
                panelAnimation = StartCoroutine(Animate());
		}

     
        public void ClosePanel()
        {
            Debug.Log("hello close");

            open = false;

            if (panelAnimation != null) StopCoroutine(panelAnimation);
            panelAnimation = StartCoroutine(Animate());

            //panel.gameObject.SetActive(true);
        }

        private IEnumerator Animate()
        {
            //Open
            while (open && movePanelRectTransform.anchoredPosition.x < 0)
            {
                movePanelRectTransform.anchoredPosition = Vector3.Lerp(movePanelRectTransform.anchoredPosition, Vector3.zero, animationSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            //Close
            while (!open && movePanelRectTransform.anchoredPosition.x > collapsedShift)
            {
                movePanelRectTransform.anchoredPosition = Vector3.Lerp(movePanelRectTransform.anchoredPosition, Vector3.right * collapsedShift, animationSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }


            yield return null;
        }
    }
}