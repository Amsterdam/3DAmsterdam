using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BinaryMeshConversion : MonoBehaviour
{
	private void Start()
	{
        SaveMeshAsBinaryFile(GetComponent<MeshFilter>().mesh,Application.persistentDataPath + "/mesh.bin");

        ReadBinaryMesh(Application.persistentDataPath + "/mesh.bin");

    }

	public void SaveMeshAsBinaryFile(Mesh sourceMesh, string filePath){
        Debug.Log(filePath);
        using (FileStream file = File.Create(filePath))
        {
            using (BinaryWriter writer = new BinaryWriter(file))
            {
                //Verts
                writer.Write(sourceMesh.vertices.Length);
                foreach (Vector3 vert in sourceMesh.vertices)
                {
                    writer.Write(vert.x);
                    writer.Write(vert.y);
                    writer.Write(vert.z);
                }

                //Triangles
                writer.Write(sourceMesh.triangles.Length);
                foreach (int index in sourceMesh.triangles)
                {
                    writer.Write(index);
                }
            }
        }
    }

    public void ReadBinaryMesh(string filePath){
        var mesh = new Mesh();
        using (FileStream file = File.OpenRead(filePath))
        {
            using (BinaryReader reader = new BinaryReader(file))
            {
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
                    Debug.Log(vertex);
                    vertices[i] = vertex;  
                }

                var trianglesLength = reader.ReadInt32();
                int[] triangles = new int[trianglesLength];
                Debug.Log("Triangle length:" + trianglesLength);
				for (int i = 0; i < trianglesLength; i++)
				{
                    triangles[i] = reader.ReadInt32();
				}

                mesh.vertices = vertices;
                mesh.triangles = triangles;

                this.GetComponent<MeshFilter>().mesh = mesh;
            }
        }
    }
}
