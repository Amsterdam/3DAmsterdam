using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.Modular
{
    public class KeepInScreenBounds : MonoBehaviour
    {
        private Image mainImage;
        private Vector3 limitedPosition;

        private Vector3 startPosition;

        private void Awake()
        {
            mainImage = GetComponent<Image>();
            startPosition = this.transform.localPosition;
        }

        void OnEnable()
        {
            limitedPosition = new Vector3(
                Mathf.Clamp(this.transform.position.x, 0, Screen.width - this.mainImage.rectTransform.sizeDelta.x * CanvasSettings.canvasScale),
                Mathf.Clamp(this.transform.position.y, this.mainImage.rectTransform.sizeDelta.y * CanvasSettings.canvasScale, Screen.height),
                0
            );

            this.transform.position = limitedPosition;
        }

        private void OnDisable()
        {
            this.transform.localPosition = startPosition;
        }
    }
}