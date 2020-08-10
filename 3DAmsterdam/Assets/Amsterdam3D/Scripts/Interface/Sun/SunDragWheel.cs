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
    private float rotationSnapDegrees;
    private float beginDragAngle = 0.0f;

    private float angle;

    public delegate void ChangedSunWheel(float rotation);
    public ChangedSunWheel changedDirection;

    private void Awake()
    {
        changedDirection += UpdateIcons;
    }

    private void UpdateIcons(float rotation = 0.0f)
    {
        Debug.Log("Changed sun wheel to " + rotation);
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
            this.transform.Rotate(0, 0, -Input.mouseScrollDelta.y * scrollWheelSensitivity);
            changedDirection.Invoke(transform.rotation.z);
            yield return null;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Get starting rotation, so we rotate from this point
        Vector3 relativePosition = transform.position;
        relativePosition = Input.mousePosition - relativePosition;
        beginDragAngle = Mathf.Atan2(relativePosition.y, relativePosition.x) * Mathf.Rad2Deg;
        beginDragAngle -= Mathf.Atan2(transform.right.y, transform.right.x) * Mathf.Rad2Deg;

        changedDirection.Invoke(transform.rotation.z);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Rotate wheel according to relative mouse position
        Vector3 relativePosition = transform.position;
        relativePosition = Input.mousePosition - relativePosition;
       
        angle = Mathf.Atan2(relativePosition.y, relativePosition.x) * Mathf.Rad2Deg - beginDragAngle;
        angle -= (angle % (360.0f / rotationSnapDegrees));
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        changedDirection.Invoke(transform.rotation.z);
    }

	public void SetUpDirection(Vector3 newUpDirection)
	{
        this.transform.up = newUpDirection.normalized;
        UpdateIcons();
    }
}
