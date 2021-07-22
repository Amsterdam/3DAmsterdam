using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.Tools
{
	public class ToolBar : MonoBehaviour
	{
		[SerializeField]
		private MenuTool defaultMenuTool;

		[SerializeField]
		private MenuTool activeMenuTool;

		[SerializeField]
		private Image highlightImage;

		public static ToolBar Instance;

		private void Awake()
		{
			Instance = this;
		}

		public void ActivatedTool(ToolMenuLink activatedToolInteractable = null)
		{
			//Disable other tools
			ToolMenuLink[] otherToolInteractables = FindObjectsOfType<ToolMenuLink>();
			foreach (var otherTool in otherToolInteractables)
				if (otherTool != activatedToolInteractable) otherTool.gameObject.SetActive(false);

			activeMenuTool = (activatedToolInteractable) ? activatedToolInteractable.MenuTool : defaultMenuTool;
			MoveMenuHighlightToTool();
		}

		private void MoveMenuHighlightToTool()
		{
			highlightImage.transform.SetParent(activeMenuTool.transform, false);
		}

		public void DisabledTool(ToolMenuLink tool)
		{
			if (tool == activeMenuTool)
			{
				activeMenuTool = defaultMenuTool;
				MoveMenuHighlightToTool();
			}
		}

		public void SetDefault()
		{
			ActivatedTool();
		}
	}
}