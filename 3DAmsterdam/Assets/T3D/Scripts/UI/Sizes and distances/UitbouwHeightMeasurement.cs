using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Netherlands3D.T3D;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public class UitbouwHeightMeasurement : DistanceMeasurement
{
    private UitbouwBase uitbouw;

    protected override void Awake()
    {
        base.Awake();
        uitbouw = GetComponent<UitbouwBase>();
    }

    protected override void DrawLines()
    {
        TryDrawLine(0, uitbouw.TopCenter, new Ray(uitbouw.TopCenter, Vector3.up));
        TryDrawLine(1, uitbouw.BottomCenter, new Ray(uitbouw.BottomCenter, -Vector3.up));
    }

    protected override void Measuring_DistanceInputOverride(BuildingMeasuring source, Vector3 direction, float delta)
    {
        uitbouw.GetComponent<UitbouwMovement>().HeightOffset += delta;
    }

    private bool TryDrawLine(int lineIndex, Vector3 point, Ray ray)
    {
        bool cornerFound = GetNextRoofEdgePlane(ray, out _, out var intersectionPoint);
        lines[lineIndex].gameObject.SetActive(cornerFound);
        if (cornerFound)
        {
            lines[lineIndex].SetLinePosition(point, intersectionPoint);
        }

        return cornerFound;
    }

    private bool GetNextRoofEdgePlane(Ray ray, out Plane plane, out Vector3 intersectionPoint)
    {
        var building = RestrictionChecker.ActiveBuilding;

        bool intersect = false;
        plane = new Plane();
        intersectionPoint = new Vector3();
        float previousEnter = Mathf.Infinity;

        foreach(var roofEdge in building.RoofEdgePlanes)
        {
            var cast = roofEdge.Raycast(ray, out var enter);
            if (cast)
            {
                intersect = true;
                if (enter < previousEnter)
                {
                    previousEnter = enter;
                    plane = roofEdge;
                    intersectionPoint = ray.origin + ray.direction * enter;
                }
            }
        }
        return intersect;
    }
}
