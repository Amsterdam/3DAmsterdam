using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Amsterdam3D.JavascriptConnection
{
    public class DrawHTMLOverCanvas : MonoBehaviour
    {
        [SerializeField]
        private string htmlObjectID = "";

        private Image image;

        [SerializeField]
        private bool followEveryUpdate = false;

        private Rect screenSpaceRectangle;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void Update()
        {
            if (followEveryUpdate)
                AlignHTMLOverlay();
        }

        private void AlignHTMLOverlay()
        {
            var screenSpaceRectangle = GetScreenSpaceRectangle();
            JavascriptMethodCaller.DisplayWithID(htmlObjectID, true, Mathf.RoundToInt(screenSpaceRectangle.x), Mathf.RoundToInt(screenSpaceRectangle.y), Mathf.RoundToInt(screenSpaceRectangle.width), Mathf.RoundToInt(screenSpaceRectangle.height));
        }

        private Rect GetScreenSpaceRectangle()
        {
            var size = Vector2.Scale(image.rectTransform.rect.size, image.rectTransform.lossyScale);
            screenSpaceRectangle = new Rect(image.rectTransform.position.x, Screen.height - image.rectTransform.position.y, size.x, size.y);
            screenSpaceRectangle.x -= (image.rectTransform.pivot.x * size.x);
            screenSpaceRectangle.y -= ((1.0f - image.rectTransform.pivot.y) * size.y);
            return screenSpaceRectangle;
        }

#if UNITY_WEBGL && UNITY_EDITOR
        private void OnEnable()
        {
            AlignHTMLOverlay();
        }

        private void OnDisable()
        {
            JavascriptMethodCaller.DisplayWithID(htmlObjectID,false);
        }
#endif
    }
}