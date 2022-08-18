using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using Netherlands3D.T3D.Uitbouw;
using T3D.Uitbouw;

public class MeshToCityJSONConverter : CityObject
{
    private Vector3[] verts;
    private int[] tris;

    public override CitySurface[] GetSurfaces()
    {
        meshFilter = GetComponent<MeshFilter>();

        var mesh = meshFilter.mesh;
        verts = mesh.vertices;
        tris = mesh.triangles;

        List<CitySurface> citySurfaces = new List<CitySurface>();

        for (int i = 0; i < tris.Length; i += 3)
        {
            var tri = new int[] { tris[i], tris[i + 2], tris[i + 1] }; //reverse the order for the CityJson to work
            var triVerts = new Vector3[] { transform.position + verts[tri[0]], transform.position + verts[tri[1]], transform.position + verts[tri[2]] };
            var polygon = new CityPolygon(triVerts, tri);
            citySurfaces.Add(new CitySurface(polygon));
        }

        return citySurfaces.ToArray();
    }
}
