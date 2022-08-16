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

		[SerializeField]
		private GameObject defaultToolSelection;

		void Start()
		{
			containingButtons = GetComponentsInChildren<Button>();
			HideToolButtons();
		}

		public void SelectToolButton(Button tool)
		{
			tool.GetComponent<Button>().onClick.Invoke();
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

		/// <summary>
		/// Simple force a click on our default pointer button, selecting our set default tool
		/// </summary>
		public void ResetToDefault()
		{
			defaultToolSelection.GetComponent<Button>().onClick.Invoke();
			HideToolButtons();
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