using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectAndScale : MonoBehaviour
{
    public GameObject scaleMenu;
    public Slider height, width, length;

    [HideInInspector]
    public GameObject selectedObject;
    private bool showMenu = false;

    private float scaleFactor = 100f;

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
                if (!(selectedObject.tag == "Sizeable" || selectedObject.tag == "CustomPlaced"))
                {
                    selectedObject = null;
                }
            }
        }

        if (selectedObject != null)
        {
            showMenu = true;
        } else
        {
            showMenu = false;
        }

        OpenMenu();

        if (selectedObject != null)
        {
            ChangeScale();
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
        height.value = selectedObject.transform.localScale.y / 100;
        width.value = selectedObject.transform.localScale.x / 100;
        length.value = selectedObject.transform.localScale.z / 100;
    }

    // verandert de schaal van het object als de sliders aangepast worden
    private void ChangeScale()
    {
        selectedObject.transform.localScale = new Vector3(width.value * scaleFactor, height.value * scaleFactor, length.value * scaleFactor);
    }
}
