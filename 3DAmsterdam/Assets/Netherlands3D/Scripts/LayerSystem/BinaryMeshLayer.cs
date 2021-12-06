using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using UnityEngine.Networking;
using System;
using System.Linq;
using UnityEngine.Rendering;
using System.IO;

namespace Netherlands3D.LayerSystem
{
	public class BinaryMeshLayer : Layer
	{
		//public Material DefaultMaterial;
		public List<Material> DefaultMaterialList = new List<Material>();
		public bool createMeshcollider = false;
		public bool addHighlightuvs = false;
		public ShadowCastingMode tileShadowCastingMode = ShadowCastingMode.On;

		public override void HandleTile(TileChange tileChange, System.Action<TileChange> callback = null)
		{
			TileAction action = tileChange.action;
			var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
			switch (action)
			{
				case TileAction.Create:
					Tile newTile = CreateNewTile(tileKey);
					tiles.Add(tileKey, newTile);
					break;
				case TileAction.Upgrade:
					tiles[tileKey].LOD++;
					break;
				case TileAction.Downgrade:
					tiles[tileKey].LOD--;
					break;
				case TileAction.Remove:
					InteruptRunningProcesses(tileKey);
					RemoveGameObjectFromTile(tileKey);
					tiles.Remove(tileKey);
					callback(tileChange);
					return;
				default:
					break;
			}
			tiles[tileKey].runningCoroutine = StartCoroutine(DownloadBinaryTile(tileChange, callback));
		}

		private Tile CreateNewTile(Vector2Int tileKey)
		{
			Tile tile = new Tile();
			tile.LOD = 0;
			tile.tileKey = tileKey;
			tile.layer = transform.gameObject.GetComponent<Layer>();
			tile.gameObject = new GameObject();
			tile.gameObject.transform.parent = transform.gameObject.transform;
			tile.gameObject.layer = tile.gameObject.transform.parent.gameObject.layer;
			tile.gameObject.transform.position = CoordConvert.RDtoUnity(tileKey);

			return tile;
		}
		private void RemoveGameObjectFromTile(Vector2Int tileKey)
		{
			if (tiles.ContainsKey(tileKey))
			{

				Tile tile = tiles[tileKey];
				if (tile == null)
				{
					return;
				}
				if (tile.gameObject == null)
				{
					return;
				}
				MeshFilter mf = tile.gameObject.GetComponent<MeshFilter>();
				if (mf != null)
				{
					DestroyImmediate(tile.gameObject.GetComponent<MeshFilter>().sharedMesh, true);
				}
				Destroy(tiles[tileKey].gameObject);

			}
		}
		private IEnumerator DownloadBinaryTile(TileChange tileChange, System.Action<TileChange> callback = null)
		{
			var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
			int lod = tiles[tileKey].LOD;
			string url = Config.activeConfiguration.webserverRootPath + Datasets[lod].path;
			if (Datasets[lod].path.StartsWith("https://") || Datasets[lod].path.StartsWith("file://"))
			{
				url = Datasets[lod].path;
			}

			url = url.ReplaceXY(tileChange.X, tileChange.Y);
			var webRequest = UnityWebRequest.Get(url);
			tiles[tileKey].runningWebRequest = webRequest;
			yield return webRequest.SendWebRequest();

			if (!tiles.ContainsKey(tileKey)) yield break;

			tiles[tileKey].runningWebRequest = null;

			if (webRequest.isNetworkError || webRequest.isHttpError)
			{
				RemoveGameObjectFromTile(tileKey);
				callback(tileChange);
			}
			else
			{
				var binaryMeshData = webRequest.downloadHandler.data;

				//AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(webRequest);
				//tiles[tileKey].assetBundle = assetBundle;
				yield return new WaitUntil(() => pauseLoading == false);
				GameObject newGameobject = CreateNewGameObject(binaryMeshData, tileChange);
				if (newGameobject != null)
				{
					if (TileHasHighlight(tileChange))
					{
						yield return UpdateObjectIDMapping(tileChange, newGameobject, callback);
					}
					else
					{
						RemoveGameObjectFromTile(tileKey);
						tiles[tileKey].gameObject = newGameobject;
						callback(tileChange);
					}
				}
				else
				{

					callback(tileChange);
				}
			}
		}
		public void EnableShadows(bool enabled)
		{
			tileShadowCastingMode = (enabled) ? ShadowCastingMode.On : ShadowCastingMode.Off;

			MeshRenderer[] existingTiles = GetComponentsInChildren<MeshRenderer>();
			foreach (var renderer in existingTiles)
			{
				renderer.shadowCastingMode = tileShadowCastingMode;
			}
		}

		private bool TileHasHighlight(TileChange tileChange)
		{
			Tile tile = tiles[new Vector2Int(tileChange.X, tileChange.Y)];
			if (tile.gameObject == null)
			{
				return false;
			}
			if (tile.gameObject.GetComponent<ObjectData>() == null)
			{
				return false;
			}
			if (tile.gameObject.GetComponent<ObjectData>().highlightIDs.Count + tile.gameObject.GetComponent<ObjectData>().hideIDs.Count == 0)
			{
				return false;
			}

			return true;
		}

		private IEnumerator UpdateObjectIDMapping(TileChange tileChange, GameObject newGameobject, System.Action<TileChange> callback = null)
		{
			Tile tile = tiles[new Vector2Int(tileChange.X, tileChange.Y)];
			ObjectData oldObjectMapping = tile.gameObject.GetComponent<ObjectData>();
			GameObject newTile = newGameobject;
			string name = newTile.GetComponent<MeshFilter>().mesh.name;
			Debug.Log(name);
			string dataName = name.Replace(" Instance", "");
			dataName = dataName.Replace("mesh", "building");
			dataName = dataName.Replace("-", "_") + "-data";
			string dataURL = $"{Config.activeConfiguration.webserverRootPath}{Config.activeConfiguration.buildingsMetaDataPath}{dataName}";
			Debug.Log(dataURL);

			ObjectMappingClass data;
			using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
			{
				yield return uwr.SendWebRequest();

				if (uwr.isNetworkError || uwr.isHttpError)
				{
					callback(tileChange);
				}
				else
				{
					ObjectData objectMapping = newTile.AddComponent<ObjectData>();
					AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);

					yield return new WaitForEndOfFrame();
					data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];
					yield return new WaitForEndOfFrame();

					objectMapping.highlightIDs = oldObjectMapping.highlightIDs;
					objectMapping.hideIDs = oldObjectMapping.hideIDs;
					objectMapping.ids = data.ids;
					objectMapping.uvs = data.uvs;
					objectMapping.vectorMap = data.vectorMap;
					objectMapping.mesh = newTile.GetComponent<MeshFilter>().sharedMesh;
					objectMapping.ApplyDataToIDsTexture();
					newAssetBundle.Unload(true);

					yield return new WaitForEndOfFrame();
				}
			}
			yield return new WaitUntil(() => pauseLoading == false);
			RemoveGameObjectFromTile(tile.tileKey);
			tiles[tile.tileKey].gameObject = newGameobject;

			yield return null;
			callback(tileChange);

		}

		Mesh[] meshesInBinaryData = new Mesh[0];
		GameObject container;
		Mesh mesh;
		MeshRenderer meshRenderer;
		int[] submeshIndices;
		Vector2[] uvs;
		Vector2 defaultUV = new Vector2(0.33f, 0.6f);
		private GameObject CreateNewGameObject(byte[] binaryMeshData, TileChange tileChange)
		{
			container = new GameObject();

			container.name = tileChange.X.ToString() + "-" + tileChange.Y.ToString();
			container.transform.parent = transform.gameObject.transform;
			container.layer = container.transform.parent.gameObject.layer;
			container.transform.position = CoordConvert.RDtoUnity(new Vector2(tileChange.X + (tileSize / 2), tileChange.Y + (tileSize / 2)));

			container.SetActive(isEnabled);
			
			try
			{
				LoadMeshFromBinaryData(binaryMeshData, out mesh, out submeshIndices);
			}
			catch (Exception)
			{
				Destroy(container);
				return null;
			}
			mesh.RecalculateNormals();
			int count = mesh.vertexCount;

			// creating the UV-s runtime takes a lot of time and causes the garbage-collector to kick in.
			// uv's should be built in in to the meshes in the assetbundles.
			if (addHighlightuvs)
			{
				uvs = new Vector2[count];
				for (int i = 0; i < count; i++)
				{
					uvs[i] = (defaultUV);
				}
				mesh.uv2 = uvs;
			}

			container.AddComponent<MeshFilter>().sharedMesh = mesh;

			meshRenderer = container.AddComponent<MeshRenderer>();
			List<Material> materialList = new List<Material>();
            for (int i = 0; i < submeshIndices.Length; i++)
            {
				materialList.Add(DefaultMaterialList[submeshIndices[i]]);
            }
			meshRenderer.sharedMaterials = materialList.ToArray();
			meshRenderer.shadowCastingMode = tileShadowCastingMode;

			if (createMeshcollider)
			{
				container.AddComponent<MeshCollider>().sharedMesh = mesh;
			}



			return container;
		}

		private void LoadMeshFromBinaryData(byte[] binaryData, out Mesh mesh, out int[] submeshIndices)
		{
			using (var stream = new MemoryStream(binaryData))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					Mesh outputMesh = new Mesh();

					var version = reader.ReadInt32();
					var vertexcount = reader.ReadInt32();
					Vector3[] vertices = new Vector3[vertexcount];
					var normalscount = reader.ReadInt32();
					Vector3[] normals = new Vector3[normalscount];
					var indicescount = reader.ReadInt32();
					int[] indices = new int[indicescount];
					var submeshcount = reader.ReadInt32();

					for (int i = 0; i < vertexcount; i++)
					{
						vertices[i] = (new Vector3(
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle()
						 ));
					}
					outputMesh.vertices = vertices;
					for (int i = 0; i < normalscount; i++)
					{
						normals[i] = (new Vector3(
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle()
						 ));
					}
					outputMesh.normals = normals;
					for (int i = 0; i < indicescount; i++)
					{
						indices[i] = (reader.ReadInt32());
					}

					outputMesh.SetIndexBufferParams(indicescount, IndexFormat.UInt32);
					outputMesh.SetIndexBufferData(indices, 0, 0, indicescount);

					outputMesh.subMeshCount = submeshcount;
					int[] submeshIndexes = new int[submeshcount];

                    for (int i = 0; i < submeshcount; i++)
                    {
						submeshIndexes[i] = reader.ReadInt32();
						int smstartindex = reader.ReadInt32();
						int smIndexcount = reader.ReadInt32();
						int smFirstindex = reader.ReadInt32(); 
						int smVertexcount = reader.ReadInt32();
						SubMeshDescriptor smd = new SubMeshDescriptor(smstartindex,smIndexcount);
						outputMesh.SetSubMesh(i, smd);
					}

					//return the data
					submeshIndices = submeshIndexes;
					mesh = outputMesh;
				}
			}
		}
		public void GetIDData(GameObject obj, int vertexIndex, System.Action<string> callback = null)
		{
			if (!obj) return;

			ObjectData objectMapping = obj.GetComponent<ObjectData>();
			if (!objectMapping || objectMapping.ids.Count == 0)
			{
				//No/empty object data? Download it and return the ID
				StartCoroutine(DownloadObjectData(obj, vertexIndex, callback));
			}
			else
			{
				//Return the ID directly
				int idIndex = objectMapping.vectorMap[vertexIndex];
				var id = objectMapping.ids[idIndex];
				callback?.Invoke(id);
			}
		}

		public void GetAllVerts(List<string> selectedIDs)
		{

		}

		private IEnumerator DownloadObjectData(GameObject obj, int vertexIndex, System.Action<string> callback)
		{
			yield return new WaitUntil(() => pauseLoading == false); //wait for opportunity to start

			pauseLoading = true;
			var meshFilter = obj.GetComponent<MeshFilter>();
			if (!meshFilter) yield break;

			string name = meshFilter.mesh.name;
			string dataName = name.Replace(" Instance", "");
			dataName = dataName.Replace("mesh", "building");
			dataName = dataName.Replace("-", "_") + "-data";
			string dataURL = $"{Config.activeConfiguration.webserverRootPath}{Config.activeConfiguration.buildingsMetaDataPath}{dataName}";

			ObjectMappingClass data;
			string id = "null";

			using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
			{
				yield return uwr.SendWebRequest();

				if (uwr.isNetworkError || uwr.isHttpError)
				{
					//Not showing warnings for now, because this can occur pretty often. I dialog would be annoying.
					//WarningDialogs.Instance.ShowNewDialog("De metadata voor " + obj.name + " kon niet worden geladen. Ben je nog online?");
				}
				else if (obj != null)
				{

					ObjectData objectMapping = obj.AddComponent<ObjectData>();
					AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);

					yield return new WaitForEndOfFrame();

					data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];
					int idIndex = data.vectorMap[vertexIndex];
					id = data.ids[idIndex];
					objectMapping.ids = data.ids;
					objectMapping.uvs = data.uvs;
					objectMapping.vectorMap = data.vectorMap;

					newAssetBundle.Unload(true);
				}
			}
			callback?.Invoke(id);
			yield return null;
			pauseLoading = false;
		}

		public void Highlight(List<string> ids)
		{
			StartCoroutine(HighlightIDs(ids));
		}

		/// <summary>
		/// Hide mesh parts with the matching object data ID's
		/// </summary>
		/// <param name="ids">List of unique (BAG) id's we want to hide</param>
		public void Hide(List<string> ids)
		{

			StartCoroutine(HideIDs(ids));
		}

		/// <summary>
		/// Adds mesh colliders to the meshes found within this layer
		/// </summary>
		/// <param name="onlyTileUnderPosition">Optional world position where this tile should be close to</param>
		public void AddMeshColliders(Vector3 onlyTileUnderPosition = default)
		{
			MeshCollider meshCollider;
			MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();

			if (meshFilters != null)
			{
				if (onlyTileUnderPosition != default)
				{
					foreach (MeshFilter meshFilter in meshFilters)
					{
						if (Mathf.Abs(onlyTileUnderPosition.x - meshFilter.gameObject.transform.position.x) < tileSize && Mathf.Abs(onlyTileUnderPosition.z - meshFilter.gameObject.transform.position.z) < tileSize)
						{
							meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
							if (meshCollider == null)
							{
								meshFilter.gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
							}
						}
					}
					return;
				}

				//Just add all MeshColliders if no specific area was supplied
				foreach (MeshFilter meshFilter in meshFilters)
				{
					meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
					if (meshCollider == null)
					{
						meshFilter.gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
					}
				}
			}
		}
		private IEnumerator HighlightIDs(List<string> ids)
		{
			pauseLoading = true;
			ObjectData objectData;

			//Check all tiles that have metadata if they have matching ID's
			foreach (KeyValuePair<Vector2Int, Tile> kvp in tiles)
			{
				if (kvp.Value.gameObject == null)
				{
					continue;
				}
				objectData = kvp.Value.gameObject.GetComponent<ObjectData>();
				if (objectData != null)
				{
					if (ids.Count > 0)
					{
						objectData.highlightIDs = ids.Where(targetID => objectData.ids.Any(objectId => objectId == targetID)).ToList<string>();
					}
					else
					{
						objectData.highlightIDs.Clear();
					}

					objectData.ApplyDataToIDsTexture();
				}
			}
			pauseLoading = false;
			yield return null;
		}

		private IEnumerator HideIDs(List<string> ids)
		{
			pauseLoading = true;
			ObjectData objectData;
			foreach (KeyValuePair<Vector2Int, Tile> kvp in tiles)
			{
				if (kvp.Value.gameObject == null)
				{
					continue;
				}
				objectData = kvp.Value.gameObject.GetComponent<ObjectData>();
				if (objectData != null)
				{
					if (ids.Count > 0)
					{
						objectData.hideIDs.AddRange(ids.Where(targetID => objectData.ids.Any(objectId => objectId == targetID)).ToList<string>());
					}
					else
					{
						objectData.hideIDs.Clear();
					}
					yield return new WaitForEndOfFrame();
					objectData.ApplyDataToIDsTexture();
					yield return new WaitForEndOfFrame();
				}
			}
			pauseLoading = false;
			yield return null;
		}

	}
}
