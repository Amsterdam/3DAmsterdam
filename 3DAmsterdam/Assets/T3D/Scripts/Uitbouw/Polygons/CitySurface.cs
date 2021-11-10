using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using SimpleJSON;

[RequireComponent(typeof(CityPolygon))]
public class CitySurface : MonoBehaviour
{
    public List<CityPolygon> Polygons = new List<CityPolygon>();

    public CityPolygon SolidSurfacePolygon => Polygons[0];
    public CityPolygon[] HolePolygons => Polygons.Skip(1).ToArray(); //skip the first element (the solid part)

    private void Awake()
    {
        Polygons.Add(GetComponent<CityPolygon>());
    }

    public void TryAddHole(CityPolygon hole)
    {
        if(!Polygons.Contains(hole))
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
