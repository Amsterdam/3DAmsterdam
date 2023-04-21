using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleAccordionController : MonoBehaviour
{
    //Logic
    public bool isOpen = false;
    public GameObject[] children;

    //Visuals
    public GameObject chevron;
    public Sprite defaultSprite;
    public Sprite openedSprite;

    private Image image;

    // Start is called before the first frame update
    void Start()
    {
        //By default
        //Tabs are closed
        CloseChildren();

        //Chevron is (not) shown depending if there are children
        chevron.SetActive(children.Length > 0);

        SetImage();
    }

    private void SetImage()
    {
        if (image == null) image = this.gameObject.GetComponent<Image>();
    }


    public void ChevronPressed()
    {
        SetImage();

        if (isOpen)
        {
            CloseChildren();
        }
        else
        {
            OpenChildren();
        }

        isOpen = !isOpen;
        Debug.Log($"Chevron pressed: {isOpen}");
    }

    private void OpenChildren()
    {
        foreach(var child in children)
        {
            child.gameObject.SetActive(true);
        }


        Debug.Log($"OpenChildren1: {image.sprite}");
        Debug.Log($"OpenChildren2: {openedSprite}");

        image.sprite = openedSprite;
    }


    private void CloseChildren()
    {
        foreach (var child in children)
        {
            child.gameObject.SetActive(false);
        }

        image.sprite = defaultSprite;
    }
}
