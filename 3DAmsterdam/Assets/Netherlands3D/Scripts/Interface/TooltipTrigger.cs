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
            TooltipDialog.Instance.Hide();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.dragging) return; //Dont show new tooltips when we are still holding our mouse button
            TooltipDialog.Instance.ShowMessage(tooltipText, rectTransform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipDialog.Instance.Hide();
        }

        public void OnScroll(PointerEventData eventData)
        {
            TooltipDialog.Instance.Hide();
        }
    }
}