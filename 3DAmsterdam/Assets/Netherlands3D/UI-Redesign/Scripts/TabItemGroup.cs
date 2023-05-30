using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TabItemGroup : MonoBehaviour
{
    private bool childenIsOpen = false;
    private TabItem[] children;

    void Awake()
    {
        children = gameObject.GetComponentsInChildren<TabItem>() ?? new TabItem[0];
        children = children.Where(w => w != children[0]).ToArray();
    }

    void Start()
    {
        ToggleChildren(false);
    }

    public void ToggleChildren()
    {
        ToggleChildren(!childenIsOpen);
    }

    public void ToggleChildren(bool isOpen)
    {
        GetComponent<RectMask2D>().enabled = !isOpen;

        foreach (var child in children)
        {
            //If you want to close the chidl group AND the toggle is on
            if (!isOpen && child.gameObject.GetComponent<Toggle>().isOn)
            {
                child.gameObject.GetComponent<Toggle>().isOn = false;
            }
        }
    }

    public void ReplaceImage(Sprite image)
    {
        GetComponent<TabItem>().image.GetComponent<Image>().sprite = image;
    }
}
