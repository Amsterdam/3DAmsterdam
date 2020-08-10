using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LimitedNumericInput : MonoBehaviour, /*IPointerDownHandler,IDragHandler, IEndDragHandler,*/ IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private float horizontalIncrement = 5.0f;
	[SerializeField]
	private float scrollWheelSensitivity = 1.0f;

	/*
	private float startDragX = 0;
	private int startOffset = 0;

	[SerializeField]
	private int minValue = 0;
	public int MinValue { get => minValue; set => minValue = value; }

	[SerializeField]
	private int maxValue = 1440;
	public int MaxValue { get => maxValue; set => maxValue = value; }
	*/

	[SerializeField]
	private InputField inputField;

	public delegate void AddedOffset(int addedOffset);
	public AddedOffset addedOffset;

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		StartCoroutine(ReadScrollWheelInput());
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		StopAllCoroutines();
	}

	public void SetInputText(string textInput){
		inputField.text = textInput;
	}
	IEnumerator ReadScrollWheelInput() {
		while (true){
			var offset = Mathf.RoundToInt(Input.mouseScrollDelta.y * scrollWheelSensitivity);
			if(offset != 0.0f)
				addedOffset.Invoke(offset);
			yield return null;
		}	
	}

	/* Disabled dragging for now. Feels a bit confusing.
	public void OnPointerDown(PointerEventData eventData)
	{
		startDragX = Input.mousePosition.x;
		startOffset = Offset;
	}

	public void OnDrag(PointerEventData eventData)
	{
		Offset = startOffset + Mathf.RoundToInt((Input.mousePosition.x - startDragX) / horizontalIncrement);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		
	}
	*/
	// Update is called once per frame
	void Update()
    {
        
    }

}
