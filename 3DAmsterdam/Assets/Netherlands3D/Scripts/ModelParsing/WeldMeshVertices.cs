using System.Collections.Generic;
using UnityEngine;

public class WeldMeshVertices: MonoBehaviour
{
    private List<Mesh> submeshes;
    public float MaxAngle = 5;
    private UnityEngine.Rendering.IndexFormat meshformat;

    /// <summary>
    /// welds mesh vertices where the angle between adjacent triangles is smaller than MaxAngle
    /// </summary>
    /// <param name="oldMesh">original mesh</param>
    /// <returns>mesh with welded vertices</returns>
    public Mesh WeldVertices(Mesh oldMesh)
    {
        meshformat = UnityEngine.Rendering.IndexFormat.UInt32;
        int submeshCount = oldMesh.subMeshCount;
        if (submeshCount ==1)
        {
            Mesh mesh = SubdivideMesh(oldMesh,0);
            return mesh;
        }
        else
        {
            CombineInstance[] ci = new CombineInstance[submeshCount];
            for (int i = 0; i < submeshCount; i++)
            {
                ci[i].mesh = SubdivideMesh(oldMesh, i);
            }
            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = meshformat;
            combinedMesh.CombineMeshes(ci,false, false, false);
            // remove combineInstances
            for (int i = ci.Length - 1; i >= 0; i--)
            {
                Destroy(ci[i].mesh);
            }
            return combinedMesh;
        }


        
    }

    /// <summary>
    /// calculate triangle normal
    /// </summary>
    /// <param name="vert1"></param>
    /// <param name="vert2"></param>
    /// <param name="vert3"></param>
    /// <param name="normalize"></param>
    /// <returns>triangle normal</returns>
    private Vector3 CalculateNormal(Vector3 vert1, Vector3 vert2, Vector3 vert3, bool normalize = true)
    {
        // Calculate the normal of the triangle
        Vector3 p1 = vert2 - vert1;
        Vector3 p2 = vert3 - vert1;
        Vector3 normal = Vector3.Cross(p1, p2);
        if (normalize)
        {
            return normal.normalized;
        }
        return normal;
    }
    
    /// <summary>
    /// divides the mesh in submeshes with triangles with adjoining triangles with similar normals
    /// then combines the submeshes into a single mesh
    /// </summary>
    /// <param name="inputMesh"></param>
    /// <returns></returns>
    private Mesh SubdivideMesh(Mesh inputMesh, int submeshIndex)
    {
        float cosAngleThreshold = Mathf.Cos(MaxAngle * Mathf.Deg2Rad);

        //// turn the mesh inot a mesh with 1 submesh
        //int submeshcount = inputMesh.subMeshCount;
        //if (submeshcount > 1)
        //{
        //    inputMesh.SetTriangles(inputMesh.triangles, 0);
        //    inputMesh.subMeshCount = 1;
        //}

        // get the triangledata
        Vector3[] vertices = inputMesh.vertices;
        int[] inputIndices = inputMesh.GetIndices(submeshIndex);

        //prepare triangles
        List<triangle> unUsedTriangles = new List<triangle>(); // all the trangles that have not been added to a submesh
        Vector3 triangleNormal;
        for (int i = 0; i < inputIndices.Length; i += 3)
        {
            triangleNormal = CalculateNormal(vertices[inputIndices[i]], vertices[inputIndices[i + 1]], vertices[inputIndices[i + 2]]);
            unUsedTriangles.Add(new triangle(vertices[inputIndices[i]], vertices[inputIndices[i + 1]], vertices[inputIndices[i + 2]], triangleNormal));
        }

        List<triangle> usedTriangles = new List<triangle>(); //triangles that are used in the current submesh
        List<triangle> unconnectedTriangles = new List<triangle>(); //triangles that are not connected to this submesh (physically connected, but at an angle) 
        List<triangle> frontlineTriangles = new List<triangle>(); // triangles at the edge of the current submesh
        
        submeshes = new List<Mesh>(); // found submeshes


        while (unUsedTriangles.Count > 0)
        {
            // add first triangle to usedTriangles
            
            frontlineTriangles.Clear();
            frontlineTriangles.Add(unUsedTriangles[0]); // set the first unused triangle as frontlinetriangle
            usedTriangles.Add(unUsedTriangles[0]); // set the same first unused triangle as used triangle
            unUsedTriangles.RemoveAt(0);
            Vector3 planeNormal = usedTriangles[0].normal;
            List<triangle> foundTriangles;

            //find a flat area
            while (frontlineTriangles.Count > 0)
            {
                foundTriangles = FindConnectedTriangles(unUsedTriangles, frontlineTriangles);
                frontlineTriangles = new List<triangle>();
                for (int i = 0; i < foundTriangles.Count; i++)
                {
                    float AngleTreshhold = cosAngleThreshold;
                    if (Vector3.Dot(planeNormal.normalized, foundTriangles[i].normal.normalized) >= AngleTreshhold) // angle is smaller, so include in plane
                    {
                        
                        frontlineTriangles.Add(foundTriangles[i]); // set the triangle to be a frontile triangle for the next iteration
                        usedTriangles.Add(foundTriangles[i]);
                    }
                    else
                    {
                        unconnectedTriangles.Add(foundTriangles[i]);

                    }
                    unUsedTriangles.Remove(foundTriangles[i]);
                }
            }


            // create a mesh with welded vertices form the found triangles
            List<Vector3> childVertices = new List<Vector3>();
            List<int> childIndices = new List<int>();
            triangle currentTriangle;
            for (int i = 0; i < usedTriangles.Count; i++)
                {
                currentTriangle = usedTriangles[i];
                if (childVertices.Contains(currentTriangle.vert1))
                {
                    childIndices.Add(childVertices.IndexOf(currentTriangle.vert1));
                }
                else
                {
                    childIndices.Add(childVertices.Count);
                    childVertices.Add(currentTriangle.vert1);
                }
                if (childVertices.Contains(currentTriangle.vert2))
                {
                    childIndices.Add(childVertices.IndexOf(currentTriangle.vert2));
                }
                else
                {
                    childIndices.Add(childVertices.Count);
                    childVertices.Add(currentTriangle.vert2);
                }
                if (childVertices.Contains(currentTriangle.vert3))
                {
                    childIndices.Add(childVertices.IndexOf(currentTriangle.vert3));
                }
                else
                {
                    childIndices.Add(childVertices.Count);
                    childVertices.Add(currentTriangle.vert3);
                }
            }
            
            Mesh childmesh = new Mesh();
            childmesh.indexFormat = meshformat;
            childmesh.vertices = childVertices.ToArray();
            childmesh.SetIndices(childIndices.ToArray(), MeshTopology.Triangles, 0);
            childmesh.RecalculateNormals();
            // add the new mesh to the list of submeshes    
            submeshes.Add(childmesh);
            usedTriangles.Clear();
            unUsedTriangles.AddRange(unconnectedTriangles);
            unconnectedTriangles.Clear();
        }
       

       // GetComponent<MeshFilter>().mesh = new Mesh();
        Mesh mesh = combineParts();
        return mesh;
    }

    /// <summary>
    /// combines the submeshes into a single mesh
    /// </summary>
    /// <returns></returns>
    private Mesh combineParts()
    {
        CombineInstance[] combine = new CombineInstance[submeshes.Count];

        int i = 0;
        while (i < submeshes.Count)
        {
            combine[i].mesh = submeshes[i];
            i++;
        }
        Mesh mesh = new Mesh();
        mesh.indexFormat = meshformat;
        mesh.CombineMeshes(combine, true, false, false);


        // remove combineInstances
        for ( i = combine.Length - 1; i >= 0; i--)
        {
            Destroy(combine[i].mesh);
        }

        return mesh;
    }

    /// <summary>
    /// find all the triangles that are connected to the frontline
    /// </summary>
    /// <param name="addabletriangles"> all the triangles that might be added</param>
    /// <param name="frontlineTriangles">the triangles that form the frontline</param>
    /// <returns>triangles that are connected to the frontline</returns>
    private List<triangle> FindConnectedTriangles(List<triangle> addabletriangles, List<triangle> frontlineTriangles)
    {
        if (frontlineTriangles.Count == 0)
        {
            return new List<triangle>();
        }
        List<triangle> FoundTriangles = new List<triangle>();
        triangle FoundTriangle;
        int numberofHits;
        for (int i = 0; i < addabletriangles.Count; i++)
        {
            FoundTriangle = addabletriangles[i];

            for (int j = 0; j < frontlineTriangles.Count; j++)
            {
                numberofHits = 0;
                if (FoundTriangle.vertexlist.Contains(frontlineTriangles[j].vert1))
                {
                    numberofHits++;
                }
                if (FoundTriangle.vertexlist.Contains(frontlineTriangles[j].vert2))
                {
                    numberofHits++;
                }
                if (FoundTriangle.vertexlist.Contains(frontlineTriangles[j].vert3))
                {
                    numberofHits++;
                }
                if (numberofHits > 1)
                {
                    FoundTriangles.Add(FoundTriangle);
                    break;
                }
            }
        }
        return FoundTriangles;
    }
}
public class triangle : System.IEquatable<triangle>
{
    public int id;
    public Vector3 vert1;
    public Vector3 vert2;
    public Vector3 vert3;
    public Vector3 normal;
    public List<Vector3> vertexlist;
    public triangle(Vector3 Vert1, Vector3 Vert2, Vector3 Vert3, Vector3 Normal)
    {
        vert1 = Vert1;
        vert2 = Vert2;
        vert3 = Vert3;
        normal = Normal;
        vertexlist = new List<Vector3> { vert1, vert2, vert3 };

    }
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        triangle objAsPart = obj as triangle;
        if (objAsPart == null) return false;
        else return Equals(objAsPart);
    }

    public bool Equals(triangle other)
    {
        if (other == null) return false;
        if (vert1 == other.vert1 && vert2 == other.vert2 && vert3 == other.vert3)
        {
            return true;
        }
        return false;
    }


    public override int GetHashCode()
    {
        int hashCode = -1951484959;
        hashCode = hashCode * -1521134295 + vert1.GetHashCode();
        hashCode = hashCode * -1521134295 + vert2.GetHashCode();
        hashCode = hashCode * -1521134295 + vert3.GetHashCode();
        return hashCode;
    }
}