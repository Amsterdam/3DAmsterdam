using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using UnityEngine;

public class MeasurePoint : MonoBehaviour
{
    [SerializeField]
    private GameObject heightIndicator;

    [SerializeField]
    private GameObject pointIndicator;

	[SerializeField]
	private float pointScale = 1f;
	public float PointScale
    {
        get
        {
			return pointScale;
        }
        set
        {
			pointScale = value;
        }
    }

	public enum Shape{
		POINT,
		HEIGHT,
		NONE
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
				heightIndicator.SetActive(false);
				pointIndicator.SetActive(false);
				break;
		}
	}

	public void SetSelectable(bool selectable)
    {
		var layer = selectable ? LayerMask.NameToLayer("SelectionPoints") : LayerMask.NameToLayer("Default");
		heightIndicator.layer = layer;
		pointIndicator.layer = layer;
	}

    private void Update()
    {
		AutoScalePointByDistance();
    }

    public float AutoScalePointByDistance()
	{
		var cameraDistanceToPoint = Vector3.Distance(transform.position, ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position);
		transform.transform.localScale = Vector3.one * cameraDistanceToPoint * pointScale;

		return cameraDistanceToPoint;
	}
}
