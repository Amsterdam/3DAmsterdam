using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class BinaryMeshConversion : MonoBehaviour
{
    private System.Diagnostics.Stopwatch stopwatch;

    private const int version = 1;

    [SerializeField]
    private string assetBundlePath = "C:/Users/Sam/Desktop/binaryconvert/buildings1.0/";

    [ContextMenu("Convert to binary")]
	private void ConvertToBinary()
	{
        SaveMeshAsBinaryFile(GetComponent<MeshFilter>().mesh,Application.persistentDataPath + "/mesh.bin");
    }

    [ContextMenu("Load from binary")]
    private void LoadFromBinary()
    {
        stopwatch = new System.Diagnostics.Stopwatch();

        var meshFilter = GetComponent<MeshFilter>();
        DestroyImmediate(meshFilter);

        byte[] readBytes = File.ReadAllBytes(Application.persistentDataPath + "/mesh.bin");

        //Time from the moment we have the bytes in memory, to finished displaying on screen
        stopwatch.Start();
        Profiler.BeginSample("readbinarymesh",this);
                
        gameObject.AddComponent<MeshFilter>().mesh = ReadBinaryMesh(readBytes);
        // Code to measure...
        Profiler.EndSample();

        Debug.Log(stopwatch.ElapsedMilliseconds);
        stopwatch.Stop();
        stopwatch.Reset();
    }


	public static void SaveMeshAsBinaryFile(Mesh sourceMesh, string filePath){
        Debug.Log(filePath);
        using (FileStream file = File.Create(filePath))
        {
            using (BinaryWriter writer = new BinaryWriter(file))
            {
                //Version int
                writer.Write(version);

                //Verts
                writer.Write(sourceMesh.vertices.Length);
                foreach (Vector3 vert in sourceMesh.vertices)
                {
                    writer.Write(vert.x);
                    writer.Write(vert.y);
                    writer.Write(vert.z);
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

                //Every triangle list per submesh
                writer.Write(sourceMesh.subMeshCount);
				for (int i = 0; i < sourceMesh.subMeshCount; i++)
				{
                    int[] submeshTriangleList = sourceMesh.GetTriangles(i);
                    writer.Write(submeshTriangleList.Length);
                    writer.Write(sourceMesh.GetSubMesh(i).baseVertex);
                    //var offset = sourceMesh.GetSubMesh(i).baseVertex;
                    foreach (int index in submeshTriangleList)
                    {
                        writer.Write(index);
                    }
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

				for (int i = 0; i < ids.Count; i++)
				{
                    //ID string. string starts with a length
                    //https://docs.microsoft.com/en-us/dotnet/api/system.io.binarywriter.write?view=net-5.0#System_IO_BinaryWriter_Write_System_String_
                    writer.Write(ids[i]);

                    //Check how often this ID index appears in the vectormap (that is the vert indices count of the object)
                    int amountOfInts = vectorMap.Count((vector) => vector == i);
                    writer.Write(amountOfInts);
                }
            }
        }
    }

    public static Mesh ReadBinaryMesh(byte[] fileBytes)
    {
        using (var stream = new MemoryStream(fileBytes))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                var version = reader.ReadInt32();
                //Debug.Log("V: " + version);

                var mesh = new Mesh();
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

                var vertLength = reader.ReadInt32();
                //Debug.Log("Vert length:" + vertLength);
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

                var normalsLength = reader.ReadInt32();
                //Debug.Log("Normals length:" + vertLength);
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
                //Debug.Log("UVs length:" + uvLength);
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

                //Submeshes
                var submeshes = reader.ReadInt32();
                mesh.subMeshCount = submeshes;
                //Debug.Log("Submeshes: " + submeshes);
                for (int i = 0; i < submeshes; i++)
                {
                    //Debug.Log("Submesh: " + i);
                    
                    var trianglesLength = reader.ReadInt32();
                    var baseVertex = reader.ReadInt32();
                    int[] triangles = new int[trianglesLength];
                    //Debug.Log("Triangle length:" + trianglesLength);
                    for (int j = 0; j < trianglesLength; j++)
                    {
                        triangles[j] = reader.ReadInt32();
                    }
                    //mesh.SetIndices(triangles, MeshTopology.Triangles, i);
                    mesh.SetIndices(triangles, MeshTopology.Triangles, i, false, baseVertex);
                }

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
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(filename);
            if (myLoadedAssetBundle == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }
            else if(!filename.Contains("-data"))
            {
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
            if (filename.Contains("-data")) //metadata file found
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