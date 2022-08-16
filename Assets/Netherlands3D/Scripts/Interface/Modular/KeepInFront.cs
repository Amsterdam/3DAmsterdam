using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface.Modular
{
	public class KeepInFront : MonoBehaviour
	{
		void OnEnable()
		{
			MoveToFront();
		}

		public void MoveToFront()
		{
			RectTransform rectTransform = GetComponent<RectTransform>();
			if (rectTransform)
				rectTransform.SetAsLastSibling(); //Last sibling is in front
		}
	}
}