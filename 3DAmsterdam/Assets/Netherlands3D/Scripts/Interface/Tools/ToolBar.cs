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

		public void DisableOtherTools(ToolMenuLink activatedToolInteractable = null)
		{
			ToolMenuLink[] otherToolInteractables = FindObjectsOfType<ToolMenuLink>();
			foreach (var otherTool in otherToolInteractables)
				if (otherTool != activatedToolInteractable) otherTool.gameObject.SetActive(false);

			activeMenuTool = (activatedToolInteractable) ? activatedToolInteractable.MenuTool : defaultMenuTool;

			highlightImage.transform.SetParent(activeMenuTool.transform,false);
		}

		public void SetDefault()
		{
			DisableOtherTools();
		}
	}
}