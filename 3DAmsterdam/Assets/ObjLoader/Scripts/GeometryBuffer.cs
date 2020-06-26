using UnityEngine;
using System.Collections.Generic;

public class GeometryBuffer {

	readonly List<ObjectData> Objects;
	public List<Vector3> Vertices;
	public List<Vector2> Uvs;
	public List<Vector3> Normals;
	public int UnnamedGroupIndex = 1; // naming index for unnamed group. like "Unnamed-1"
	
	ObjectData current;
	class ObjectData {
		public string Name;
		public List<GroupData> Groups;
		public List<FaceIndices> AllFaces;
		public int NormalCount;
		public ObjectData() {
			Groups = new List<GroupData>();
			AllFaces = new List<FaceIndices>();
			NormalCount = 0;
		}
	}
	
	GroupData curgr;
	class GroupData {
		public string Name;
		public string MaterialName;
		public List<FaceIndices> Faces;
		public GroupData() {
			Faces = new List<FaceIndices>();
		}
		public bool IsEmpty { get { return Faces.Count == 0; } }
	}
	
	public GeometryBuffer() {
		Objects = new List<ObjectData>();
		var d = new ObjectData();
		d.Name = "default";
		Objects.Add(d);
		current = d;
		
		var g = new GroupData();
		g.Name = "default";
		d.Groups.Add(g);
		curgr = g;
		
		Vertices = new List<Vector3>();
		Uvs = new List<Vector2>();
		Normals = new List<Vector3>();
	}
	
	public void PushObject(string name) {
		Debug.Log("Adding new object " + name + ". Current is empty: " + IsEmpty);
		if (IsEmpty) Objects.Remove(current);

		// Object Data
		var n = new ObjectData();
		n.Name = name;
		Objects.Add(n);

		// Group Data
		var g = new GroupData();
		g.Name = "default";
		n.Groups.Add(g);
		
		curgr = g;
		current = n;
	}
	
	public void PushGroup(string name) {
		if (curgr.IsEmpty) current.Groups.Remove(curgr);
		var g = new GroupData();
		if (name == null) {
			name = "Unnamed-" + UnnamedGroupIndex;
			UnnamedGroupIndex++;
		}
		g.Name = name;
		current.Groups.Add(g);
		curgr = g;
	}
	
	public void PushMaterialName(string name) {
		// Debug.Log("Pushing new material " + name + " with curgr.empty =" + curgr.IsEmpty);
		if (!curgr.IsEmpty) PushGroup(name);
		if (curgr.Name == "default") curgr.Name = name;
		curgr.MaterialName = name;
	}
	
	public void PushVertex(Vector3 v) {
		Vertices.Add(v);
	}
	
	public void PushUV(Vector2 v) {
		Uvs.Add(v);
	}
	
	public void PushNormal(Vector3 v) {
		Normals.Add(v);
	}
	
	public void PushFace(FaceIndices f) {
		curgr.Faces.Add(f);
		current.AllFaces.Add(f);
		if (f.Vn >= 0) {
			current.NormalCount++;
		}
	}
	
	public void Trace() {
		Debug.Log("OBJ has " + Objects.Count + " object(s)");
		Debug.Log("OBJ has " + Vertices.Count + " vertice(s)");
		Debug.Log("OBJ has " + Uvs.Count + " uv(s)");
		Debug.Log("OBJ has " + Normals.Count + " normal(s)");
		foreach(ObjectData od in Objects) {
			Debug.Log(od.Name + " has " + od.Groups.Count + " group(s)");
			foreach(GroupData gd in od.Groups) {
				Debug.Log(od.Name + "/" + gd.Name + " has " + gd.Faces.Count + " faces(s)");
			}
		}
	}
	
	public int NumberOfObjects { get { return Objects.Count; } }	
	public bool IsEmpty { get { return Vertices.Count == 0; } }
	public bool HasUVs { get { return Uvs.Count > 0; } }
	public bool HasNormals { get { return Normals.Count > 0; } }

	// Max Vertices Limit for a given Mesh.
	public static int MaxVerticesLimit = 64999;
	
	public void PopulateMeshes(GameObject[] gs, Dictionary<string, Material> mats) {
		// Check is valid file.
		if (gs.Length != NumberOfObjects) {
			Debug.LogError ("Failed - OBJ File may be corrupt");
			return;
		}

		// Debug.Log("PopulateMeshes GameObjects count:" + gs.Length);
		for (int i = 0; i < gs.Length; i++) {
			ObjectData od = Objects[i];
			bool objectHasNormals = (HasNormals && od.NormalCount > 0);
			
			if (od.Name != "default") gs[i].name = od.Name;
			// Debug.Log("PopulateMeshes object name: " + od.Name);

			var tvertices = new Vector3[od.AllFaces.Count];
			var tuvs = new Vector2[od.AllFaces.Count];
			var tnormals = new Vector3[od.AllFaces.Count];
		
			int k = 0;
			foreach(FaceIndices fi in od.AllFaces) {
				if (k >= MaxVerticesLimit) {
					Debug.LogWarning("Maximum vertex number for a mesh exceeded for object: " + gs[i].name);
					break;
				}
				tvertices[k] = Vertices[fi.Vi];
				if(HasUVs) tuvs[k] = Uvs[fi.Vu];
				if(HasNormals && fi.Vn >= 0) tnormals[k] = Normals[fi.Vn];
				k++;
			}
		
			Mesh m = (gs[i].GetComponent(typeof(MeshFilter)) as MeshFilter).mesh;
			m.vertices = tvertices;
			if(HasUVs) m.uv = tuvs;
			if(objectHasNormals) m.normals = tnormals;
			
			if(od.Groups.Count == 1) {
				// Debug.Log("PopulateMeshes only one group: " + od.Groups[0].Name);
				GroupData gd = od.Groups[0];

				string matName = gd.MaterialName ?? "default";
				if (mats.ContainsKey(matName)) {
					gs[i].GetComponent<Renderer>().material = mats[matName];
					// Debug.Log("PopulateMeshes mat: " + matName + " set.");
				}
				else {
					Debug.LogWarning("PopulateMeshes mat: " + matName + " not found.");
				}

				var triangles = new int[gd.Faces.Count];
				for(int j = 0; j < triangles.Length; j++) triangles[j] = j;
				
				m.triangles = triangles;
			} else {
				int gl = od.Groups.Count;
				var materials = new Material[gl];
				m.subMeshCount = gl;
				int c = 0;
				
				Debug.Log("PopulateMeshes group count: " + gl);
				for(int j = 0; j < gl; j++) {
					string matName = od.Groups [j].MaterialName ?? "default";
					if (mats.ContainsKey(matName)) {
						materials[j] = mats[matName];
						Debug.Log("PopulateMeshes mat: " + matName + " set.");
					}
					else {
						Debug.LogWarning("PopulateMeshes mat: " + matName + " not found.");
					}
					
					var triangles = new int[od.Groups[j].Faces.Count];
					int l = od.Groups[j].Faces.Count + c;
					int s = 0;
					for(; c < l; c++, s++) triangles[s] = c;
					m.SetTriangles(triangles, j);
				}
				
				gs[i].GetComponent<Renderer>().materials = materials;
			}
			if (!objectHasNormals) {
				m.RecalculateNormals();
			}
		}
	}
}