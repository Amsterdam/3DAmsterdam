using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RotateY : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private GameObject _selectedObject;
    private SelectAndScale script;
    private bool buttonPressed = false;
    private float rotationFactor = 3f;

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
            _selectedObject.transform.RotateAround(_selectedObject.transform.GetComponent<Collider>().bounds.center, Vector3.up, rotationFactor);
            gameObject.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            gameObject.GetComponent<Image>().color = Color.white;
        }
    }
}
