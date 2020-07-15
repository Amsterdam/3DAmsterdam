using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickOutsideToClose : MonoBehaviour
{
    [SerializeField]
    private GameObject[] additionalGameObjects;

    void Update()
    {
        if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            this.gameObject.SetActive(ClickingSelfOrChild());
        }
    }
    private bool ClickingSelfOrChild()
    {
        //Check self and children
        RectTransform[] rectTransforms = GetComponentsInChildren<RectTransform>();
        foreach(RectTransform rectTransform in rectTransforms)
        {
            if (EventSystem.current.currentSelectedGameObject == rectTransform.gameObject)
            {
                return true;
            };
        }

        //Check the additional list we set manualy
        foreach(GameObject otherGameObject in additionalGameObjects)
        {
            if (EventSystem.current.currentSelectedGameObject == otherGameObject)
            {
                return true;
            };
        }

        return false;
    }
}
