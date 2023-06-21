using Netherlands3D.Interface.SidePanel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Events;
using UnityEngine.Events;

public class TabItem : MonoBehaviour
{

    [SerializeField]
    private SidePanel tabPanel;
    public SidePanel TabPanel { get => tabPanel; }

    private Toggle toggle;

    [Header("Events")]
    [SerializeField] private UnityEvent<bool> onChange;
    [SerializeField] private UnityEvent turnOn;
    [SerializeField] private UnityEvent turnOff;


    [Header("Image")]
    [SerializeField] public GameObject image;
    [SerializeField] private Sprite sprite;

    private void OnValidate()
    {
        SetImageToDefault();
    }

    public void SetImageToDefault()
    {
        if (image) image.GetComponent<Image>().sprite = sprite;
    }

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        if (toggle) toggle.onValueChanged.AddListener(delegate { ToggleChanged(); });
    }
    
    private void ToggleChanged()
    {  
        onChange.Invoke(toggle.isOn);

        if (toggle.isOn)
        {
            turnOn.Invoke();
        } else
        {
            turnOff.Invoke();
        }
    }

    public void SetActivePanel(bool isActive)
    {
        if (tabPanel) tabPanel.gameObject.SetActive(isActive);
    }



    /// <summary>
    /// Method to use as dynamic bool in the editor (nice for toggles)
    /// </summary>
    /// <param name="open">Is this tab opened</param>
    public void OpenTab(bool open = true)
    {
        //Debug.Log($"OpenTab {tabPanel.name}");

        //GetComponent<Toggle>().isOn = open;
        if (SideTabPanel.Instance)
        {
            if (/*SideTabPanel.Instance.open*/ open)
            {
                SideTabPanel.Instance.OpenPanel();
            }
            else
            {
                //SideTabPanel.Instance.ClosePanel();
            }
        }
        
    }
}
