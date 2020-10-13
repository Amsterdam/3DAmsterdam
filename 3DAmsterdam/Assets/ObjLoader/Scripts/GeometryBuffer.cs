using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GeometryBuffer
{

	readonly List<ObjectData> Objects;
	public List<Vector3> Vertices;
	public List<Vector2> Uvs;
	public List<Vector3> Normals;
	public int UnnamedGroupIndex = 1; // naming index for unnamed group. like "Unnamed-1"

	SubMeshGroupData currentSubMeshGroup;
	ObjectData currentObjectData;

	class ObjectData
	{
		public string Name;
		public Dictionary<string, SubMeshGroupData> SubMeshGroups;
		public List<FaceIndices> AllFaces;
		public int NormalCount;
		public ObjectData()
		{
			SubMeshGroups = new Dictionary<string, SubMeshGroupData>();
			AllFaces = new List<FaceIndices>();
			NormalCount = 0;
		}
	}

	class SubMeshGroupData
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
		var d = new ObjectData();
		d.Name = "default";
		Objects.Add(d);
		currentObjectData = d;

		Vertices = new List<Vector3>();
		Uvs = new List<Vector2>();
		Normals = new List<Vector3>();
	}

	public void AddObject(string name)
	{
		Debug.Log("Adding new object " + name + ". Current is empty: " + IsEmpty);
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
			//This submeshgroup already exists. Select it.
			Debug.Log("Write faces to existing submesh:" + currentSubMeshGroup.Name);
			return true;
		}
		var newGroup = new SubMeshGroupData();
		if (name == null)
		{
			name = "Unnamed-" + UnnamedGroupIndex;
			UnnamedGroupIndex++;
		}
		newGroup.Name = name;
		Debug.Log("Created new submesh to write next faces to:" + newGroup.Name);
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
		Debug.Log("Adding face indice to submesh: " + currentSubMeshGroup.Name);
		currentSubMeshGroup.FaceIndices.Add(f);
		currentObjectData.AllFaces.Add(f);
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
				Debug.Log(od.Name + "/" + subMeshGroupData.Value.Name + " has " + subMeshGroupData.Value.FaceIndices.Count + " faces(s)");
			}
		}
	}

	public int NumberOfObjects { get { return Objects.Count; } }
	public bool IsEmpty { get { return Vertices.Count == 0; } }
	public bool HasUVs { get { return Uvs.Count > 0; } }
	public bool HasNormals { get { return Normals.Count > 0; } }

	// Max Vertices Limit for a given Mesh.
	public static long MaxVerticesLimit = 4294967296;

	public void PopulateMeshes(GameObject[] gameObjects, Dictionary<string, Material> materialDictionary, Material defaultMaterial)
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
			// Debug.Log("PopulateMeshes object name: " + od.Name);

			var allVertices = new Vector3[objectData.AllFaces.Count];
			var allUVs = new Vector2[objectData.AllFaces.Count];
			var allNormals = new Vector3[objectData.AllFaces.Count];

			int k = 0;
			foreach (FaceIndices fi in objectData.AllFaces)
			{
				if (k >= MaxVerticesLimit)
				{
					Debug.LogWarning("Maximum vertex number for a mesh exceeded for object: " + gameObjects[i].name);
					break;
				}
				allVertices[k] = Vertices[fi.vertexIndex];
				if (HasUVs) allUVs[k] = Uvs[fi.vertexUV];
				if (HasNormals && fi.vertexNormal >= 0) allNormals[k] = Normals[fi.vertexNormal];
				k++;
			}

			Mesh mesh = gameObjects[i].GetComponent<MeshFilter>().mesh;
			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //Supports 4 billion verts instead of 65535
			mesh.vertices = allVertices;
			if (HasUVs) mesh.uv = allUVs;
			if (objectHasNormals) mesh.normals = allNormals;

			if (objectData.SubMeshGroups.Count == 1)
			{
				SubMeshGroupData firstSubMeshGroup = objectData.SubMeshGroups.Values.First();
				string matName = firstSubMeshGroup.Name;
				if (materialDictionary.ContainsKey(matName))
				{ 
					Debug.Log("Assigning " + materialDictionary[matName]);
					gameObjects[i].GetComponent<Renderer>().material = materialDictionary[matName];
				}
				else
				{
					gameObjects[i].GetComponent<Renderer>().material = defaultMaterial;
				}

				var triangles = new int[firstSubMeshGroup.FaceIndices.Count];
				for (int j = 0; j < triangles.Length; j++) triangles[j] = j;

				mesh.triangles = triangles;
			}
			else
			{
				int subMeshCount = objectData.SubMeshGroups.Count;
				var materials = new Material[subMeshCount];
				mesh.subMeshCount = subMeshCount;
				int vertIndex = 0;
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
						materials[submeshIndex] = defaultMaterial;
						Debug.LogWarning("PopulateMeshes mat: " + matName + " not found.");
					}

					var triangles = new int[subMesh.Value.FaceIndices.Count];
					int vertIndexSelectionMax = subMesh.Value.FaceIndices.Count + vertIndex;
					int targetIndexSlot = 0;
					for (; vertIndex < vertIndexSelectionMax; vertIndex++, targetIndexSlot++)
					{
						triangles[targetIndexSlot] = vertIndex;
					}
					/*
					var triangles = new int[subMesh.Value.FaceIndices.Count];
					for (int j = 0; j < subMesh.Value.FaceIndices.Count; j++)
					{
						triangles[j] = subMesh.Value.FaceIndices[j].vertexIndex;
					}
					*/
					mesh.SetTriangles(triangles, submeshIndex);
					submeshIndex++;
				}

				gameObjects[i].GetComponent<Renderer>().materials = materials;
			}
			if (!objectHasNormals)
			{
				mesh.RecalculateNormals();
			}
		}
	}
}