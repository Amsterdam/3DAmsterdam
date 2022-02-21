using Netherlands3D.Interface.Modular;
using Netherlands3D.JavascriptConnection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Interface
{
    [RequireComponent(typeof(ChangePointerStyleHandler))]
    public class DraggablePanelHandle : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        private RectTransform rectTransform;
        [SerializeField]
        private bool allowOutsideOfScreen = false;

        [SerializeField]
        private bool rememberPosition = true;

        private const string saveName = "draggablePanelPosition_";

        ChangePointerStyleHandler changePointerStyle;
        void Awake()
        {
            changePointerStyle = this.GetComponent<ChangePointerStyleHandler>();
            changePointerStyle.StyleOnHover = ChangePointerStyleHandler.Style.GRAB;
            rectTransform = transform.parent.GetComponent<RectTransform>();
            if (!rectTransform) Destroy(this);

            if (rememberPosition) LoadPosition();
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

            if (rememberPosition)
            {
                SavePosition();
            }
        }

        private void LoadPosition()
        {
            if (!PlayerPrefs.HasKey($"{saveName}{this.transform.parent.gameObject.name}"))
                return;

            string[] xAndYPosition = PlayerPrefs.GetString($"{saveName}{this.transform.parent.gameObject.name}").Split(',');
            float.TryParse(xAndYPosition[0], out float xPosition);
            float.TryParse(xAndYPosition[1], out float yPosition);
            print($"Load panel position from memory: {this.transform.parent.gameObject.name} \n{xPosition},{yPosition}");

            this.transform.localPosition = new Vector3(
                xPosition,
                yPosition,
                0
            );
        }
        private void SavePosition()
		{
			PlayerPrefs.SetString($"{saveName}{this.transform.parent.gameObject.name}", $"{this.transform.localPosition.x},{this.transform.localPosition.y}");
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

        public void Close()
        {
            rectTransform.gameObject.SetActive(false);
        }
    }
}