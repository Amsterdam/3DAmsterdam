using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SunDragWheel : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [SerializeField]
    private RectTransform moonIcon;
    [SerializeField]
    private RectTransform sunIcon;

    [SerializeField]
    private float rotationSnapDegrees;
    private float beginDragAngle = 0.0f;

    public delegate void ChangedSunWheel(float rotation);
    public ChangedSunWheel changedDirection;

    private void Awake()
    {
        changedDirection += PickedColorMessage;
    }

    private void PickedColorMessage(float rotation)
    {
        Debug.Log("Changed sun wheel to " + rotation);
    }

    private void Update()
    {
        //Keep icons straight
        moonIcon.rotation = Quaternion.identity;
        sunIcon.rotation = Quaternion.identity;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Get starting rotation, so we rotate from this point
        Vector3 relativePosition = transform.position;
        relativePosition = Input.mousePosition - relativePosition;
        beginDragAngle = Mathf.Atan2(relativePosition.y, relativePosition.x) * Mathf.Rad2Deg;
        beginDragAngle -= Mathf.Atan2(transform.right.y, transform.right.x) * Mathf.Rad2Deg;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Rotate wheel according to relative mouse position
        Vector3 relativePosition = transform.position;
        relativePosition = Input.mousePosition - relativePosition;
        float ang = Mathf.Atan2(relativePosition.y, relativePosition.x) * Mathf.Rad2Deg - beginDragAngle;
        ang -= (ang % (360.0f / rotationSnapDegrees));
        transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
    }
}
