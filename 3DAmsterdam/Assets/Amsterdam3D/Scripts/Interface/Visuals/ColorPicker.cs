using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorPicker : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
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

	private float intensity = 1.0f;

	public delegate void PickedNewColor(Color color);
	public PickedNewColor pickedNewColor;

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
		PickColorFromPalette();
	}

	public void SetColorIntensity(float intensityValue){
		intensity = intensityValue;
		PickColorFromPalette();
		colorPalette.color = Color.Lerp(Color.black, Color.white, intensity);
	}

	void MovePointer()
	{
		dragRegionRectangle = RectTransformToScreenSpace(dragDropRegion);

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
		Vector3 inverseTransform = this.colorPalette.rectTransform.InverseTransformPoint(pointer.rectTransform.position);
		var colorPalette = (Texture2D)this.colorPalette.texture;
		int paletteWidth = colorPalette.width;
		int paletteHeight = colorPalette.height;

		var paletteRectangle = this.colorPalette.rectTransform.rect;
		inverseTransform.x -= paletteRectangle.x;
		inverseTransform.y -= paletteRectangle.y;
		inverseTransform.x /= paletteRectangle.width;
		inverseTransform.y /= paletteRectangle.height;

		if (radialConstraint && useVectorPalette)
		{
			//Get a radial vector color based on pointer position
			var redVector = Vector2.left;
			var greenVector = Quaternion.AngleAxis(-120, Vector3.forward) * redVector;
			var blueVector = Quaternion.AngleAxis(-120, Vector3.forward) * greenVector;

			var pointerLocalVector = pointer.rectTransform.anchoredPosition.normalized;
			var lightness = pointer.rectTransform.anchoredPosition.magnitude / (paletteWidth/2.0f);
			var red = Vector2.Dot(redVector, pointerLocalVector);
			var green = Vector2.Dot(greenVector, pointerLocalVector);
			var blue = Vector2.Dot(blueVector, pointerLocalVector);

			pickedColor = Color.Lerp(Color.white, new Color(red,green,blue), lightness) * intensity;
		}
		else
		{
			//Grab the raw texture pixel at the picker coordinates
			pickedColor = colorPalette.GetPixel((int)(inverseTransform.x * paletteWidth), (int)(inverseTransform.y * paletteHeight)) * intensity;
		}
		pickedColor.a = 1.0f;
		pointer.color = pickedColor;

		//Apply color to selected object/material
		pickedNewColor.Invoke(pickedColor);
	}

	public Rect RectTransformToScreenSpace(RectTransform transform)
	{
		Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
		return new Rect((Vector2)transform.position - (size * 0.5f), size);
	}
}
