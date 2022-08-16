using Netherlands3D.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Interface
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IScrollHandler
    {
        [SerializeField]
        [TextArea(3, 10)]
        private string tooltipText = "";
        public string TooltipText { get => tooltipText; set => tooltipText = value; }

        private RectTransform rectTransform;

        private void Awake(){
            rectTransform = this.GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ServiceLocator.GetService<TooltipDialog>().Hide();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Input.GetMouseButton(0)) return; //Dont show new tooltips when we are still holding our mouse button
            ServiceLocator.GetService<TooltipDialog>().ShowMessage(tooltipText, rectTransform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ServiceLocator.GetService<TooltipDialog>().Hide();
        }

        public void OnScroll(PointerEventData eventData)
        {
            ServiceLocator.GetService<TooltipDialog>().Hide();
        }
    }
}