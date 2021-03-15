using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchTool : MonoBehaviour
{
    private Button[] containingButtons;
	[SerializeField]
	private HorizontalLayoutGroup forceOrderGroup;

	private bool expanded = false;

    void Start()
	{
		containingButtons = GetComponentsInChildren<Button>();

		if(!expanded)
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
			if(containingButtons[i].transform.GetSiblingIndex() != 0)
				containingButtons[i].gameObject.SetActive(false);
		}
	}

	private void ShowToolButtons()
	{
		expanded = true;
		for (int i = 0; i < containingButtons.Length; i++)
		{
			containingButtons[i].gameObject.SetActive(true);
		}
		forceOrderGroup.enabled = true;
		forceOrderGroup.enabled = false;
	}


	public void SwitchTo(int tool = 0)
    {
		if(expanded)
		{
			ActivateTool(tool);
			HideToolButtons();
		}
		else{
			
		}
	}

	private void ActivateTool(int tool)
	{
		switch (tool)
		{
			case 0:
				//
				break;
			case 1:
				//
				break;
			default:
				break;
		}
	}
}
