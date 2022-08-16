using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Interface.Modular
{
    public class DraggablePanelHandle : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        private RectTransform rectTransform;

        [SerializeField]
        private bool allowOutsideOfScreen = false;

        void Awake()
        {
            rectTransform = transform.parent.GetComponent<RectTransform>();
            if (!rectTransform) Destroy(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.position += new Vector3(eventData.delta.x, eventData.delta.y);

            if (!allowOutsideOfScreen)
            {
                //Clamp position (based on centered anchor)
                rectTransform.position = new Vector2(
                    Mathf.Clamp(rectTransform.position.x, (rectTransform.rect.width * CanvasSettings.canvasScale) / 2, Screen.width - ((rectTransform.rect.width * CanvasSettings.canvasScale) / 2.0f)),
                    Mathf.Clamp(rectTransform.position.y, (rectTransform.rect.height * CanvasSettings.canvasScale) / 2, Screen.height - ((rectTransform.rect.height * CanvasSettings.canvasScale) / 2.0f))
                );
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            MoveToFront();
        }

        private void MoveToFront()
        {
            KeepInFront keepInFront = rectTransform.GetComponent<KeepInFront>();
            if (keepInFront) keepInFront.MoveToFront();

        }
    }
}