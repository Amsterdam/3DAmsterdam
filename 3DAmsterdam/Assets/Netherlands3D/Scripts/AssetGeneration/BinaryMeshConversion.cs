using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class BinaryMeshConversion : MonoBehaviour
{
    private const int version = 1;

    [SerializeField]
    private string assetBundlePath = "C:/Users/Sam/Desktop/data/buildings1.0/";

    [ContextMenu("Convert to binary")]
	private void ConvertToBinary()
	{
        SaveMeshAsBinaryFile(GetComponent<MeshFilter>().mesh,Application.persistentDataPath + "/mesh.bin");
    }

    [ContextMenu("Load from binary")]
    private void LoadFromBinary()
    {
        var meshFilter = GetComponent<MeshFilter>();
        DestroyImmediate(meshFilter);

        byte[] readBytes = File.ReadAllBytes(Application.persistentDataPath + "/mesh.bin");
        Profiler.BeginSample("readbinarymesh",this);

		gameObject.AddComponent<MeshFilter>().mesh = ReadBinaryMesh(readBytes, out int[] materialIndices);
	}

	public static void SaveMeshAsBinaryFile(Mesh sourceMesh, string filePath){
        Debug.Log(filePath);
        using (FileStream file = File.Create(filePath))
        {
            using (BinaryWriter writer = new BinaryWriter(file))
            {
                //Version int
                writer.Write(version);
                writer.Write(sourceMesh.vertices.Length);
                writer.Write(sourceMesh.normals.Length);
                writer.Write(sourceMesh.uv.Length);
                writer.Write(sourceMesh.triangles.Length);
                writer.Write(sourceMesh.subMeshCount);

                //Verts
                var vertices = sourceMesh.vertices;
                foreach (Vector3 vert in sourceMesh.vertices)
                {
                    writer.Write(vert.x);
                    writer.Write(vert.y);
                    writer.Write(vert.z);
                }

                //Normals
                var normals = sourceMesh.normals;
                foreach (Vector3 normal in normals)
                {
                    writer.Write(normal.x);
                    writer.Write(normal.y);
                    writer.Write(normal.z);
                }

                //UV
                var uvs = sourceMesh.uv;
                foreach (Vector2 uv in uvs)
                {
                    writer.Write(uv.x);
                    writer.Write(uv.y);
                }

                //Indices
                var indices = sourceMesh.triangles;
                foreach(var index in indices)
                {
                    writer.Write(index);
                }

                //Every triangle list per submesh
				for (int i = 0; i < sourceMesh.subMeshCount; i++)
				{
                    writer.Write(i); //submesh/material index
                    writer.Write(sourceMesh.GetSubMesh(i).indexStart);
                    writer.Write(sourceMesh.GetSubMesh(i).indexCount);
                    writer.Write(sourceMesh.GetSubMesh(i).firstVertex);
                    writer.Write(sourceMesh.GetSubMesh(i).vertexCount);
                }            
            }
        }
    }

    public static void SaveMetadataAsBinaryFile(ObjectMappingClass sourceObjectMapping, string filePath)
    {
        Debug.Log(filePath);
        using (FileStream file = File.Create(filePath))
        {
            var ids = sourceObjectMapping.ids;
            //var uvs = sourceObjectMapping.uvs;
            var vectorMap = sourceObjectMapping.vectorMap;

            using (BinaryWriter writer = new BinaryWriter(file))
            {
                //Version int
                writer.Write(version);

                //Subobject count
                writer.Write(ids.Count);

                //All subobject id's, and their indices
				for (int i = 0; i < ids.Count; i++)
				{
                    //ID string. string starts with a length:
                    //https://docs.microsoft.com/en-us/dotnet/api/system.io.binarywriter.write?view=net-5.0#System_IO_BinaryWriter_Write_System_String_
                    writer.Write(ids[i]);

                    int firstIndex = 0; //We do not need these in Unity atm.
                    writer.Write(firstIndex);
                    int indexCount = 0; //We do not need these in Unity atm.
                    writer.Write(indexCount);

                    int firstVertex = 0;
					//Check how often this ID index appears in the vectormap (that is the vert indices count of the object)
                    int vertexCount = vectorMap.Count((vector) => vector == i);
					for (int j = 0; j < vectorMap.Count; j++)
					{
                        //get the first vert position we encounter in the vectormap
                        if (vectorMap[j] == i)
                        {
                            firstVertex = j;
                            break;
                        }
                    }
                                        
                    writer.Write(firstVertex);
                    writer.Write(vertexCount);

                    writer.Write(0);
                }
            }
        }
    }

    public static Mesh ReadBinaryMesh(byte[] fileBytes, out int[] submeshMaterialIndices)
    {
        using (var stream = new MemoryStream(fileBytes))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                var version = reader.ReadInt32();
                var vertexCount = reader.ReadInt32();
                var normalsCount = reader.ReadInt32();
                var uvsCount = reader.ReadInt32();
                var indicesCount = reader.ReadInt32();
                var submeshCount = reader.ReadInt32();

                var mesh = new Mesh();

                Vector3[] vertices = new Vector3[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                {
                    Vector3 vertex = new Vector3(
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle()
                     );
                    vertices[i] = vertex;
                }
                mesh.vertices = vertices;

                Vector3[] normals = new Vector3[normalsCount];
                for (int i = 0; i < normalsCount; i++)
                {
                    Vector3 normal = new Vector3(
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle()
                     );
                    normals[i] = normal;
                }
                mesh.normals = normals;

                Vector2[] uvs = new Vector2[uvsCount];
                for (int i = 0; i < uvsCount; i++)
                {
                    Vector2 uv = new Vector2(
                        reader.ReadSingle(),
                        reader.ReadSingle()
                     );
                    uvs[i] = uv;
                }
                mesh.uv = uvs;

                int[] indices = new int[indicesCount];
                for (int i = 0; i < indicesCount; i++)
                {
                    var index = reader.ReadInt32();
                    indices[i] = index;
                }

                mesh.SetIndexBufferParams(indicesCount, UnityEngine.Rendering.IndexFormat.UInt32);
                mesh.SetIndexBufferData(indices, 0, 0, indicesCount);

                mesh.subMeshCount = submeshCount;
                int[] materialIndices = new int[submeshCount];

                for (int i = 0; i < submeshCount; i++)
                {
                    var subMeshID = reader.ReadInt32();
                    materialIndices[i] = subMeshID;

                    var subMeshFirstIndex = reader.ReadInt32();
                    var subMeshIndexCount = reader.ReadInt32();
                    var submeshFirstVertex = reader.ReadInt32();
                    var submeshVertexCount = reader.ReadInt32();

                    var subMeshDescriptor = new UnityEngine.Rendering.SubMeshDescriptor();
                    subMeshDescriptor.baseVertex = 0;
                    subMeshDescriptor.firstVertex = submeshFirstVertex;
                    subMeshDescriptor.vertexCount = submeshVertexCount;

                    subMeshDescriptor.indexStart = subMeshFirstIndex;
                    subMeshDescriptor.indexCount = subMeshIndexCount;

                    mesh.SetSubMesh(subMeshID, subMeshDescriptor);
                }
                submeshMaterialIndices = materialIndices;

                return mesh;
            }
        }
    }

    [ContextMenu("Convert AssetBundles to binary files")]
    private void ConvertFromAssetBundleMeshesToBinary()
    {
        var files = Directory.GetFiles(assetBundlePath);

		for (int i = 0; i < files.Length; i++)
		{
            var filename = files[i];
            if (!filename.Contains("-data") && !filename.Contains(".bin"))
            {
                var myLoadedAssetBundle = AssetBundle.LoadFromFile(filename);
                if (myLoadedAssetBundle == null)
                {
                    Debug.Log("Failed to load AssetBundle!");
                    continue;
                }
                try
                {
                    var mesh = myLoadedAssetBundle.LoadAllAssets<Mesh>()[0];
                    SaveMeshAsBinaryFile(mesh, filename + ".bin");
                    myLoadedAssetBundle.Unload(true);
                }
                catch (Exception)
                {
                    Debug.Log("No mesh in AssetBundle");
                    myLoadedAssetBundle.Unload(true);
                }
            }
        }
    }

    [ContextMenu("Convert AssetBundles metadata to binary files")]
    private void ConvertFromAssetBundleMetaDataToBinary()
    {
        var files = Directory.GetFiles(assetBundlePath);
        for (int i = 0; i < files.Length; i++)
        {
            var filename = files[i];
            if (filename.Contains("-data") && !filename.Contains(".bin")) //metadata file found
            {
                var myLoadedAssetBundle = AssetBundle.LoadFromFile(filename);
                if (myLoadedAssetBundle == null)
                {
                    Debug.Log("Failed to load AssetBundle!");
                    continue;
                }
                try
                {
                    var data = myLoadedAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];
                    SaveMetadataAsBinaryFile(data, filename + ".bin");
                    myLoadedAssetBundle.Unload(true);
                }
                catch (Exception)
                {
                    Debug.Log("No mesh in AssetBundle");
                    myLoadedAssetBundle.Unload(true);
                }
            }
        }
    }
}
