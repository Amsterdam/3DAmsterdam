using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SelectAndScale : MonoBehaviour
{
    public GameObject scaleMenu, cubeScaling, objectScaling;
    public Slider height, width, length; // kubussen
    public Slider size; // andere objecten
    public TextMeshProUGUI text;
    private TextMeshProUGUI _text, sizeText;

    [HideInInspector]
    public GameObject selectedObject;
    private bool showMenu = false;

    private float scaleFactor = 100f;
    private string cubeTag, objectTag;

    void Start()
    {
        _text = text.GetComponent<TextMeshProUGUI>();
        sizeText = objectScaling.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

        scaleMenu.SetActive(false);

        cubeTag = "Sizeable";
        objectTag = "CustomPlaced";
    }

    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                // het geselecteerde object is het object waar op geklikt wordt
                selectedObject = hit.collider.gameObject;

                SetStartValues();
            
                // als er niet op een correct object wordt geklikt is het geselecteerde object nul
                if (!(selectedObject.tag == cubeTag || selectedObject.tag == objectTag))
                {
                    selectedObject = null;
                }
            }
        }

        if (selectedObject != null)
        {
            showMenu = true;
        }
        else
        {
            showMenu = false;
        }

        OpenMenu();

        if (selectedObject != null)
        {
            if (selectedObject.tag == cubeTag)
            {
                cubeScaling.SetActive(true);
                objectScaling.SetActive(false);
            }
            else
            {
                cubeScaling.SetActive(false);
                objectScaling.SetActive(true);
            }

            ChangeScale();
            ObjectIndication();
        }
    }

    // opent en sluit het verschaalmenu
    private void OpenMenu()
    {
        if (showMenu)
        {
            scaleMenu.SetActive(true);
        } else
        {
            scaleMenu.SetActive(false);
        }
    }

    // zet de juiste beginwaardes voor het verschaalmenu
    private void SetStartValues()
    {
        var scale = selectedObject.transform.localScale;

        if (selectedObject.tag == cubeTag)
        {
            height.value = scale.y / scaleFactor;
            width.value = scale.x / scaleFactor;
            length.value = scale.z / scaleFactor;
        }
        else
        {
            Debug.Log("ei");
            //switch (scale.x)
            //{
            //    case 0.125f:
            //        size.value = 0;
            //        break;
            //    case 0.25f:
            //        size.value = 1;
            //        break;
            //    case 0.5f:
            //        size.value = 2;
            //        break;
            //    case 1f:
            //        size.value = 3;
            //        break;
            //    case 2f:
            //        size.value = 4;
            //        break;
            //    case 4f:
            //        size.value = 5;
            //        break;
            //    case 8f:
            //        size.value = 6;
            //        break;
            //}
        }
    }

    // verandert de schaal van het object als de sliders aangepast worden
    private void ChangeScale()
    {
        if (selectedObject.tag == cubeTag)
        {
            selectedObject.transform.localScale = new Vector3(width.value * scaleFactor, height.value * scaleFactor, length.value * scaleFactor);
        }
        else
        {
            sizeText.text = selectedObject.transform.localScale.x.ToString("F2") + "x";

            if (size.value > 0)
            {
                selectedObject.transform.localScale = Vector3.one * ((size.value + 10) / 10);
            } else if (size.value == 0)
            {
                selectedObject.transform.localScale = Vector3.one;
            } else if (size.value < 0 && size.value > (size.minValue + 5))
            {
                selectedObject.transform.localScale = Vector3.one * ((size.maxValue - Mathf.Abs(size.value)) / size.maxValue);
            } 
        }
    }

    // naam van het object wordt weergegeven boven het menu
    private void ObjectIndication()
    {
        _text.text = selectedObject.gameObject.name;
    }
}
