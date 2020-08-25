using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExpandGroup : MonoBehaviour
{
	[SerializeField]
	private bool hideWhenEmpty = true;

	[SerializeField]
	private bool calculateHeightBasedOnChildren = true;

	private RectTransform rectTransform;
	private float closedHeight = 30.0f;
	private float openHeight = 30.0f;

	[SerializeField]
	private float moveSpeed = 5.0f;

	[SerializeField]
	private bool openGroup = true;

	private int defaultChildCount = 0;

	[SerializeField]
	private GameObject openGraphic;
	[SerializeField]
	private GameObject closeGraphic;

	void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		closedHeight = rectTransform.sizeDelta.y;
		defaultChildCount = transform.childCount;

		if (ShowIfHasChildren())
		{
			ShowOpenCloseGraphic();
			StartCoroutine(Open(openGroup));
		}
	}

	private void ShowOpenCloseGraphic()
	{
		openGraphic.SetActive(openGroup);
		closeGraphic.SetActive(!openGroup);
	}

	private bool ShowIfHasChildren()
	{
		var hasChildren = !(transform.childCount <= defaultChildCount);
		gameObject.SetActive(hasChildren);

		return hasChildren;
	}

	void OnTransformChildrenChanged()
	{
		if (ShowIfHasChildren())
		{
			if (calculateHeightBasedOnChildren)
			{
				CalculateMaximumHeight();
			}
			ShowOpenCloseGraphic();

			StopAllCoroutines();
			StartCoroutine(Open(openGroup));
		}
	}

	public void ToggleGroup()
	{
		openGroup = !openGroup;
		ShowOpenCloseGraphic();

		StopAllCoroutines();
		StartCoroutine(Open(openGroup));
	}

	private void CalculateMaximumHeight()
	{
		openHeight = 0;
		foreach (Transform child in transform)
		{
			openHeight += child.GetComponent<RectTransform>().sizeDelta.y;
		}
	}

	IEnumerator Open(bool open)
	{
		if (open)
		{
			while (rectTransform.rect.height != openHeight)
			{
				rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Mathf.Min(rectTransform.sizeDelta.y + moveSpeed* transform.childCount, openHeight));
				yield return new WaitForEndOfFrame();
			}
		}
		else
		{
			while (rectTransform.rect.height != closedHeight)
			{
				rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Mathf.Max(rectTransform.sizeDelta.y - moveSpeed* transform.childCount, closedHeight));
				yield return new WaitForEndOfFrame();
			}
		}
	}
}
