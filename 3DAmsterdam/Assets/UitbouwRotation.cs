using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using UnityEngine;

public class UitbouwRotation : MonoBehaviour
{
    private Vector3 previousIntersectionPoint;
    private Vector3 previousOrigin;

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButton(0))
        {
            Rotate();
        }
    }

    private void Rotate()
    {
        var deltaAngle = CalculateDeltaAngle();
        print("deltaangle:" + deltaAngle);
        foreach (Transform t in transform)
        {
            t.Rotate(transform.up, deltaAngle); //rotate children because snapping occurs on this Transform
        }
    }

    GameObject test;
    GameObject test2;
    private void Awake()
    {
        test = new GameObject();
        test2 = new GameObject();
    }

    private float CalculateDeltaAngle()
    {
        var newOrigin = transform.position;
        var contactPlane = new Plane(transform.up, newOrigin);
        var ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);

        if (contactPlane.Raycast(ray, out float enter))
        {
            var newIntersectionPoint = ray.origin + (ray.direction * enter);
            print(previousIntersectionPoint + "\t" + newIntersectionPoint);

            test.transform.position = newIntersectionPoint;
            test2.transform.position = previousIntersectionPoint;

            var previousDir = (previousIntersectionPoint - previousOrigin).normalized;
            var newDir = (newIntersectionPoint - newOrigin).normalized;

            var angle = Vector3.SignedAngle(previousDir, newDir, transform.up);
            
            previousOrigin = newOrigin;
            previousIntersectionPoint = newIntersectionPoint;

            return angle;
        }
        else
        {
            previousIntersectionPoint = Vector3.zero;
        }
        test2.transform.position = previousIntersectionPoint;

        return 0;
    }
}
