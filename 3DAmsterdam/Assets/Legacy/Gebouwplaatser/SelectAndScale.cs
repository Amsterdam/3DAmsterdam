using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SelectAndScale : MonoBehaviour
{
    private MenuFunctions menuScript;

    public GameObject scaleMenu, uploadMenu, buildingMenu, cubeScaling, objectScaling;
    public Slider height, width, length; // kubussen
    public Slider size; // andere objecten
    private TextMeshProUGUI sizeText, heightText, widthText, lengthText;

    [HideInInspector]
    public GameObject selectedObject;
    private bool showMenu = false;

    private float scaleFactor = 100f;
    private string cubeTag, objectTag;
    private Vector3 startPos, startScale;

    void Start()
    {
        menuScript = GameObject.Find("Menus").GetComponent<MenuFunctions>();

        sizeText = objectScaling.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        heightText = cubeScaling.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        widthText = cubeScaling.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        lengthText = cubeScaling.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();

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

                    if (scaleMenu.activeSelf)
                    {
                        menuScript.currentMenu = 10;
                    }
                }
                else
                {
                    menuScript.currentMenu = 2;

                    startPos = selectedObject.transform.position;
                    startScale = selectedObject.transform.localScale;
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
        }
    }

    // opent en sluit het verschaalmenu
    private void OpenMenu()
    {
        if (showMenu)
        {
            scaleMenu.SetActive(true);
            buildingMenu.SetActive(false);
            uploadMenu.SetActive(false);
        } else
        {
            scaleMenu.SetActive(false);
            buildingMenu.SetActive(true);
            uploadMenu.SetActive(false);
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
            if (scale.x == 1)
            {
                size.value = 0;
            } else if (scale.x > 1)
            {
                size.value = scale.x * 10 - 10;
            } else
            {
                size.value = -(size.maxValue - (scale.x * size.maxValue));
            }
        }
    }

    // verandert de schaal van het object als de sliders aangepast worden
    private void ChangeScale()
    {
        if (selectedObject.tag == cubeTag)
        {
            heightText.text = ((int)(selectedObject.transform.localScale.y)).ToString() + "m";
            widthText.text = ((int)(selectedObject.transform.localScale.x)).ToString() + "m";
            lengthText.text = ((int)(selectedObject.transform.localScale.z)).ToString() + "m";

            selectedObject.transform.localScale = new Vector3(width.value * scaleFactor, 2*height.value * scaleFactor, length.value * scaleFactor);
        }
        else
        {
            sizeText.text = selectedObject.transform.localScale.x.ToString("F2") + "x";

            if (size.value > 0) // vergroten van object
            {
                selectedObject.transform.localScale = Vector3.one * ((size.value + 10) / 10);
            } else if (size.value == 0) // beginwaarde van object
            {
                selectedObject.transform.localScale = Vector3.one;
            } else if (size.value < 0 && size.value > (size.minValue + 5)) // verkleinen van object
            {
                selectedObject.transform.localScale = Vector3.one * ((size.maxValue - Mathf.Abs(size.value)) / size.maxValue);
            }
        }
    }

    public void Reset()
    {
        selectedObject.transform.position = startPos;

        if (selectedObject.transform.tag == cubeTag)
        {
            width.value = startScale.x / 100;
            height.value = startScale.y / 100;
            length.value = startScale.z / 100;
        } else
        {
            size.value = 0;
        }
    }
}
