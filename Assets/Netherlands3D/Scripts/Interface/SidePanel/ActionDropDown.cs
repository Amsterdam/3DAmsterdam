using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.SidePanel
{
	public class ActionDropDown : MonoBehaviour
	{
		[SerializeField]
		private Dropdown dropdown;
		private Action<string> optionAction;

		private string currentValue = "";

		public void SetAction(string[] dropdownOptions, Action<string> selectOptionAction, string selected = "")
		{
			optionAction = selectOptionAction;
			gameObject.name = "Dropdown: " + selected;
			dropdown.ClearOptions();

			UpdateOptions(dropdownOptions);

			if (selected != "")
			{
				dropdown.value = Array.IndexOf(dropdownOptions, selected);
				currentValue = selected;
			}
			dropdown.onValueChanged.AddListener(delegate
			{
				var newSelection = dropdown.options[dropdown.value].text;

				gameObject.name = "Dropdown: " + newSelection;
				optionAction.Invoke(newSelection);
			});
		}

		public void UpdateOptions(string[] dropdownOptions)
		{
			dropdown.value = 0;
			dropdown.options.Clear();
			foreach (var option in dropdownOptions)
				dropdown.options.Add(new Dropdown.OptionData() { text = option });
			
		}

		private void OnDestroy()
		{
			dropdown.onValueChanged.RemoveAllListeners();
		}
	}
}