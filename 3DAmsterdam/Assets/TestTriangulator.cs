using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CityGML;

public class TestTriangulator : MonoBehaviour
{
    public string coordlist;
    public Vector3Double origin;
    public Material standardmaterial;

    public LineRenderer lr;
    // Start is called before the first frame update
    void Start()
    {
        origin = new Vector3Double(25497000,6677000,0);
        List<Vector3Double> coords =  ReadLinearRing(coordlist,origin);
        List<Vector3> verts = CreateVectorlist(coords);
        lr.positionCount = verts.Count;
        lr.SetPositions(verts.ToArray());

        //PolygonTriangulator.polygon poly = new PolygonTriangulator.polygon();
        Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();

        poly.outside = verts;
        //poly.SetOuterRing(coords);

        Mesh submesh = Poly2Mesh.CreateMesh(poly);
        //Mesh submesh = poly.createmesh( true);
        GameObject go = new GameObject();
        go.AddComponent<MeshFilter>().mesh = submesh;
        go.AddComponent<MeshRenderer>().material = standardmaterial;

    }

    private List<Vector3Double> ReadLinearRing(string posliststring, Vector3Double origin)
    {
        List<Vector3Double> poscoords = new List<Vector3Double>();
        posliststring = posliststring.Trim(' ');
        string[] poslist = posliststring.Split(' ');
        Vector3Double coord;
        for (int i = 0; i < poslist.Length-1; i += 3)
        {
            coord = new Vector3Double();
            coord.x = double.Parse(poslist[i], System.Globalization.CultureInfo.InvariantCulture)-origin.x;
            coord.y = double.Parse(poslist[i + 1], System.Globalization.CultureInfo.InvariantCulture)-origin.y;
            coord.z = double.Parse(poslist[i + 2], System.Globalization.CultureInfo.InvariantCulture)-origin.z;
            poscoords.Add(coord);
        }

        return poscoords;
    }
    private List<Vector3> CreateVectorlist(List<Vector3Double> vectors)
    {
        List<Vector3> output = new List<Vector3>();
        Vector3 vect;
        for (int i = 0; i < vectors.Count; i++)
        {
            vect = new Vector3();
            vect.x = (float)(vectors[i].x);
            vect.y = (float)(vectors[i].z);
            vect.z = (float)(vectors[i].y);
            output.Add(vect);
        }

        output.Reverse();
        return output;
    }
}
