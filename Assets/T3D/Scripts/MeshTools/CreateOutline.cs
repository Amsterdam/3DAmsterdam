using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

public class CreateOutline : MonoBehaviour
{
    public Material material;


    void Start()
    {
        List<Vector3> verts = new List<Vector3>();
        verts.Add(new Vector3(-2, 0.1f, -4));
        verts.Add(new Vector3(-2, 0.5f, 0));
        verts.Add(new Vector3(1, 0.7f, 0));
        verts.Add(new Vector3(1, 0.3f, -4));
        verts.Add(new Vector3(-2, 0.1f, -4));

        //CreateLine(verts);
        RenderPolygons(verts);
        
    }

    /// <summary>
    /// Linerenderer
    /// </summary>
    void CreateLine(List<Vector3> verts)
    {
        var lr = GetComponent<LineRenderer>();

        lr.alignment = LineAlignment.TransformZ;

        lr.startWidth = 0.2f;
        lr.endWidth = 0.2f;
        lr.material = material;
        lr.positionCount = verts.Count;
        lr.SetPositions(verts.ToArray());

        transform.eulerAngles = new Vector3(0.1f, 0, 0);

    }

    void RenderPolygons(List<Vector3>points)
    {       
        List<int> indices = new List<int>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            indices.Add(i);
            indices.Add(i + 1);
        }

        GameObject newgameobject = new GameObject();       
        MeshFilter filter = newgameobject.AddComponent<MeshFilter>();
        newgameobject.AddComponent<MeshRenderer>().material = material;

        var mesh = new Mesh();
        mesh.vertices = points.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        filter.sharedMesh = mesh;
    }


    /// <summary>
    /// Probuilder
    /// </summary>
    void Create()
    {
        

        var go = new GameObject();
        go.name = "Perceel mesh";        
        ProBuilderMesh m_Mesh = go.gameObject.AddComponent<ProBuilderMesh>();
        go.GetComponent<MeshRenderer>().material = material;

        List<Vector3> verts = new List<Vector3>();

        m_Mesh.CreateShapeFromPolygon(verts.ToArray(), 5, false);


        
    }

}
