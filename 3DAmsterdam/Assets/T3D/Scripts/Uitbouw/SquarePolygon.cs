using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquarePolygon : CityPolygon
{
    [SerializeField]
    protected Transform leftBound;
    [SerializeField]
    protected Transform rightBound;
    [SerializeField]
    protected Transform topBound;
    [SerializeField]
    protected Transform bottomBound;

    public override Vector3[] Polygon
    {
        get
        {
            return new Vector3[] {

                GetCorner(leftBound, topBound),
                GetCorner(rightBound, topBound),
                GetCorner(leftBound, bottomBound),
                GetCorner(rightBound, bottomBound),
            };
        }
    }

    protected Vector3 GetCorner(Transform hBound, Transform vBound)
    {
        var plane = new Plane(-transform.forward, transform.position);

        var projectedHPoint = plane.ClosestPointOnPlane(hBound.position);
        var projectedVPoint = plane.ClosestPointOnPlane(vBound.position);

        float hDist = Vector3.Distance(transform.position, projectedHPoint);
        float vDist = Vector3.Distance(transform.position, projectedVPoint);

        return transform.position - hBound.forward * hDist - vBound.forward * vDist;
    }
}
