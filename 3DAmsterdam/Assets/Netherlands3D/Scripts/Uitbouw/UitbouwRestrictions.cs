using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConvertCoordinates;
using Netherlands3D.T3D.Uitbouw;
using Netherlands3D.Utilities;
using UnityEngine;

public interface UitbouwRestriction
{
    public bool ConformsToRestriction(BuildingMeshGenerator building, PerceelRenderer perceel, Uitbouw uitbouw);
}

public class MonumentRestriction : UitbouwRestriction
{
    public bool ConformsToRestriction(BuildingMeshGenerator building, PerceelRenderer perceel, Uitbouw uitbouw)
    {
        return building.IsMonument;
    }
}

public class DimensionRestriction : UitbouwRestriction
{
    public static float MaxHeight = 3f;
    public static float MaxDepth = 3f;
    public bool ConformsToRestriction(BuildingMeshGenerator building, PerceelRenderer perceel, Uitbouw uitbouw)
    {
        return uitbouw.Height < MaxHeight && uitbouw.Depth < MaxDepth;
    }
}

public class PerceelAreaRestriction : UitbouwRestriction
{
    public static float MaxAreaPercentage { get { return 33f; } }
    public static float MaxAreaFraction { get { return MaxAreaPercentage / 100f; } }

    public bool ConformsToRestriction(BuildingMeshGenerator building, PerceelRenderer perceel, Uitbouw uitbouw)
    {
        var uitbouwArea = uitbouw.Width * uitbouw.Depth;
        //var totalPerceelArea = perceel.Area;
        //var builtArea = building.Area;

        var freeArea = perceel.Area - building.Area;

        var percentage = (uitbouwArea / freeArea) * 100;
        Debug.Log(uitbouwArea + "\t" + freeArea + "\t" + percentage + "%");
        return percentage <= MaxAreaPercentage;
    }
}

public class PerceelBoundsRestriction : UitbouwRestriction
{
    public bool ConformsToRestriction(BuildingMeshGenerator building, PerceelRenderer perceel, Uitbouw uitbouw)
    {
        return IsInPerceel(uitbouw.GetFootprint(), perceel.Perceel, uitbouw.transform.position);
    }

    public static bool IsInPerceel(Vector2[] uitbouwFootprint, List<Vector2[]> perceel, Vector3 uitbouwPositionOffset)
    {
        var q = from i in perceel
                from p in i
                select CoordConvert.RDtoUnity(p) into v3
                select new Vector2(v3.x, v3.z);

        var polyPoints = q.ToArray(); //todo: test for non-contiguous perceels

        foreach (var vert in uitbouwFootprint)
        {
            if (!GeometryCalculator.ContainsPoint(polyPoints, vert + new Vector2(uitbouwPositionOffset.x, uitbouwPositionOffset.z)))
            {
                return false;
            }
        }
        return true;
    }
}

public static class RestrictionChecker
{
    private static UitbouwRestriction[] activeRestrictions = new UitbouwRestriction[]
    {
        new MonumentRestriction(),
        new DimensionRestriction(),
        new PerceelAreaRestriction(),
        new PerceelBoundsRestriction(),
    };
    public static UitbouwRestriction[] ActiveRestrictions => activeRestrictions;

    public static bool ConformsToAllRestrictions(BuildingMeshGenerator building, PerceelRenderer perceel, Uitbouw uitbouw)
    {
        foreach (var restriction in ActiveRestrictions)
        {
            if (!restriction.ConformsToRestriction(building, perceel, uitbouw))
            {
                return false;
            }
        }
        return true;
    }
}
