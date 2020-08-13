using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Annotation : WorldPointFollower, IDragHandler, IPointerClickHandler
{
    [SerializeField]
    private Image balloon;

    [SerializeField]
    private Text balloonText;

    [SerializeField]
    private InputField editInputField;

    private Plane groundPlane;
    private float lastClickTime = 0;
    private float doubleClickTime = 0.2f;

    public void OnDrag(PointerEventData eventData)
    {
        groundPlane = new Plane(Vector3.up,new Vector3(0, Constants.ZERO_GROUND_LEVEL_Y, 0));
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
       
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            MoveToWorldPosition(hitPoint);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.time - lastClickTime < doubleClickTime)
        {
            StartEditingText();
        }
        lastClickTime = Time.time;
    }

    private void StartEditingText()
    {
        editInputField.gameObject.SetActive(true);
        editInputField.text = balloonText.text;

        editInputField.Select();
    }

    public void StopEditingText()
    {
        balloonText.text = editInputField.text;
        editInputField.gameObject.SetActive(false);
    }
} 
