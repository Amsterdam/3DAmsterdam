using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AccordionController : MonoBehaviour
{
    [SerializeField]
    private int height = 48;


    [SerializeField]
    private bool isRoot = false;
    [SerializeField]
    private bool isOpen = false;
    private List<AccordionController> directChildren;

    //Visuals
    [SerializeField]
    private GameObject chevron;
    [SerializeField]
    private Sprite defaultSprite;
    [SerializeField]
    private Sprite openedSprite;

    private Image image;

    // Start is called before the first frame update
    void Start()
    {
        directChildren = new List<AccordionController>();
        foreach (Transform transform in transform)
        {
            AccordionController component;
            if ((component = transform.GetComponent<AccordionController>()) != null)
            {
                directChildren.Add(component);
            }
        }

        SetImage();

        //By default
        //Tabs are closed
        ToggleChildren(isOpen);

        //Chevron is (not) shown depending if there are children
        chevron.SetActive(directChildren.Count > 0);
    }

    private void PlaceChildren()
    {
        foreach (AccordionController child in GetComponentsInChildren<AccordionController>())
        {

        }
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
            var allChildren = gameObject.GetComponentsInChildren<AccordionController>();
            foreach (var child in allChildren)
            {
                child.gameObject.SetActive(true);

            }
        }
        else //Open
        {
            foreach (var child in directChildren)
            {
                child.gameObject.SetActive(true);
                Debug.Log($"child: {child.name}");
            }
        }

        image.sprite = turnOff ? openedSprite : defaultSprite;
    }
}
