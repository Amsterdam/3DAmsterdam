using Netherlands3D.Interface.SidePanel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tabv2 : MonoBehaviour
{

    [SerializeField]
    private TabPanelv2 tabPanel;
    public TabPanelv2 TabPanel { get => tabPanel; }

    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(delegate { ToggleChanged(); });
    }

    private void ToggleChanged()
    {

    }

    /// <summary>
    /// Method to use as dynamic bool in the editor (nice for toggles)
    /// </summary>
    /// <param name="open">Is this tab opened</param>
    public void OpenTab(bool open = true)
    {
        Debug.Log($"OpenTab {tabPanel.name}");

        GetComponent<Toggle>().isOn = open;
        if (SideTabPanel.Instance.open)
        {
            SideTabPanel.Instance.OpenPanel(tabPanel);
        } else
        {
            SideTabPanel.Instance.ClosePanel();
        }
    }
}
