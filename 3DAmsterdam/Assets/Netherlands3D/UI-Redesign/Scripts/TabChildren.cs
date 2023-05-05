using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabChildren : MonoBehaviour
{
    private bool childenIsOpen = false;
    private Tabv2[] children;

    [SerializeField]
    private GameObject image;
    private Sprite originalImage;

    void Start()
    {
        originalImage = image.GetComponent<Image>().sprite;
        children = gameObject.GetComponentsInChildren<Tabv2>() ?? new Tabv2[0];
        ToggleChildren(false);
    }

    public void ToggleChildren()
    {
        ToggleChildren(!childenIsOpen);
    }

    public void ToggleChildren(bool isOpen)
    {
        foreach (var child in children)
        {
            child.gameObject.SetActive(isOpen);
        }
    }

    public void ReplaceImage(Sprite image)
    {
        this.image.GetComponent<Image>().sprite = image;
    }

    public void ResetImage()
    {
        this.image.GetComponent<Image>().sprite = originalImage;
    }

}
