using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BinaryMeshConversion : MonoBehaviour
{
	private void Start()
	{
        SaveMeshAsBinaryFile(GetComponent<MeshFilter>().mesh,Application.persistentDataPath + "/mesh.bin");

        this.GetComponent<MeshFilter>().mesh = ReadBinaryMesh(Application.persistentDataPath + "/mesh.bin");
    }

	public void SaveMeshAsBinaryFile(Mesh sourceMesh, string filePath){
        Debug.Log(filePath);
        using (FileStream file = File.Create(filePath))
        {
            using (BinaryWriter writer = new BinaryWriter(file))
            {
                //Version
                writer.Write(1);

                //Verts
                writer.Write(sourceMesh.vertices.Length);
                foreach (Vector3 vert in sourceMesh.vertices)
                {
                    writer.Write(vert.x);
                    writer.Write(vert.y);
                    writer.Write(vert.z);
                }

                //Every triangle list per submesh
                writer.Write(sourceMesh.subMeshCount);
				for (int i = 0; i < sourceMesh.subMeshCount; i++)
				{
                    int[] submeshTriangleList = sourceMesh.GetTriangles(i);
                    writer.Write(submeshTriangleList.Length);
                    foreach (int index in submeshTriangleList)
                    {
                        writer.Write(index);
                    }
                }            

                //Normals
                writer.Write(sourceMesh.normals.Length);
                foreach (Vector3 normal in sourceMesh.normals)
                {
                    writer.Write(normal.x);
                    writer.Write(normal.y);
                    writer.Write(normal.z);
                }

                //UV
                writer.Write(sourceMesh.uv.Length);
                foreach (Vector2 uv in sourceMesh.uv)
                {
                    writer.Write(uv.x);
                    writer.Write(uv.y);
                }
            }
        }
    }

    public Mesh ReadBinaryMesh(string filePath){
        using (FileStream file = File.OpenRead(filePath))
        {
            using (BinaryReader reader = new BinaryReader(file))
            {
                var version = reader.ReadInt32();
                Debug.Log("V: " + version);

                var mesh = new Mesh();
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;

                var vertLength = reader.ReadInt32();
                Debug.Log("Vert length:" + vertLength);
                Vector3[] vertices = new Vector3[vertLength];
				for (int i = 0; i < vertLength; i++)
				{
                    Vector3 vertex = new Vector3(
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle()
                     );
                    vertices[i] = vertex;  
                }
                mesh.vertices = vertices;

                //Submeshes
                var submeshes = reader.ReadInt32();
                Debug.Log("Submeshes: " + submeshes);
                for (int i = 0; i < submeshes; i++)
                {
                    Debug.Log("Submesh: " + i);
                    var trianglesLength = reader.ReadInt32();
                    int[] triangles = new int[trianglesLength];
                    Debug.Log("Triangle length:" + trianglesLength);
                    for (int j = 0; j < trianglesLength; j++)
                    {
                        triangles[j] = reader.ReadInt32();
                    }
                    mesh.SetIndices(triangles, MeshTopology.Triangles, i);
                }                
                

                var normalsLength = reader.ReadInt32();
                Debug.Log("Normals length:" + vertLength);
                Vector3[] normals = new Vector3[normalsLength];
                for (int i = 0; i < normalsLength; i++)
                {
                    Vector3 normal = new Vector3(
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle()
                     );
                    normals[i] = normal;
                }
                mesh.normals = normals;

                var uvLength = reader.ReadInt32();
                Debug.Log("UVs length:" + uvLength);
                Vector2[] uvs = new Vector2[uvLength];
                for (int i = 0; i < uvLength; i++)
                {
                    Vector2 uv = new Vector2(
                        reader.ReadSingle(),
                        reader.ReadSingle()
                     );
                    uvs[i] = uv;
                }
                mesh.uv = uvs;

                return mesh;
            }
        }
    }
}
