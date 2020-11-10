using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if(Input.GetMouseButtonDown(1)){
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
