using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SunDragWheel : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private RectTransform moonIcon;
    [SerializeField]
    private RectTransform sunIcon;

    [SerializeField]
    private float scrollWheelSensitivity = 1.0f;
    [SerializeField]
    private float dragSensitivity = 1.0f;

    [SerializeField]
    private float rotationSnapDegrees;
    private float previousPosition = 0.0f;

    private float angle;

    public delegate void ChangedSunWheel(float rotate);
    public ChangedSunWheel deltaTurn;

    private void Awake()
    {
        deltaTurn += UpdateIcons;
    }

    private void UpdateIcons(float rotate = 0.0f)
    {
        Debug.Log("Wheel rotation input " + rotate);
        //Keep icons straight
        moonIcon.rotation = Quaternion.identity;
        sunIcon.rotation = Quaternion.identity;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(RotateWheelByScrollInput());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
    }

    IEnumerator RotateWheelByScrollInput()
    {
        while (true)
        {
            //this.transform.Rotate(0, 0, -Input.mouseScrollDelta.y * scrollWheelSensitivity);
            if(Input.mouseScrollDelta.y != 0.0f)
                deltaTurn.Invoke(Input.mouseScrollDelta.y * scrollWheelSensitivity);
            yield return null;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        /*//Get starting rotation, so we rotate from this point
        Vector3 relativePosition = transform.position;
        relativePosition = Input.mousePosition - relativePosition;
        beginDragAngle = Mathf.Atan2(relativePosition.y, relativePosition.x) * Mathf.Rad2Deg;
        beginDragAngle -= Mathf.Atan2(transform.right.y, transform.right.x) * Mathf.Rad2Deg;*/

        previousPosition = Input.mousePosition.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Rotate wheel according to relative mouse position
        float relativePosition = Input.mousePosition.x - previousPosition;

        //angle = Mathf.Atan2(relativePosition.y, relativePosition.x) * Mathf.Rad2Deg - beginDragAngle;

        deltaTurn.Invoke(relativePosition * dragSensitivity);

        previousPosition = Input.mousePosition.x;
    }

	public void SetUpDirection(Vector3 newUpDirection)
	{
        this.transform.up = newUpDirection.normalized;
        UpdateIcons();
    }
}
