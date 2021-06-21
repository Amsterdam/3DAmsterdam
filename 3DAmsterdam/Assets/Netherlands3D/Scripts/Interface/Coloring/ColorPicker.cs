using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Netherlands3D.Interface.Coloring 
{
	public class ColorPicker : ColorSelector, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
	{
		[SerializeField]
		private RectTransform dragDropRegion;

		[SerializeField]
		private Image pointer;

		private Rect dragRegionRectangle;

		public RawImage colorPalette;
		public Color pickedColor;

		[SerializeField]
		private bool useVectorPalette = false;
		[SerializeField]
		private bool radialConstraint = false;

		[SerializeField]
		private Slider intensitySlider;
		private float intensity = 1.0f;
		[SerializeField]
		private Image sliderHandle;
		[SerializeField]
		private Image sliderArea;

		private Vector3 redVector = Vector2.right;
		private Vector3 greenVector;
		private Vector3 blueVector;

		private bool ignoreChanges = false;

		public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);
		public void OnBeginDrag(PointerEventData eventData) => OnDrag(eventData);
		public void OnEndDrag(PointerEventData eventData) => OnDrag(eventData);
		public void OnDrag(PointerEventData eventData = null)
		{
			MovePointer();
			PickColorFromPalette();
		}

		void Awake()
		{
			if (useVectorPalette && radialConstraint)
			{
				//Radial vector color based on pointer position
				//This is based on a color wheel with red on the left, and green and blue following clockwise
				greenVector = Quaternion.AngleAxis(-120, Vector3.forward) * redVector;
				blueVector = Quaternion.AngleAxis(-120, Vector3.forward) * greenVector;
			}
		}

		public void CalculateHitArea()
		{
			dragRegionRectangle = RectTransformToScreenSpace(dragDropRegion);
		}

		public void SetColorIntensity(float intensityValue) {
			intensity = intensityValue;
			PickColorFromPalette();
		}

		private void MovePointer()
		{
			CalculateHitArea();

			var newPosition = new Vector2(
				Mathf.Max(Mathf.Min(dragRegionRectangle.max.x, Input.mousePosition.x), dragRegionRectangle.min.x),
				Mathf.Max(Mathf.Min(dragRegionRectangle.max.y, Input.mousePosition.y), dragRegionRectangle.min.y)
			);

			if (radialConstraint)
			{
				var radius = dragRegionRectangle.width * 0.5f;
				var distanceFromCenter = Vector2.Distance(dragRegionRectangle.center, newPosition);
				newPosition = Vector2.Lerp(dragRegionRectangle.center, newPosition, (radius / distanceFromCenter));
			}

			pointer.rectTransform.position = newPosition;
		}
		public void PickColorFromPalette()
		{
			//Lets inverse transform point so we can scale stuff as well
			Vector3 pointerPosition = this.colorPalette.rectTransform.InverseTransformPoint(pointer.rectTransform.position);
			var colorPalette = (Texture2D)this.colorPalette.texture;
			int paletteWidth = colorPalette.width;
			int paletteHeight = colorPalette.height;

			var paletteRectangle = this.colorPalette.rectTransform.rect;
			pointerPosition.x -= paletteRectangle.x;
			pointerPosition.y -= paletteRectangle.y;
			pointerPosition.x /= paletteRectangle.width;
			pointerPosition.y /= paletteRectangle.height;

			if (radialConstraint && useVectorPalette)
			{
				var pointerLocalVector = pointer.rectTransform.anchoredPosition;
				var lightness = pointer.rectTransform.anchoredPosition.magnitude / (paletteWidth / 2.0f);

				Quaternion rotation = Quaternion.FromToRotation(pointerLocalVector, Vector3.right);
				var hue = rotation.eulerAngles.z / 360.0f;
				pickedColor = Color.HSVToRGB(hue, lightness, intensity);
				var pickedColorWithoutIntensity = Color.HSVToRGB(hue, lightness, 1.0f);

				sliderHandle.color = pickedColor;
				sliderArea.color = pickedColorWithoutIntensity;
				pointer.color = pickedColorWithoutIntensity;
			}
			else
			{
				//Grab the raw texture pixel at the coordinates of the pointer
				pickedColor = colorPalette.GetPixel((int)(pointerPosition.x * paletteWidth), (int)(pointerPosition.y * paletteHeight)) * intensity;

				sliderHandle.color = pickedColor;
				sliderArea.color = pickedColor;
				pointer.color = pickedColor;
			}
			pickedColor.a = 1.0f;

			selectedNewColor.Invoke(pickedColor, this);
		}

		public override void ChangeColorInput(Color inputColor)
		{
			CalculateHitArea();
			print(inputColor);

			Color.RGBToHSV(inputColor, out float hue, out float saturation, out float value);

			float radius = dragDropRegion.rect.width / 2.0f;
			float colorRadius = saturation * radius;
			float angle = ((1.0f - hue) * (2.0f * Mathf.PI));

			float xOffset = Mathf.Cos(angle) * colorRadius; //offset from the midpoint of the circle
			float yOffset = Mathf.Sin(angle) * colorRadius;

			intensitySlider.SetValueWithoutNotify(value);

			Vector3 anchorTarget = new Vector3();
			anchorTarget.x = xOffset;
			anchorTarget.y = yOffset;
			pointer.rectTransform.anchoredPosition = anchorTarget;

			inputColor.a = 1.0f;
			sliderHandle.color = inputColor;

			sliderArea.color = Color.HSVToRGB(hue, saturation, 1.0f);
			pointer.color = sliderArea.color;
		}

		public Rect RectTransformToScreenSpace(RectTransform transform)
		{
			Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
			return new Rect((Vector2)transform.position - (size * 0.5f), size);
		}
	}
}