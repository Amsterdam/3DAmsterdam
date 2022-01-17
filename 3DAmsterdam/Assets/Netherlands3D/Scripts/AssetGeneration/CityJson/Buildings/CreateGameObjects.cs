#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using TextureCombiner;
using UnityEditor;

namespace Netherlands3D.AssetGeneration.CityJSON
{
	public class CreateGameObjects
	{
		private Material Defaultmaterial;
		public bool useTextures = true;
		public bool singleMeshBuildings = false;
		public bool minimizeMeshes = true;
		public bool createPrefabs = false;
		public bool enableRenderers = false;
		 
		private Vector3Double Origin;
		private Dictionary<string, Material> Materialmapping;
		private bool wgsConvert;
		public void CreateBuildings(List<Building> buildings, Vector3Double origin, Material DefaultMaterial, GameObject parent = null, bool WGSConvert = false, Dictionary<string, Material> materialmapping = null)
		{
			Materialmapping = materialmapping;
			wgsConvert = WGSConvert;
			Defaultmaterial = DefaultMaterial;
			Origin = origin;

			CreateBuildingsSlowly(buildings, parent);
		}

		public void CreateBuildingsSlowly(List<Building> buildings, GameObject parent = null)
		{
			if (minimizeMeshes)
			{
				CreateMultiBuildingMeshes(buildings, parent);
			}
			else if (singleMeshBuildings)
			{
				foreach (Building building in buildings)
				{
					CreateOneMeshBuildingObject(building, parent);
				}
			}
			else
			{
				foreach (Building building in buildings)
				{
					CreateBuilding(building, parent);
				}
			}

			if (createPrefabs)
			{
				string guid = AssetDatabase.CreateFolder("Assets/Prefabs", parent.name);
				//string guid = AssetDatabase.CreateFolder("Assets", "My Folder");
				string prefabfoldername = AssetDatabase.GUIDToAssetPath(guid);
				Debug.Log("parentname: " + parent.name);
				Debug.Log("foldername is: " + prefabfoldername);
				AssetDatabase.Refresh();
				MeshFilter[] meshfilters = parent.GetComponentsInChildren<MeshFilter>();
				int i = 0;
				AssetDatabase.CreateFolder(prefabfoldername, "meshes");
				AssetDatabase.Refresh();
				foreach (MeshFilter mf in meshfilters)
				{
					AssetDatabase.CreateAsset(mf.sharedMesh, prefabfoldername + "/meshes/mesh_" + parent.name + i + ".mesh");
					i += 1;
				}
				MeshRenderer[] meshrenderers = parent.GetComponentsInChildren<MeshRenderer>();
				i = 0;

				AssetDatabase.CreateFolder(prefabfoldername, "textures");
				AssetDatabase.CreateFolder(prefabfoldername, "materials");
				AssetDatabase.Refresh();
				foreach (MeshRenderer mr in meshrenderers)
				{
					Texture2D tex = (Texture2D)mr.sharedMaterial.GetTexture("_MainTex");
					if (tex != null)
					{
						byte[] bytes = tex.EncodeToPNG();
						string filepath;
						filepath = Application.dataPath + "/Prefabs/" + parent.name + "/textures/texture_" + parent.name + i + ".png";
						FileStream stream = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.Write);
						BinaryWriter writer = new BinaryWriter(stream);
						for (int b = 0; b < bytes.Length; b++)
						{
							writer.Write(bytes[b]);
						}
						writer.Close();
						stream.Close();
						AssetDatabase.Refresh();
						Texture2D newTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(prefabfoldername + "/textures/texture_" + parent.name + i + ".png", typeof(Texture2D));
						mr.sharedMaterial.SetTexture("_MainTex", newTexture);
						AssetDatabase.CreateAsset(mr.sharedMaterial, prefabfoldername + "/materials/material_" + parent.name + i + ".mat");
					}
					else
					{
						//AssetDatabase.CreateAsset(mr.material, "Assets/Prefabs/materials/material" + parent.name + i + ".mat");
					}

					i++;
				}
				AssetDatabase.Refresh();

				PrefabUtility.SaveAsPrefabAssetAndConnect(parent, prefabfoldername + "/" + parent.name + ".prefab", InteractionMode.AutomatedAction);

			}
		}

		private void CreateMultiBuildingMeshes(List<Building> buildings, GameObject parent)
		{
			List<TexturedMesh> Allbuildingparts = new List<TexturedMesh>();
			List<TexturedMesh> buildingparts = new List<TexturedMesh>();
			TexturedMesh[] materialparts;

			foreach (Building building in buildings)
			{
				buildingparts = CreateOneMeshBuilding(building);
				foreach (TexturedMesh item in buildingparts)
				{
					Allbuildingparts.Add(item);
				}
			}
			List<TexturedMesh> textured = new List<TexturedMesh>();
			List<TexturedMesh> untextured = new List<TexturedMesh>();
			if (Materialmapping != null)
			{
				materialparts = new TexturedMesh[Materialmapping.Count];
			}
			foreach (TexturedMesh item in Allbuildingparts)
			{
				if (item.TextureName != "" && useTextures == true)
				{
					textured.Add(item);
				}
				else
				{
					untextured.Add(item);
				}
			}

			List<CombineInstance> ci = new List<CombineInstance>();
			if (untextured.Count > 0)
			{
				foreach (TexturedMesh item in untextured)
				{
					CombineInstance cci = new CombineInstance();
					cci.mesh = item.mesh;
					ci.Add(cci);
				}
				GameObject go = new GameObject();
				go.transform.parent = parent.transform;
				Mesh mesh = new Mesh();
				mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
				mesh.CombineMeshes(ci.ToArray(), true, false);
				foreach (var meshHolder in ci)
				{
					GameObject.Destroy(meshHolder.mesh);
				}
				go.AddComponent<MeshFilter>().mesh = mesh;
				go.AddComponent<MeshRenderer>().sharedMaterial = Defaultmaterial;
			}

			if (textured.Count > 0)
			{
				List<TexturedMesh> tm = CombineTextures.MultiCombineTextures(textured);
				foreach (TexturedMesh item in tm)
				{
					GameObject go = new GameObject();
					go.transform.parent = parent.transform;
					Material mat = new Material(Defaultmaterial);
					mat.color = new Color(1, 1, 1);
					mat.SetTexture("_MainTex", item.texture);
					go.AddComponent<MeshFilter>().mesh = item.mesh;
					go.AddComponent<MeshRenderer>().sharedMaterial = mat;
				}
			}
		}

		private void CreateOneMeshBuildingObject(Building building, GameObject parent)
		{
			List<TexturedMesh> buildingparts = new List<TexturedMesh>();
			buildingparts = CreateOneMeshBuilding(building);
			List<TexturedMesh> textured = new List<TexturedMesh>();
			List<TexturedMesh> untextured = new List<TexturedMesh>();
			foreach (TexturedMesh item in buildingparts)
			{
				if (item.TextureName != "" && useTextures == true)
				{
					textured.Add(item);
				}
				else
				{
					untextured.Add(item);
				}
			}
			GameObject go = new GameObject();

			Material mat = new Material(Defaultmaterial);
			go.AddComponent<ObjectProperties>().semantics = building.semantics;
			go.name = go.name = GetBuildingID(building);

			List<CombineInstance> ci = new List<CombineInstance>();
			if (untextured.Count > 0)
			{
				foreach (TexturedMesh item in untextured)
				{
					CombineInstance cci = new CombineInstance();
					cci.mesh = item.mesh;
					ci.Add(cci);
				}
			}
			go.transform.parent = parent.transform;
			if (textured.Count > 0)
			{
				TexturedMesh tm = CombineTextures.CombineMeshes(textured);
				if (tm != null)
				{
					CombineInstance cci = new CombineInstance();
					cci.mesh = tm.mesh;
					ci.Add(cci);
					mat.color = new Color(1, 1, 1);
					mat.SetTexture("_MainTex", tm.texture);
				}
			}
			Mesh mesh = new Mesh();
			mesh.CombineMeshes(ci.ToArray(), true, false);
			foreach(var meshHolder in ci)
			{
				GameObject.Destroy(meshHolder.mesh);
			}

			go.AddComponent<MeshFilter>().sharedMesh = mesh;
			var renderer = go.AddComponent<MeshRenderer>();
			renderer.material = mat;
			renderer.enabled = enableRenderers;
		}

		private void CreateBuilding(Building building, GameObject parent)
		{
			GameObject go = new GameObject();
			go.transform.parent = parent.transform;
			go.AddComponent<ObjectProperties>().semantics = building.semantics;
			go.name = GetBuildingID(building);

			foreach (Surface surf in building.surfaces)
			{
				CreateSurface(surf, go);
			}

			foreach (Building child in building.children)
			{
				CreateBuilding(child, go);
			}
		}

		private string GetBuildingID(Building building)
		{
			foreach (Semantics item in building.semantics)
			{
				if (item.name == "identificatie")
				{
					return item.value.Replace("NL.IMBAG.Pand.","");
				}
			}

			return "No 'identificatie' found";
		}

		private List<TexturedMesh> CreateOneMeshBuilding(Building building, GameObject parent = null)
		{
			List<TexturedMesh> texturedmeshes = new List<TexturedMesh>();
			if (parent != null)
			{
				GameObject go = new GameObject();
				go.transform.parent = parent.transform;
				go.AddComponent<ObjectProperties>().semantics = building.semantics;
				go.name = GetBuildingID(building);
				
			}

			foreach (Surface surf in building.surfaces)
			{
				texturedmeshes.Add(CreateSurface(surf));
			}
			foreach (Building child in building.children)
			{
				List<TexturedMesh> subtextures = CreateOneMeshBuilding(child);
				foreach (TexturedMesh item in subtextures)
				{
					texturedmeshes.Add(item);
				}

			}
			return texturedmeshes;
		}

		private TexturedMesh CreateSurface(Surface surf, GameObject parentGameobject = null)
		{

			Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
			poly.outside = CreateVectorlist(surf.outerRing, Origin);
			if (surf.outerringUVs.Count > 0)
			{

				poly.outsideUVs = surf.outerringUVs;
			};
			if (surf.innerRings.Count > 0)
			{
				foreach (var innerring in surf.innerRings)
				{
					poly.holes.Add(CreateVectorlist(innerring, Origin));
				}
			}
			if (surf.innerringUVs.Count > 0)
			{

				poly.holesUVs = surf.innerringUVs;
			}

			Mesh submesh = Poly2Mesh.CreateMesh(poly);
			if (parentGameobject != null)
			{
				GameObject go = new GameObject();

				for (int i = 0; i < surf.semantics.Count; i++)
				{
					if (surf.semantics[i].name == "type")
					{
						go.name = surf.semantics[i].value;
						i = surf.semantics.Count;
					}
				}


				if (submesh != null)
				{
					go.transform.parent = parentGameobject.transform;
					go.AddComponent<MeshFilter>().sharedMesh = submesh;
					go.AddComponent<MeshRenderer>().sharedMaterial = Defaultmaterial;
					ObjectProperties op = go.AddComponent<ObjectProperties>();
					op.semantics = surf.semantics;
					if (surf.surfacetexture != null && useTextures == true)
					{
						Material mat = new Material(Defaultmaterial);
						mat.color = new Color(1, 1, 1);
						mat.SetTexture("_MainTex", LoadTexture(surf.surfacetexture));
						go.GetComponent<MeshRenderer>().material = mat;
					}
				}
			}
			else
			{
				TexturedMesh texturedmesh = new TexturedMesh();
				for (int i = 0; i < surf.semantics.Count; i++)
				{
					if (surf.semantics[i].name == "name")
					{
						texturedmesh.IdentifierString = surf.semantics[i].value;
					}
				}
				texturedmesh.mesh = submesh;
				if (surf.surfacetexture != null)
				{
					texturedmesh.TextureName = surf.surfacetexture.path;
					texturedmesh.texture = LoadTexture(surf.surfacetexture);
				}

				return texturedmesh;
			}
			return null;
		}

		private Texture2D LoadTexture(Surfacetexture tex)
		{
			Texture2D texture;
			if (tex is null)
			{
				return null;
			}

			string texturepath = tex.path;

			byte[] imagedata = File.ReadAllBytes(texturepath);
			Texture2D testtex = new Texture2D(2, 2);
			testtex.LoadImage(imagedata);


			return testtex;

		}
		private List<Vector3> CreateVectorlist(List<Vector3Double> vectors, Vector3Double origin)
		{
			List<Vector3> output = new List<Vector3>();
			Vector3 vect;
			for (int i = 0; i < vectors.Count; i++)
			{
				vect = new Vector3();
				vect.x = (float)(vectors[i].x - origin.x);
				vect.y = (float)(vectors[i].z - origin.z);
				vect.z = (float)(vectors[i].y - origin.y);
				output.Add(vect);
			}

			output.Reverse();
			return output;
		}
	}
}
#endif