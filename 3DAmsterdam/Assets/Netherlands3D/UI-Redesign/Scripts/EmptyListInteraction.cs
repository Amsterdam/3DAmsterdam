using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EmptyListInteraction : MonoBehaviour
{

    [SerializeField]
    private GameObject defaultText;

    [SerializeField]
    private Transform parentGroup;


    public void ShowDefaultText()
    {
        bool isChildren = false;
        foreach(Transform transform in parentGroup)
        {
            isChildren = true;
        }

        defaultText.SetActive(!isChildren);
    }
}
