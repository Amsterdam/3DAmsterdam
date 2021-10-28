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

    public void RecalculateScale()
    {
        transform.localScale = CalculateXYScale(leftBound, rightBound, topBound, bottomBound);
    }

    protected static Vector3 CalculateXYScale(Transform left, Transform right, Transform top, Transform bottom)
    {
        float hDist = Vector3.Distance(left.position, right.position);
        float vDist = Vector3.Distance(top.position, bottom.position);

        return new Vector3(hDist, vDist, 1);
    }

    protected Vector3 GetCorner(Transform hBound, Transform vBound)
    {
        var plane = new Plane(-transform.forward, transform.position);

        var projectedHPoint = plane.ClosestPointOnPlane(hBound.position);
        var projectedVPoint = plane.ClosestPointOnPlane(vBound.position);

        float hDist = Vector3.Distance(transform.position, projectedHPoint);
        float vDist = Vector3.Distance(transform.position, projectedVPoint);

        var hDir = (projectedHPoint- transform.position).normalized;
        var vDir = (projectedVPoint - transform.position).normalized;

        return transform.position + hDir * hDist + vDir * vDist;
    }

    //private void OnDrawGizmos()
    //{
    //    foreach (var corner in Polygon)
    //    {
    //        Gizmos.DrawSphere(corner, 0.1f);
    //    }
    //}
}
