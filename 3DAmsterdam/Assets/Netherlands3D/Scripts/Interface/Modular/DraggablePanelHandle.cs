using Netherlands3D.Interface.Modular;
using Netherlands3D.JavascriptConnection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

        private const int marginPixels = 10;

        private int lastScreenWidth = 0;
        private int lastScreenHeight = 0;
        private Vector2 preferedSize;

        private bool panelHasContentSizeFitter = false;

        private Animator animator;

        void Awake()
        {
            animator = GetComponent<Animator>();
            if (animator) animator.enabled = false;

            changePointerStyle = this.GetComponent<ChangePointerStyleHandler>();
            changePointerStyle.StyleOnHover = ChangePointerStyleHandler.Style.GRAB;
            rectTransform = transform.parent.GetComponent<RectTransform>();
            if (!rectTransform) Destroy(this);

            preferedSize = rectTransform.sizeDelta;

            if (rememberPosition) LoadPosition();
        }

		private void Start()
		{
            panelHasContentSizeFitter = (rectTransform.GetComponent<ContentSizeFitter>());
            StartCoroutine(ClampInScreenBounds());
        }

        private void OnEnable()
        {
            StartCoroutine(ClampInScreenBounds());
        }

        private void LateUpdate()
		{
			if(Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;

                StartCoroutine(ClampInScreenBounds());
            }
        }

		public void OnDrag(PointerEventData eventData)
		{
			rectTransform.position += new Vector3(eventData.delta.x, eventData.delta.y);

            Clamp();

            if (rememberPosition)
			{
				SavePosition();
			}
		}

		private IEnumerator ClampInScreenBounds()
        {
            //Delay one frame to make sure canvas item is up to date.
            yield return new WaitForEndOfFrame();
            Clamp();
        }

        private void Clamp()
        {
            if (!allowOutsideOfScreen)
            {
                //Make sure this panel is small enought to fit in current screen in height if it does not have a contentsizefitter overruling it.
                if (!panelHasContentSizeFitter)
                {
                    Vector2 currentSize = rectTransform.sizeDelta;
                    currentSize.y = Math.Min(Screen.height - (marginPixels * 2), preferedSize.y);
                    rectTransform.sizeDelta = currentSize;
                }

                //Clamp position (based on centered anchor)
                rectTransform.position = new Vector2(
                    Mathf.Clamp(rectTransform.position.x, (rectTransform.rect.width * CanvasSettings.canvasScale) / 2, Screen.width - ((rectTransform.rect.width * CanvasSettings.canvasScale) / 2.0f)),
                    Mathf.Clamp(rectTransform.position.y, (rectTransform.rect.height * CanvasSettings.canvasScale) / 2, Screen.height - ((rectTransform.rect.height * CanvasSettings.canvasScale) / 2.0f))
                );
            }
        }

        private void LoadPosition()
        {
            if (!PlayerPrefs.HasKey($"{saveName}{rectTransform.gameObject.name}"))
                return;

            string[] xAndYPosition = PlayerPrefs.GetString($"{saveName}{rectTransform.gameObject.name}").Split(',');
            float.TryParse(xAndYPosition[0], out float xPosition);
            float.TryParse(xAndYPosition[1], out float yPosition);
            print($"Load panel position from memory: {rectTransform.gameObject.name} \n{xPosition},{yPosition}");

            rectTransform.localPosition = new Vector3(
                xPosition,
                yPosition,
                0
            );

            ClampInScreenBounds();
        }
        private void SavePosition()
		{
			PlayerPrefs.SetString($"{saveName}{rectTransform.gameObject.name}", $"{rectTransform.localPosition.x},{rectTransform.localPosition.y}");
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