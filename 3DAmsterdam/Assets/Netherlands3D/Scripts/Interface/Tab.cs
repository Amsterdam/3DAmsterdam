using Netherlands3D.Interface.SidePanel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    public class Tab : MonoBehaviour
    {
        [SerializeField]
        private string tabPaneTitle = "";

        [SerializeField]
        private TabPanel tabPanel;
		public TabPanel TabPanel { get => tabPanel; }

        private Toggle toggle;

        [SerializeField]
        private GameObject[] toggleOjects;

		private void Awake()
		{
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate { ToggleChanged(); });
        }

		private void ToggleChanged()
		{
            foreach(GameObject gameObject in toggleOjects)
            {
                gameObject.SetActive(toggle.isOn);
			}
		}

		/// <summary>
		/// Method to use as dynamic bool in the editor (nice for toggles)
		/// </summary>
		/// <param name="open">Is this tab opened</param>
		public void OpenTab(bool open = true)
        {
            GetComponent<Toggle>().isOn = open;
            TabPanel.Open(open);
            if (open)
            {
                ServiceLocator.GetService<PropertiesPanel>().OpenPanel(tabPaneTitle);
            }
        }
    }
}