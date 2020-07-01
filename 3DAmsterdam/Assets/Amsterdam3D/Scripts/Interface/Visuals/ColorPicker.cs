using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorPicker : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[SerializeField]
	private RectTransform dragDropRegion;

	[SerializeField]
	private Image pointer;

	private Rect regionRect;
	private Rect pickerRect;

	public RawImage colorPalette;
	public Color pickedColor;

	public Material[] targetMaterials;
	public Image[] targetImages;

	[SerializeField]
	private bool radialConstraint = false;

	void Start()
	{
		Pick();
	}

	public void OnBeginDrag(PointerEventData EventData)
	{
		MovePicker();
		Pick();
	}
	public void OnDrag(PointerEventData EventData = null)
	{
		MovePicker();
		Pick();
	}
	public void OnEndDrag(PointerEventData EventData)
	{
		MovePicker();
		Pick();
	}
	void MovePicker()
	{
		regionRect = RectTransformToScreenSpace(dragDropRegion);
		pickerRect = RectTransformToScreenSpace(pointer.rectTransform);

		var newPosition = new Vector2(
			Mathf.Max(Mathf.Min(regionRect.max.x, Input.mousePosition.x), regionRect.min.x),
			Mathf.Max(Mathf.Min(regionRect.max.y, Input.mousePosition.y), regionRect.min.y)
		);

		if (radialConstraint)
		{
			var radius = regionRect.width * 0.5f;
			var distanceFromCenter = Vector2.Distance(regionRect.center, newPosition);
			newPosition = Vector2.Lerp(regionRect.center,newPosition,(radius / distanceFromCenter));
		}

		pointer.rectTransform.position = newPosition;
	}
	public void Pick()
	{
		//Lets inverse transform point so we can scale stuff as well
		Vector3 inverseTransform = this.colorPalette.rectTransform.InverseTransformPoint(pointer.rectTransform.position);
		var colorPalette = (Texture2D)this.colorPalette.texture;
		int W = colorPalette.width;
		int H = colorPalette.height;

		var paletteRectangle = this.colorPalette.rectTransform.rect;
		inverseTransform.x -= paletteRectangle.x;
		inverseTransform.y -= paletteRectangle.y;
		inverseTransform.x /= paletteRectangle.width;
		inverseTransform.y /= paletteRectangle.height;

		//Grab the raw texture pixel at the picker coordinates
		pickedColor = colorPalette.GetPixel((int)(inverseTransform.x * W), (int)(inverseTransform.y * H));
		pointer.color = pickedColor;

		//Apply color to selected object/material
		foreach (var material in targetMaterials)
		{
			material.color = pickedColor;
		}
	}

	public Rect RectTransformToScreenSpace(RectTransform transform)
	{
		Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
		return new Rect((Vector2)transform.position - (size * 0.5f), size);
	}
}
