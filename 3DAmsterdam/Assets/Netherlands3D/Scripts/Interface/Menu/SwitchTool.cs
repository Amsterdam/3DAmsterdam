using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.Menu {
	public class SwitchTool : MonoBehaviour
	{
		private Button[] containingButtons;

		private bool expanded = false;

		[SerializeField]
		private float spacing = 50;

		void Start()
		{
			containingButtons = GetComponentsInChildren<Button>();
			HideToolButtons();
		}

		/// <summary>
		/// Hide all tool buttons except the first (the active one)
		/// </summary>
		private void HideToolButtons()
		{
			expanded = false;
			for (int i = 0; i < containingButtons.Length; i++)
			{
				containingButtons[i].gameObject.SetActive((containingButtons[i].transform.GetSiblingIndex() == 0));
			}
		}

		private void AlignToolButtonsSideBySide()
		{
			expanded = true;
			for (int i = 0; i < containingButtons.Length; i++)
			{
				containingButtons[i].gameObject.SetActive(true);
				containingButtons[i].transform.localPosition = new Vector3(containingButtons[i].transform.GetSiblingIndex() * spacing, 0, 0);
			}
		}


		public void OrderToolItems()
		{
			if (expanded)
			{
				AlignToolButtonsSideBySide();
				HideToolButtons();
			}
			else {
				AlignToolButtonsSideBySide();
			}
		}
	}
}