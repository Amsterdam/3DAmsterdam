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
    private Vector3 previousPosition;

    private float angle;

    public delegate void ChangedSunWheel(float rotate);
    public ChangedSunWheel deltaTurn;

    private RectTransform rectTransform;

    private void Awake()
    {
        deltaTurn += UpdateIcons;
        rectTransform = GetComponent<RectTransform>();
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

        previousPosition = Input.mousePosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 relativePosition = transform.position;
        relativePosition = Input.mousePosition - relativePosition;

        //Rotate wheel according to relative mouse position
        float increment = (relativePosition.y > 0) ? Input.mousePosition.x - previousPosition.x : -(Input.mousePosition.x - previousPosition.x);
        increment += (relativePosition.x < 0) ? Input.mousePosition.y - previousPosition.y : -(Input.mousePosition.y - previousPosition.y);

        //angle = Mathf.Atan2(relativePosition.y, relativePosition.x) * Mathf.Rad2Deg - beginDragAngle;

        deltaTurn.Invoke(increment * dragSensitivity);

        previousPosition = Input.mousePosition;
    }

	public void SetUpDirection(Vector3 newUpDirection)
	{
        this.transform.up = -newUpDirection.normalized;
        this.transform.rotation = Quaternion.Euler(this.transform.localEulerAngles.x, this.transform.localEulerAngles.y, -this.transform.localEulerAngles.z+180.0f);

        UpdateIcons();
    }
}
