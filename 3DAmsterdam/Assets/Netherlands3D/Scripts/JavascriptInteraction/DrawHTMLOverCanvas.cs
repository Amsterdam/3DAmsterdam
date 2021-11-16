using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Netherlands3D.JavascriptConnection
{
	public class DrawHTMLOverCanvas : MonoBehaviour
	{
		[SerializeField]
		private string htmlObjectID = "";

		private Image image;

		[SerializeField]
		private bool alignEveryUpdate = false;

		private Rect screenSpaceRectangle;

		[Header("Padding (px) to add extra alignment if your WebGL canvas is not fullscreen")]
		[SerializeField]
		private float paddingX = 0;
		[SerializeField]
		private float paddingY = 60.0f;

		private void Awake()
		{
			image = GetComponent<Image>();
		}

		private void Update()
		{
			if (alignEveryUpdate)
				AlignHTMLOverlay();
		}

		/// <summary>
		/// Tell JavaScript to make a DOM object with htmlObjectID to align with the Image component
		/// </summary>
		private void AlignHTMLOverlay()
		{
			var screenSpaceRectangle = GetScreenSpaceRectangle();
			JavascriptMethodCaller.DisplayWithID(htmlObjectID, true,
				screenSpaceRectangle.x / Screen.width,
				screenSpaceRectangle.y / Screen.height,
				screenSpaceRectangle.width / Screen.width,
				screenSpaceRectangle.height / Screen.height,
				paddingX,
				paddingY
			);
		}

		/// <summary>
		/// Get the Image its rectangle in screenspace
		/// </summary>
		/// <returns>Rectangle with screenspace position, width and height</returns>
		private Rect GetScreenSpaceRectangle()
		{
			var size = Vector2.Scale(image.rectTransform.rect.size, image.rectTransform.lossyScale);
			screenSpaceRectangle = new Rect(image.rectTransform.position.x, image.rectTransform.position.y, size.x, size.y);
			screenSpaceRectangle.x -= (image.rectTransform.pivot.x * size.x);
			screenSpaceRectangle.y -= ((1.0f - image.rectTransform.pivot.y) * size.y);
			return screenSpaceRectangle;
		}

#if UNITY_WEBGL && !UNITY_EDITOR
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