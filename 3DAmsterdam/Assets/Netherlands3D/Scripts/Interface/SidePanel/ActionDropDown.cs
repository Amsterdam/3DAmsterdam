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

		public void SetAction(string[] dropdownOptions, Action<string> selectOptionAction)
		{
			optionAction = selectOptionAction;

			dropdown.ClearOptions();
			var options = new List<Dropdown.OptionData>();
			foreach (var option in dropdownOptions)
				dropdown.options.Add(new Dropdown.OptionData() { text = option });

			dropdown.onValueChanged.AddListener(delegate
			{
				optionAction.Invoke(dropdown.options[dropdown.value].text);
			});
		}

		private void OnDestroy()
		{
			dropdown.onValueChanged.RemoveAllListeners();
		}
	}
}