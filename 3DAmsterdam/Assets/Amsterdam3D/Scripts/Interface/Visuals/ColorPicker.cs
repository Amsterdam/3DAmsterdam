using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorPicker : ColorSelector, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
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

	private Vector3 redVector = Vector2.left;
	private Vector3 greenVector;
	private Vector3 blueVector;

	private bool ignoreChanges = false;

	public void OnPointerClick(PointerEventData eventData) => OnDrag(eventData);
	public void OnBeginDrag(PointerEventData eventData) => OnDrag(eventData);
	public void OnEndDrag(PointerEventData eventData) => OnDrag(eventData);
	public void OnDrag(PointerEventData eventData = null)
	{
		MovePointer();
		PickColorFromPalette();
	}

	void Start()
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

	public void SetColorIntensity(float intensityValue){
		if (!ignoreChanges)
		{
			intensity = intensityValue;
			PickColorFromPalette();
		}
	}

	void MovePointer()
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
			newPosition = Vector2.Lerp(dragRegionRectangle.center,newPosition,(radius / distanceFromCenter));
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
			var pointerLocalVector = pointer.rectTransform.anchoredPosition.normalized;
			var lightness = pointer.rectTransform.anchoredPosition.magnitude / (paletteWidth/2.0f);
			var red = Vector2.Dot(redVector, pointerLocalVector);
			var green = Vector2.Dot(greenVector, pointerLocalVector);
			var blue = Vector2.Dot(blueVector, pointerLocalVector);

			pickedColor = Color.Lerp(Color.white, new Color(red,green,blue), lightness);
			sliderArea.color = pickedColor;

			pickedColor *= intensity;
		}
		else
		{
			//Grab the raw texture pixel at the coordinates of the pointer
			pickedColor = colorPalette.GetPixel((int)(pointerPosition.x * paletteWidth), (int)(pointerPosition.y * paletteHeight)) * intensity;
		}
		pickedColor.a = 1.0f;
		pointer.color = pickedColor;
		sliderHandle.color = pickedColor;

		selectedNewColor.Invoke(pickedColor,this);
	}

	public override void ChangeColorInput(Color inputColor)
	{
		CalculateHitArea();

		var intensity = Vector3.Distance(new Vector3(inputColor.r, inputColor.g, inputColor.b), Vector3.zero);

		ignoreChanges = true;
		intensitySlider.value = Mathf.InverseLerp(0.0f,1.0f,intensity);
		ignoreChanges = false;

		var targetVector = ((inputColor.r * redVector) + (inputColor.g * greenVector) + (inputColor.b * blueVector)).normalized;
		targetVector = Vector3.Lerp(targetVector, Vector3.zero, Mathf.InverseLerp(1.0f, 2.0f, intensity));

		targetVector.x = (dragDropRegion.rect.width / 2.0f) * targetVector.x;
		targetVector.y = (dragDropRegion.rect.height / 2.0f) * targetVector.y;
		pointer.rectTransform.anchoredPosition = targetVector;

		inputColor.a = 1.0f;
		pointer.color = inputColor;
		sliderHandle.color = inputColor;
		sliderArea.color = inputColor;
	}

	public Rect RectTransformToScreenSpace(RectTransform transform)
	{
		Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
		return new Rect((Vector2)transform.position - (size * 0.5f), size);
	}
}
