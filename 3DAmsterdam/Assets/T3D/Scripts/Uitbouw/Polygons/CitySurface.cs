using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Assertions;
using T3D.Uitbouw;

public enum SemanticType
{
    Null = 0,

    RoofSurface = 1000,
    GroundSurface = 1001,
    WallSurface = 1002,
    ClosureSurface = 1003,
    OuterCeilingSurface = 1004,
    OuterFloorSurface = 1005,
    Window = 1006,
    Door = 1007,

    WaterSurface = 1130,
    WaterGroundSurface = 1131,
    WaterClosureSurface = 1132,

    TrafficArea = 1080,
    AuxiliaryTrafficArea = 1081,
}

//[RequireComponent(typeof(CityPolygon))]
public class CitySurface
{
    public List<CityPolygon> Polygons = new List<CityPolygon>();

    public virtual CityPolygon SolidSurfacePolygon => Polygons[0];
    public virtual CityPolygon[] HolePolygons => Polygons.Skip(1).ToArray();

    public SemanticType SurfaceType { get; set; }

    public CitySurface(CityPolygon solidSurfacePolygon, SemanticType type = SemanticType.Null)
    {
        SurfaceType = type;
        Polygons.Add(solidSurfacePolygon);
    }

    public static bool IsValidSemanticType(T3D.Uitbouw.CityObjectType parent, SemanticType type)
    {
        if (type == SemanticType.Null) //no semantic type is always allowed
            return true;

        var testInt = (int)type / 10;
        var parentInt = (int)parent / 10;

        if (testInt == parentInt) //default test
        {
            return true;
        }
        if (testInt == parentInt - 100) // child test
        {
            return true;
        }

        if (testInt == 108 && (parent == CityObjectType.Road || parent == CityObjectType.Railway || parent == CityObjectType.TransportSquare)) //custom test
        {
            return true;
        }
        return false;
    }

    public void TryAddHole(CityPolygon hole)
    {
        if (!Polygons.Contains(hole))
            Polygons.Add(hole);
    }

    public void TryRemoveHole(CityPolygon hole)
    {
        if (Polygons.Contains(hole))
            Polygons.Remove(hole);
    }

    public JSONArray GetJSONPolygons()
    {
        var surfaceArray = new JSONArray(); //defines the entire surface with holes

        // the following line and loop could be replaced by 1 loop through all the polygons of the surface, but separating them makes it clearer how the structure of the array works

        // add surface
        surfaceArray.Add(SolidSurfacePolygon.GetJSONPolygon());
        // add holes
        var holes = HolePolygons;
        for (int j = 0; j < holes.Length; j++)
        {
            surfaceArray.Add(holes[j].GetJSONPolygon());
        }
        return surfaceArray;
    }
}
