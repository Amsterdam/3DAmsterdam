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

		public void SetAction(List<Dropdown.OptionData> dropdownOptions, Action<string> selectOptionAction)
		{
			optionAction = selectOptionAction;
			dropdown.ClearOptions();
			dropdown.AddOptions(dropdownOptions);
			dropdown.onValueChanged.AddListener(delegate {
				optionAction.Invoke(dropdown.options[dropdown.value].text);
			});
		}

		private void OnDestroy()
		{
			dropdown.onValueChanged.RemoveAllListeners();
		}
	}
}