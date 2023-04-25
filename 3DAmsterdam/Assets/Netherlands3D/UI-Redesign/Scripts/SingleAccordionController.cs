using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
        SetImage();

        //By default
        //Tabs are closed
        ToggleChildren(false);

        //Chevron is (not) shown depending if there are children
        chevron.SetActive(children.Length > 0);
    }

    private void SetImage()
    {
        if (image == null) image = this.gameObject.GetComponent<Image>();
    }


    public void ChevronPressed()
    {
        SetImage();
        isOpen = !isOpen;
        ToggleChildren(isOpen);

        Debug.Log($"Chevron pressed: {isOpen}");
    }

    private void ToggleChildren(bool turnOff)
    {
        Debug.Log($"ToggleChildren: {turnOff} {gameObject.name}");

        if (turnOff) //Close
        {
            CascadeChildren(children, false);

        } else //Opem
        {
            foreach(var child in children)
            {
                child.SetActive(true);
                Debug.Log($"child: {child.name}");
            }
        }

        image.sprite = turnOff ? openedSprite : defaultSprite;
    }

    private void CascadeChildren(GameObject[] children, bool setActive)
    {
        foreach (GameObject child in children)
        {
            var accordion = child.GetComponent<SingleAccordionController>();
            if (accordion == null) return;

            child.SetActive(setActive);
            CascadeChildren(accordion.children, setActive);

            Debug.Log($"child: {child.name}");

        }
    }

}
