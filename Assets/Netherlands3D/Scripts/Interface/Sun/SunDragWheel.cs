using Netherlands3D.JavascriptConnection;
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

    public delegate void ChangedSunWheel(float rotate);
    public ChangedSunWheel deltaTurn;

    private void Awake()
    {
        deltaTurn += UpdateIcons;
    }

    private void UpdateIcons(float rotate = 0.0f)
    {
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
        ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.ERESIZE);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.AUTO);
    }

    /// <summary>
    /// Keeps checking the scroll input and invokes the deltaTurn event when needed
    /// </summary>
    IEnumerator RotateWheelByScrollInput()
    {
        while (true)
        {
            if(Input.mouseScrollDelta.y != 0.0f)
                deltaTurn.Invoke(Input.mouseScrollDelta.y * scrollWheelSensitivity);
            yield return null;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        previousPosition = Input.mousePosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 relativePosition = transform.position;
        relativePosition = Input.mousePosition - relativePosition;

        //Rotate wheel according to relative mouse position
        float increment = (relativePosition.y > 0) ? Input.mousePosition.x - previousPosition.x : -(Input.mousePosition.x - previousPosition.x);
        increment += (relativePosition.x < 0) ? Input.mousePosition.y - previousPosition.y : -(Input.mousePosition.y - previousPosition.y);

        deltaTurn.Invoke(increment * dragSensitivity);

        previousPosition = Input.mousePosition;
    }

    /// <summary>
    /// Change the wheel up position to another vector
    /// </summary>
    /// <param name="newUpDirection">The new direction vector</param>
	public void SetUpDirection(Vector3 newUpDirection)
	{
        this.transform.up = -newUpDirection.normalized;
        this.transform.rotation = Quaternion.Euler(this.transform.localEulerAngles.x, this.transform.localEulerAngles.y, -this.transform.localEulerAngles.z+180.0f);

        UpdateIcons();
    }
}
