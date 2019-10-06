using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectMapping : MonoBehaviour
{
    public Dictionary<string, float> Objectenlijst = new Dictionary<string, float>();
    private MeshRenderer mr;
    public Material HighlightMaterial;
    public Material DefaultMaterial;

    private SubObject Totaalmesh;
    private SubObject Tempmesh;

    private List<SubObject> HighlightObjecten = new List<SubObject>();

    public void SetMesh(Mesh totaalmesh)
    {
        Totaalmesh = new SubObject();
        Totaalmesh.verts = totaalmesh.vertices;
        Totaalmesh.triangles = totaalmesh.triangles;
        Totaalmesh.uv2 = totaalmesh.uv2;
        Tempmesh = new SubObject();
        Tempmesh.verts = totaalmesh.vertices;
        Tempmesh.triangles = totaalmesh.triangles;
        Tempmesh.uv2 = totaalmesh.uv2;
    }

    public void SetHighlight(List<string> HighlightObjectenstring)
    {

        HighlightObjecten = new List<SubObject>();
        Totaalmesh.triangles = Tempmesh.triangles;
        for (int i = 0; i < 1; i++)
        {
            if (Objectenlijst.ContainsKey(HighlightObjectenstring[i]))
            {

                    ExtractSubObject(HighlightObjectenstring[i]);

            }
        }
        SetupMesh();
    }

    void ReturnSubobject(SubObject SO)
    {
        List<int> verts = new List<int>();
        for (int i = 0; i < Totaalmesh.triangles.Length; i++)
        {
            verts.Add(Totaalmesh.triangles[i]);
        }
        for (int i = 0; i < SO.triangles.Length; i++)
        {
            verts.Add(SO.triangles[i]);
        }
        Totaalmesh.triangles = verts.ToArray();

    }
    void ExtractSubObject(string ObjectID)
    {
        
        SubObject SO = new SubObject(); //nieuw subobject
        SubObject TO = new SubObject(); // nieuw totaalobject zonder subobject
        SO.ObjectID = ObjectID;
        
        float uv2Waarde = Objectenlijst[ObjectID];
        
        //triangles splitsen
        List<int> trianglesSO = new List<int>();
        List<int> trianglesTO = new List<int>();
        for (int TriangledIndex = 0; TriangledIndex < (Tempmesh.triangles.Length); TriangledIndex+=3)
        {
            
            int vertIndex = Tempmesh.triangles[TriangledIndex];
            if (Mathf.Abs(Tempmesh.uv2[vertIndex].x - uv2Waarde) < 0.5)
            {
                
                trianglesSO.Add(Tempmesh.triangles[TriangledIndex]);
                trianglesSO.Add(Tempmesh.triangles[TriangledIndex+1]);
                trianglesSO.Add(Tempmesh.triangles[TriangledIndex+2]);
            }
            else
            {
                trianglesTO.Add(Tempmesh.triangles[TriangledIndex]);
                trianglesTO.Add(Tempmesh.triangles[TriangledIndex + 1]);
                trianglesTO.Add(Tempmesh.triangles[TriangledIndex + 2]);
            }
        }
        SO.triangles = trianglesSO.ToArray();
        TO.triangles = trianglesTO.ToArray();
        HighlightObjecten.Add(SO);
        Totaalmesh.triangles = TO.triangles;
    }

    void SetupMesh()
    {
        Mesh ms = new Mesh();
        List<Material> mats = new List<Material>();
        mats.Add(DefaultMaterial);
        for (int i = 0; i < HighlightObjecten.Count; i++)
        {
            mats.Add(HighlightMaterial);
        }
        transform.GetComponent<MeshRenderer>().sharedMaterials = mats.ToArray();
        
        ms.subMeshCount = HighlightObjecten.Count+1;
        ms.vertices = Tempmesh.verts;
        ms.uv2 = Tempmesh.uv2;
        //ms.SetTriangles( Totaalmesh.triangles,0);
        ms.SetTriangles(Totaalmesh.triangles,  0);
        for (int i = 0; i < HighlightObjecten.Count; i++)
        {
  
         ms.SetTriangles(HighlightObjecten[i].triangles, i+1);
            
        }
        ms.RecalculateNormals();
        transform.GetComponent<MeshFilter>().sharedMesh = ms;
        
        
    }
}
class SubObject
{
    public string ObjectID;
    public Vector3[] verts;
    public int[] triangles;
    public Vector2[] uv2;
}
