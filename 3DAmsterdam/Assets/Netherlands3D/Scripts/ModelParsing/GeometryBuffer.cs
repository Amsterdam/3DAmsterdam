using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;
using Netherlands3D.AssetGeneration;

public class GeometryBuffer
{

	public List<ObjectData> Objects;
	public List<Vector3> Vertices;
	public List<Vector2> Uvs;
	public List<Vector3> Normals;
	public int UnnamedGroupIndex = 1; // naming index for unnamed group. like "Unnamed-1"
	public bool flipTriangleDirection = false;

	SubMeshGroupData currentSubMeshGroup;
	public ObjectData currentObjectData;
	public struct  FaceIndices
	{
		public int vertexIndex;
		public int vertexUV;
		public int vertexNormal;
	}
	public class ObjectData
	{
		public string Name;
		public Dictionary<string, SubMeshGroupData> SubMeshGroups;
		public List<FaceIndices> AllFacesIndices;
		public int NormalCount;
		public ObjectData()
		{
			SubMeshGroups = new Dictionary<string, SubMeshGroupData>();
			AllFacesIndices = new List<FaceIndices>();
			NormalCount = 0;
		}
	}

	public class SubMeshGroupData
	{
		public string Name;
		public List<FaceIndices> FaceIndices;
		public SubMeshGroupData()
		{
			FaceIndices = new List<FaceIndices>();
		}
		public bool IsEmpty { get { return FaceIndices.Count == 0; } }
	}

	public GeometryBuffer()
	{
		Objects = new List<ObjectData>();
		var defaultObject = new ObjectData();
		defaultObject.Name = "default";
		Objects.Add(defaultObject);
		currentObjectData = defaultObject;

		Vertices = new List<Vector3>();
		Uvs = new List<Vector2>();
		Normals = new List<Vector3>();
	}

	public void AddObject(string name)
	{
		if (IsEmpty) Objects.Remove(currentObjectData);

		// Object Data
		var newObjectData = new ObjectData();
		newObjectData.Name = name;
		Objects.Add(newObjectData);

		// Group Data
		AddSubMeshGroup("default");

		currentObjectData = newObjectData;
	}

	public bool AddSubMeshGroup(string name)
	{
		if (currentObjectData.SubMeshGroups.TryGetValue(name, out currentSubMeshGroup))
		{
			return true;
		}
		var newGroup = new SubMeshGroupData();
		if (name == null)
		{
			name = "Unnamed-" + UnnamedGroupIndex;
			UnnamedGroupIndex++;
		}
		newGroup.Name = name;
		currentObjectData.SubMeshGroups.Add(newGroup.Name, newGroup);
		currentSubMeshGroup = newGroup;

		return false;
	}

	public void PushVertex(Vector3 v)
	{
		Vertices.Add(v);
	}

	public void PushUV(Vector2 v)
	{
		Uvs.Add(v);
	}

	public void PushNormal(Vector3 v)
	{
		Normals.Add(v);
	}

	public void PushFace(FaceIndices f)
	{
		currentSubMeshGroup.FaceIndices.Add(f);
		currentObjectData.AllFacesIndices.Add(f);
		if (f.vertexNormal >= 0)
		{
			currentObjectData.NormalCount++;
		}
	}

	public void Trace()
	{
		Debug.Log("OBJ has " + Objects.Count + " object(s)");
		Debug.Log("OBJ has " + Vertices.Count + " vertice(s)");
		Debug.Log("OBJ has " + Uvs.Count + " uv(s)");
		Debug.Log("OBJ has " + Normals.Count + " normal(s)");
		foreach (ObjectData od in Objects)
		{
			Debug.Log(od.Name + " has " + od.SubMeshGroups.Count + " group(s)");
			foreach (KeyValuePair<string,SubMeshGroupData> subMeshGroupData in od.SubMeshGroups)
			{
				Debug.Log(od.Name + "/" + subMeshGroupData.Value.Name + " has " + subMeshGroupData.Value.FaceIndices.Count/3 + " tri's");
			}
		}
	}

	public int NumberOfObjects { get { return Objects.Count; } }
	public bool IsEmpty { get { return Vertices.Count == 0; } }
	public bool HasUVs { get { return Uvs.Count > 0; } }
	public bool HasNormals { get { return Normals.Count > 0; } }

	// Max Vertices Limit for a given Mesh.
	public static long MaxVerticesLimit = 4294967296;

	public void PopulateMeshes(GameObject[] gameObjects, Dictionary<string, Material> materialDictionary, Material defaultMaterial, ObjLoad objLoad)
	{
		// Check is valid file.
		if (gameObjects.Length != NumberOfObjects)
		{
			Debug.LogError("Failed - OBJ File may be corrupt");
			return;
		}

		for (int i = 0; i < gameObjects.Length; i++)
		{
			ObjectData objectData = Objects[i];
			bool objectHasNormals = (HasNormals && objectData.NormalCount > 0);

			if (objectData.Name != "default") gameObjects[i].name = objectData.Name;

			//Some OBJ's will reuse verts, uvs or normals.
			//Our faces are in the lead. Lets make copies and rewrite ID's so our 
			//Unity arrays can all be the same length.
			var allVertices = new Vector3[objectData.AllFacesIndices.Count];
			var allUVs = new Vector3[objectData.AllFacesIndices.Count];
			var allNormals = new Vector3[objectData.AllFacesIndices.Count];

			for (int j = 0; j < objectData.AllFacesIndices.Count; j++)
			{
				allVertices[j] = Vertices[objectData.AllFacesIndices[j].vertexIndex];

				if (HasUVs)
					allUVs[j] = Uvs[objectData.AllFacesIndices[j].vertexUV];

				if (HasNormals)
					allNormals[j] = Normals[objectData.AllFacesIndices[j].vertexNormal];

				//set new unique id's
				var faceIndices = objectData.AllFacesIndices[j];
				faceIndices.vertexIndex = j;
				faceIndices.vertexUV = j;
				faceIndices.vertexNormal = j;
			}

			//If the bounds of this object fall outside the RD bounds, skip making the object
			if (objLoad.IgnoreObjectsOutsideOfBounds)
			{
				//if one vert of model is within bounds, continue. else skip creating mesh/building
				if (!TileCombineUtility.IsAnyVertexWithinBounds(allVertices, objLoad.BottomLeftBounds, objLoad.TopRightBounds))
				{
					MonoBehaviour.Destroy(gameObjects[i]);
					gameObjects[i] = null;
					Debug.Log("Skip object. Outside given bounds.");
					continue;
				}
			}

			Mesh mesh = CreateMesh(gameObjects, materialDictionary, defaultMaterial, i, objectData, allVertices, allUVs, allNormals);
			mesh.Optimize();
			if (!objectHasNormals)
				mesh.RecalculateNormals();
		}
	}

	private Mesh CreateMesh(GameObject[] gameObjects, Dictionary<string, Material> materialDictionary, Material defaultMaterial, int i, ObjectData objectData, Vector3[] allVertices, Vector3[] allUVs, Vector3[] allNormals)
	{
		Mesh mesh = gameObjects[i].GetComponent<MeshFilter>().mesh;
		mesh.indexFormat = (allVertices.Length > 65536) ? IndexFormat.UInt32 : IndexFormat.UInt32; //Supports 4 billion verts for larger models
		mesh.vertices = allVertices;
		if (HasUVs)
			mesh.SetUVs(0, allUVs);
		if (HasNormals)
			mesh.SetNormals(allNormals);

		if (objectData.SubMeshGroups.Count == 1)
		{
			SubMeshGroupData firstSubMeshGroup = objectData.SubMeshGroups.Values.First();
			string matName = firstSubMeshGroup.Name;
			if (materialDictionary.ContainsKey(matName))
			{
				gameObjects[i].GetComponent<Renderer>().material = materialDictionary[matName];
			}
			else
			{
				gameObjects[i].GetComponent<Renderer>().material = new Material(defaultMaterial);
				gameObjects[i].GetComponent<Renderer>().material.name = firstSubMeshGroup.Name;
			}

			var triangles = new int[firstSubMeshGroup.FaceIndices.Count];
			for (int j = 0; j < triangles.Length; j++)
			{
				triangles[j] = firstSubMeshGroup.FaceIndices[j].vertexIndex;
			}

			if (flipTriangleDirection)
			{
				mesh.triangles = triangles.Reverse().ToArray();
			}
			else
			{
				mesh.triangles = triangles;
			}
		}
		else
		{
			int subMeshCount = objectData.SubMeshGroups.Count;
			var materials = new Material[subMeshCount];
			mesh.subMeshCount = subMeshCount;

			int submeshIndex = 0;
			Debug.Log("PopulateMeshes group count: " + subMeshCount);
			foreach (KeyValuePair<string, SubMeshGroupData> subMesh in objectData.SubMeshGroups)
			{
				string matName = subMesh.Value.Name;
				if (materialDictionary.ContainsKey(matName))
				{
					materials[submeshIndex] = materialDictionary[matName];
					Debug.Log("PopulateMeshes mat: " + matName + " set.");
				}
				else
				{
					materials[submeshIndex] = new Material(defaultMaterial);
					materials[submeshIndex].name = matName;
					Debug.LogWarning("PopulateMeshes mat: " + matName + " not found.");
				}

				var triangles = new int[subMesh.Value.FaceIndices.Count];
				for (int v = 0; v < subMesh.Value.FaceIndices.Count; v++)
				{
					triangles[v] = subMesh.Value.FaceIndices[v].vertexIndex;
				}

				if (flipTriangleDirection)
				{
					mesh.SetTriangles(triangles.Reverse().ToArray(), submeshIndex);
				}
				else
				{
					mesh.SetTriangles(triangles, submeshIndex);
				}
				submeshIndex++;
			}

			gameObjects[i].GetComponent<Renderer>().materials = materials;
		}

		return mesh;
	}
}