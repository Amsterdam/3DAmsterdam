using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LimitedNumericInput : MonoBehaviour, /*IPointerDownHandler,IDragHandler, IEndDragHandler,*/ IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private float horizontalIncrement = 5.0f;
	[SerializeField]
	private float scrollWheelSensitivity = 1.0f;

	private float startDragX = 0;

	private int startValue = 0;

	[SerializeField]
	private int minValue = 0;
	[SerializeField]
	private int maxValue = 1440;

	[SerializeField]
	private bool loop = false;

	[SerializeField]
	private int value = 0;

	[SerializeField]
	private InputField inputField;

	public int Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
			if (Value > maxValue)
			{
				Value = minValue + (Value - maxValue);
			}
			else if (Value < minValue)
			{
				Value = maxValue + Value;
			}

			if (inputField) inputField.text = value.ToString();
		}
	}
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

	IEnumerator ReadScrollWheelInput() {
		while (true){ 
			Value += Mathf.RoundToInt(Input.mouseScrollDelta.y * scrollWheelSensitivity);
			yield return null;
		}	
	}

	/* Disabled dragging for now. Feels a bit confusing.
	public void OnPointerDown(PointerEventData eventData)
	{
		startDragX = Input.mousePosition.x;
		startValue = Value;
	}

	public void OnDrag(PointerEventData eventData)
	{
		Value = startValue + Mathf.RoundToInt((Input.mousePosition.x - startDragX) / horizontalIncrement);
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
