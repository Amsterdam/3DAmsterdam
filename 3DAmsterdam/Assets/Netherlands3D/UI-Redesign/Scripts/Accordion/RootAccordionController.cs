using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RootAccordionController : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private Color[] backgroundColors;
    [SerializeField] private Sprite allRounded;
    [SerializeField] private Sprite bottomRounded;
    [SerializeField] private Sprite noRounded;

    [Header("Padding")]
    [SerializeField] private int defaultAmount = 24;
    [SerializeField] private int amountIncrease = 8;



    void OnValidate()
    {
        SetupAccordion();
    }

    private void Start()
    {
        SetupAccordion();
    }

    public void SetupAccordion()
    {
        SetColors(transform, 0);

        var controller = gameObject.GetComponent<DefaultAccordionController2>();
        controller.SetDefaultBackgroundSprite(allRounded);
        controller.SetOpenedBackgroundSprite(bottomRounded);

    }

    private void SetColors(Transform element, int index)
    {
        var controller = element.GetComponent<DefaultAccordionController2>();
        if (controller != null)
        {
            controller.SetBackgroundColor(backgroundColors[index]);
            controller.SetStartGroupLeftPadding(defaultAmount + amountIncrease * index);
            controller.SetAccordionComponents();

            var image = element.GetComponent<Image>();
            if (image) image.color = backgroundColors[index];

            foreach (Transform child in controller.AccordionChildrenGroup)
            {
                SetColors(child, index + 1);
            }
        }
        else
        {
            element.GetComponent<Image>().color = backgroundColors[index];
        }
    }


}
