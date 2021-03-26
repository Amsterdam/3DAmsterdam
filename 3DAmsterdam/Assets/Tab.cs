using Netherlands3D.Interface.SidePanel;
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

		/// <summary>
		/// Method to use as dynamic bool in the editor (nice for toggles)
		/// </summary>
		/// <param name="open">Is this tab opened</param>
		public void OpenTab(bool open = true)
        {
            GetComponent<Toggle>().isOn = open;
            tabPanel.Open(open);
            if (open)
            {
                PropertiesPanel.Instance.OpenPanel(tabPaneTitle);
            }
        }
    }
}