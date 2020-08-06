using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IncrementalDragField : MonoBehaviour, IPointerDownHandler,IDragHandler, IEndDragHandler
{
	[SerializeField]
	private float horizontalIncrement = 5.0f;

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

    void Start()
    {
		        
    }

	public void OnPointerDown(PointerEventData eventData)
	{
		startDragX = Input.mousePosition.x;
		startValue = value;
	}

	public void OnDrag(PointerEventData eventData)
	{
		value = startValue + Mathf.RoundToInt((Input.mousePosition.x - startDragX) / horizontalIncrement);
		if (value > maxValue)
		{
			value = minValue + (value - maxValue);
		}
		else if(value < minValue)
		{
			value = maxValue + value;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		
	}

	// Update is called once per frame
	void Update()
    {
        
    }
}
