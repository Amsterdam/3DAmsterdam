using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasurePoint : MonoBehaviour
{
    [SerializeField]
    private GameObject heightIndicator;

    [SerializeField]
    private GameObject pointIndicator;

	public enum Shape{
		POINT,
		HEIGHT
	}

	private Shape shape = Shape.POINT;

	public void ChangeShape(Shape newShape)
	{
		shape = newShape;

		switch (shape)
		{
			case Shape.POINT:
				heightIndicator.gameObject.SetActive(false);
				pointIndicator.gameObject.SetActive(true);
				break;
			case Shape.HEIGHT:
				heightIndicator.gameObject.SetActive(true);
				pointIndicator.gameObject.SetActive(false);
				break;
			default:
				break;
		}
	}
}
