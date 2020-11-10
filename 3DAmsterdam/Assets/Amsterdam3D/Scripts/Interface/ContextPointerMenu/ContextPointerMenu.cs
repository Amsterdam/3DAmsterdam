using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContextPointerMenu : MonoBehaviour
{
    [SerializeField]
    private RectTransform contextItemsPanel;    

    void Start()
    {
        contextItemsPanel.gameObject.SetActive(false);
    }
    
    void Update()
    {
        if(Input.GetMouseButtonUp(1) && !EventSystem.current.IsPointerOverGameObject())
        {
            RetrieveOptions();  
		}
    }

	private void RetrieveOptions()
	{
        contextItemsPanel.gameObject.SetActive(false);
        contextItemsPanel.gameObject.SetActive(true);

        contextItemsPanel.transform.position = Input.mousePosition;

    }
}
