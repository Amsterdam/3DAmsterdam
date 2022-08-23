using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Assertions;
using T3D.Uitbouw;
using System;

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
    private List<CitySurface> semanticChildren = new List<CitySurface>();
    private CitySurface semanticParent;

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
        surfaceArray.Add(SolidSurfacePolygon.GetJSONPolygon(false));
        // add holes
        var holes = HolePolygons;
        for (int j = 0; j < holes.Length; j++)
        {
            surfaceArray.Add(holes[j].GetJSONPolygon(true));
        }
        return surfaceArray;
    }

    public JSONNode GetSemanticObject(CitySurface[] allGeometrySurfaces)
    {
        var node = new JSONObject();
        node["type"] = SurfaceType.ToString();
        //node["name"] = name;

        if (semanticParent != null)
            node["parent"] = GetParentIndex(allGeometrySurfaces);

        if (semanticChildren.Count > 0)
        {
            var childrenNode = new JSONArray();
            var childIndices = GetChildIndices(allGeometrySurfaces);
            foreach (var c in childIndices)
            {
                childrenNode.Add(c);
            }
            node["children"] = childrenNode;
        }
        return node;
    }

    public void SetParent(CitySurface newParent)
    {
        if (semanticParent != null)
            semanticParent.RemoveChild(this);

        semanticParent = newParent;

        if(semanticParent != null)
            newParent.AddChild(this);
    }

    private void AddChild(CitySurface child)
    {
        Assert.IsFalse(semanticChildren.Contains(child));
        semanticChildren.Add(child);
    }

    private void RemoveChild(CitySurface child)
    {
        semanticChildren.Remove(child);
    }

    private int GetParentIndex(CitySurface[] surfaces)
    {
        return Array.IndexOf(surfaces, semanticParent);
    }

    private int[] GetChildIndices(CitySurface[] surfaces)
    {
        var array = new int[semanticChildren.Count];
        for (int i = 0; i < semanticChildren.Count; i++)
        {
            array[i] = Array.IndexOf(surfaces, semanticChildren[i]);
        }
        return array;
    }
}
