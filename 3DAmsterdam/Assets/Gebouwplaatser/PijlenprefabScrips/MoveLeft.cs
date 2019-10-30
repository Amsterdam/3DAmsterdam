using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MoveLeft : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private GameObject _selectedObject;
    private SelectAndScale script;
    private bool buttonPressed = false;
    private float moveFactor = 5f;

    void Start()
    {
        script = GameObject.Find("UploadGebouwMenu").GetComponent<SelectAndScale>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonPressed = false;
    }

    private void Update()
    {
        _selectedObject = script.selectedObject;

        if (buttonPressed)
        {
            _selectedObject.transform.position -= new Vector3(moveFactor, 0, 0);
            gameObject.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            gameObject.GetComponent<Image>().color = Color.white;
        }
    }
}
