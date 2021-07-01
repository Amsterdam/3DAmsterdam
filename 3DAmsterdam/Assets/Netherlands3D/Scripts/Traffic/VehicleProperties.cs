using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleProperties : MonoBehaviour
{
    public GameObject frontLeftWheel;
    private Vector3 leftPoint;
    public GameObject frontRightWheel;
    private Vector3 rightPoint;
    public GameObject backLeftWheel;
    public GameObject backRightWheel;
    private Vector3 backPoint;
    public GameObject[] otherWheels;

    private RaycastHit hit;
    private float wheelHeight;
    private void Start()
    {
        wheelHeight = frontLeftWheel.transform.GetComponent<Renderer>().bounds.size.y;
    }

    public Quaternion GetNewRotation(Quaternion currentRotation)
    {
        leftPoint = GetCoordinatesUnderWheel(frontLeftWheel.transform.position);
        rightPoint = GetCoordinatesUnderWheel(frontRightWheel.transform.position);

        Vector3 combinedPosition = (backLeftWheel.transform.position + backRightWheel.transform.position) / 2;
        backPoint = GetCoordinatesUnderWheel(combinedPosition);

        Plane plane = new Plane(leftPoint, rightPoint, backPoint);
        return Quaternion.FromToRotation(transform.up, plane.normal) * currentRotation;
    }

    private Vector3 GetCoordinatesUnderWheel(Vector3 position)
    {
        if (Physics.Raycast(position, -Vector3.up, out hit, 10f))
        {
            return hit.point;
        }
        else
        {
            Vector3 tempPosition = new Vector3(position.x, position.y - wheelHeight / 2, position.z);
            return tempPosition;
        }
    }
}
