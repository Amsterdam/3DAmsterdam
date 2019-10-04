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

    private List<SubObject> HighlightObjecten = new List<SubObject>();

    public void SetMesh(Mesh totaalmesh)
    {
        Totaalmesh = new SubObject();
        Totaalmesh.verts = totaalmesh.vertices;
        Totaalmesh.triangles = totaalmesh.triangles;
        Totaalmesh.uv2 = totaalmesh.uv2;
    }

    public void SetHighlight(List<string> HighlightObjectenstring)
    {

        for (int i = 0; i < HighlightObjectenstring.Count; i++)
        {
            if (Objectenlijst.ContainsKey(HighlightObjectenstring[i]))
            {
                bool IsalApart = false;
                for (int j = 0; i < HighlightObjecten.Count; i++)
                {
                    if(HighlightObjecten[j].ObjectID == HighlightObjectenstring[i]) { IsalApart = true; }
                }
                if (IsalApart == false)
                {
                    ExtractSubObject(HighlightObjectenstring[i]);
                    SetupMesh();
                }
            }
        }
    }
    void ExtractSubObject(string ObjectID)
    {
        SubObject SO = new SubObject(); //nieuw subobject
        SubObject TO = new SubObject(); // nieuw totaalobject zonder subobject
        SO.ObjectID = ObjectID;
        Dictionary<int, int> indexmappingSO = new Dictionary<int, int>();
        Dictionary<int, int> indexmappingTO = new Dictionary<int, int>();
        List<Vector3> VertsSO = new List<Vector3>();
        List<Vector3> VertsTO = new List<Vector3>();
        List<Vector2> uv2SO = new List<Vector2>();
        List<Vector2> uv2TO = new List<Vector2>();
        float uv2Waarde = Objectenlijst[ObjectID];
        Vector2[] TotaalmeshUVs = Totaalmesh.uv2;

        //// verts splitsen in behorend bij suboject en behorend bij Totaalobject
        for (int uvnr = 0; uvnr < TotaalmeshUVs.Length; uvnr++)
        {
            if (TotaalmeshUVs[uvnr].x == uv2Waarde)
            {
                VertsSO.Add(Totaalmesh.verts[uvnr]);
                indexmappingSO.Add(uvnr, VertsSO.Count - 1);
                uv2SO.Add(TotaalmeshUVs[uvnr]);
            }
            else
            {
                VertsTO.Add(Totaalmesh.verts[uvnr]);
                indexmappingTO.Add(uvnr, VertsTO.Count - 1);
                uv2TO.Add(TotaalmeshUVs[uvnr]);
            }
        }
        SO.verts = VertsSO.ToArray();
        TO.verts = VertsTO.ToArray();
        SO.uv2 = uv2SO.ToArray();
        TO.uv2 = uv2TO.ToArray();
        //triangles splitsen en verwijzing naar vertices updaten
        List<int> trianglesSO = new List<int>();
        List<int> trianglesTO = new List<int>();
        for (int TriangledIndex = 0; TriangledIndex < Totaalmesh.triangles.Length/3; TriangledIndex+=3)
        {
            
            int vertIndex = Totaalmesh.triangles[TriangledIndex];
            if (Totaalmesh.uv2[vertIndex].x==uv2Waarde)
            {
                
                trianglesSO.Add(indexmappingSO[Totaalmesh.triangles[TriangledIndex]]);
                trianglesSO.Add(indexmappingSO[Totaalmesh.triangles[TriangledIndex+1]]);
                trianglesSO.Add(indexmappingSO[Totaalmesh.triangles[TriangledIndex+2]]);
            }
            else
            {
                trianglesTO.Add(indexmappingTO[Totaalmesh.triangles[TriangledIndex]]);
                trianglesTO.Add(indexmappingTO[Totaalmesh.triangles[TriangledIndex + 1]]);
                trianglesTO.Add(indexmappingTO[Totaalmesh.triangles[TriangledIndex + 2]]);
            }
        }
        SO.triangles = trianglesSO.ToArray();
        TO.triangles = trianglesTO.ToArray();
        HighlightObjecten.Add(SO);
        Totaalmesh = TO;
    }

    void SetupMesh()
    {
        Mesh ms = transform.GetComponent<MeshFilter>().sharedMesh;
        List<Material> mats = new List<Material>();
        mats.Add(DefaultMaterial);
        ms.Clear();
        ms.subMeshCount = HighlightObjecten.Count+1;
        ms.vertices = Totaalmesh.verts;
        ms.uv2 = Totaalmesh.uv2;
        //ms.SetTriangles( Totaalmesh.triangles,0);
        ms.SetIndices(Totaalmesh.triangles, MeshTopology.Triangles, 0);
        for (int i = 0; i < HighlightObjecten.Count; i++)
        {
            
            mats.Add(HighlightMaterial);
            ms.SetIndices(HighlightObjecten[i].triangles, MeshTopology.Triangles, i);
            //ms.SetUVs(1, HighlightObjecten[i].uv2);
            ms.SetTriangles(HighlightObjecten[i].triangles, i+1,false,0);
            
        }
        transform.GetComponent<MeshFilter>().sharedMesh = ms;
        transform.GetComponent<MeshRenderer>().sharedMaterials = mats.ToArray();
        
    }
}
class SubObject
{
    public string ObjectID;
    public Vector3[] verts;
    public int[] triangles;
    public Vector2[] uv2;
}
