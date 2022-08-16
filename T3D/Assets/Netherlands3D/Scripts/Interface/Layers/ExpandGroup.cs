using Netherlands3D.JavascriptConnection;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Interface.Layers
{
	public class ExpandGroup : ChangePointerStyleHandler
	{
		[SerializeField]
		private bool calculateHeightBasedOnChildren = true;

		private RectTransform rectTransform;
		private float closedHeight = 30.0f;
		private float openHeight = 30.0f;

		[SerializeField]
		private float moveSpeed = 5.0f;

		[SerializeField]
		private bool openGroup = true;

		[Tooltip("Group will be empty with this amount of children")]
		[SerializeField]
		private int defaultChildCount = 1;

		[SerializeField]
		private GameObject openGraphic;
		[SerializeField]
		private GameObject closeGraphic;

		void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			closedHeight = rectTransform.sizeDelta.y;

			ActiveIfGroupHasChildren();
		}

		private void OnEnable()
		{
			if (ActiveIfGroupHasChildren())
			{
				CalculateNewHeightAndResize();
			}
		}

		/// <summary>
		/// Calculates a new size for the group, and transition to this new size
		/// </summary>
		private void CalculateNewHeightAndResize()
		{
			if (calculateHeightBasedOnChildren)
			{
				CalculateMaximumHeight();
			}
			ShowOpenCloseGraphic();

			StopAllCoroutines();
			StartCoroutine(Open(openGroup));
		}

		/// <summary>
		/// Shows the correct caret direction
		/// </summary>
		private void ShowOpenCloseGraphic()
		{
			openGraphic.SetActive(openGroup);
			closeGraphic.SetActive(!openGroup);
		}

		/// <summary>
		/// Return if the group has children, and activates/deactivates accordingly.
		/// </summary>
		/// <returns>If this group have children</returns>
		private bool ActiveIfGroupHasChildren()
		{
			var hasChildren = transform.childCount > defaultChildCount;
			gameObject.SetActive(hasChildren);

			return hasChildren;
		}

		/// <summary>
		/// Catches changes in the child hierarchy
		/// </summary>
		void OnTransformChildrenChanged()
		{
			ActiveIfGroupHasChildren();

			if (gameObject.activeInHierarchy)
				CalculateNewHeightAndResize();
		}

		/// <summary>
		/// Toggle open/close for this group
		/// </summary>
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
					rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Mathf.Min(rectTransform.sizeDelta.y + moveSpeed * transform.childCount, openHeight));
					yield return new WaitForEndOfFrame();
				}
			}
			else
			{
				while (rectTransform.rect.height != closedHeight)
				{
					rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Mathf.Max(rectTransform.sizeDelta.y - moveSpeed * transform.childCount, closedHeight));
					yield return new WaitForEndOfFrame();
				}
			}
		}
	}
}