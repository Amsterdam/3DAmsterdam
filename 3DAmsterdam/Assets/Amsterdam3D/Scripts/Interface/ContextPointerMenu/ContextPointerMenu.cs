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

    void Start()
    {
        contextItemsPanel.gameObject.SetActive(false);

        Button[] buttons = contextItemsPanel.GetComponentsInChildren<Button>();
        foreach(Button button in buttons)
            button.onClick.AddListener(CloseContextMenu);
    }

    void CloseContextMenu()
    {
        contextItemsPanel.gameObject.SetActive(false);
    }

    void Update()
    {   
        //TODO: replace with centralized input system
        if(Input.GetMouseButtonUp(1) && !EventSystem.current.IsPointerOverGameObject())
        {
            ShowOptions();  
		}
    }

	private void ShowOptions()
	{
        contextItemsPanel.transform.position = Input.mousePosition;

        contextItemsPanel.gameObject.SetActive(false);
        contextItemsPanel.gameObject.SetActive(true);
        
        //Disable/enable right buttons
        //Check if an object is was selected
    }
}
