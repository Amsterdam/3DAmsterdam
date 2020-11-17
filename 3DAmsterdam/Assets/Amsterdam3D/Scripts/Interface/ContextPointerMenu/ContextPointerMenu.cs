using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextPointerMenu : MonoBehaviour
{
	[SerializeField]
	private RectTransform contextItemsPanel;

	public static ContextPointerMenu Instance = null;

	public ContextState state = ContextState.DEFAULT;

	[SerializeField]
	[Tooltip("Select buttons that should be active for specific states")]
	private StateButtons[] buttonsAvailableOnState;

	[Serializable]
	public class StateButtons
	{
		public ContextState state;
		public Button[] activeButtons;
	}

	public enum ContextState
	{
		DEFAULT,
		CUSTOM_OBJECTS,
		SELECTABLE_STATICS
	}

	private void Start()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		contextItemsPanel.gameObject.SetActive(false);

		Button[] buttons = contextItemsPanel.GetComponentsInChildren<Button>();
		foreach (Button button in buttons)
			button.onClick.AddListener(CloseContextMenu);
	}

	void CloseContextMenu()
	{
		contextItemsPanel.gameObject.SetActive(false);
		state = ContextState.DEFAULT;
	}

	public void SwitchState(ContextState newState)
	{
		state = newState;

		var buttons = GetComponentsInChildren<Button>();
		foreach (Button button in buttons)
			button.interactable = false;

		foreach (StateButtons stateButtons in buttonsAvailableOnState)
		{
			if (state == stateButtons.state)
			{
				foreach (Button button in stateButtons.activeButtons)
					button.interactable = true;
				return;
			}
		}
	}

	void Update()
	{
		//TODO: replace with centralized input system
		if (Input.GetMouseButtonUp(1) && !EventSystem.current.IsPointerOverGameObject())
		{
			ShowOptions();
		}
	}

	private void ShowOptions()
	{
		contextItemsPanel.transform.position = Input.mousePosition;

		contextItemsPanel.gameObject.SetActive(false);
		contextItemsPanel.gameObject.SetActive(true);
	}
}
