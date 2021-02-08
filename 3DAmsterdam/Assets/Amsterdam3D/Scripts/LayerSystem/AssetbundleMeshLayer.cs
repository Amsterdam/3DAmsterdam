using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using UnityEngine.Networking;
using System;
using System.Linq;

namespace LayerSystem
{
    public class AssetbundleMeshLayer : Layer
    {
        //public Material DefaultMaterial;
		public List<Material> DefaultMaterialList = new List<Material>();
		public bool createMeshcollider = false;
		public override void OnDisableTiles(bool isenabled)
        {

        }

		public override void HandleTile(TileChange tileChange, System.Action<TileChange> callback = null)
        {
			TileAction action = tileChange.action;
			switch (action)
			{
				case TileAction.Create:
					Tile newTile = CreateNewTile(tileChange);
					tiles.Add(new Vector2Int(tileChange.X, tileChange.Y), newTile);
					break;
				case TileAction.Upgrade:
					tiles[new Vector2Int(tileChange.X, tileChange.Y)].LOD++;
					break;
				case TileAction.Downgrade:
					tiles[new Vector2Int(tileChange.X, tileChange.Y)].LOD--;
					break;
				case TileAction.Remove:
					RemoveGameObjectFromTile(tileChange);
					tiles.Remove(new Vector2Int(tileChange.X, tileChange.Y));
					callback(tileChange);
					return;
				default:
					break;
			}
			StartCoroutine(DownloadAssetBundle(tileChange,callback));
		}
		private Tile CreateNewTile(TileChange tileChange)
		{
			Tile tile = new Tile();
			tile.LOD = 0;
			tile.tileKey = new Vector2Int(tileChange.X,tileChange.Y);
			tile.layer = transform.gameObject.GetComponent<Layer>();
			tile.gameObject = new GameObject();
			tile.gameObject.transform.parent = transform.gameObject.transform;
			tile.gameObject.layer = tile.gameObject.transform.parent.gameObject.layer;
			tile.gameObject.transform.position = CoordConvert.RDtoUnity(new Vector2(tileChange.X, tileChange.Y));

			return tile;
		}
		private void RemoveGameObjectFromTile(TileChange tileChange)
		{
			if (tiles.ContainsKey(new Vector2Int(tileChange.X, tileChange.Y)))
			{

				Tile tile = tiles[new Vector2Int(tileChange.X, tileChange.Y)];
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
				Destroy(tiles[new Vector2Int(tileChange.X, tileChange.Y)].gameObject);
				
			}
		}
		private IEnumerator DownloadAssetBundle(TileChange tileChange, System.Action<TileChange> callback = null)
		{
			int lod = tiles[new Vector2Int(tileChange.X, tileChange.Y)].LOD;
			string url = Constants.BASE_DATA_URL + Datasets[lod].path;
            if (Datasets[lod].path.StartsWith("https://"))
            {
				url = Datasets[lod].path;

			}
			url = url.Replace("{x}", tileChange.X.ToString());
			url = url.Replace("{y}", tileChange.Y.ToString());
			using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(url))
			{
				yield return uwr.SendWebRequest();

				if (uwr.isNetworkError || uwr.isHttpError)
				{
					RemoveGameObjectFromTile(tileChange);
					callback(tileChange);
				}
				else
				{
					AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
					yield return new WaitUntil(() => pauseLoading == false);
					GameObject newGameobject = CreateNewGameObject(assetBundle, tileChange);
					if (newGameobject != null)
					{
						if (TileHasHighlight(tileChange))
						{
							StartCoroutine(DownloadIDMappingData(tileChange, newGameobject,callback));
						}
						else
						{
							RemoveGameObjectFromTile(tileChange);
							tiles[new Vector2Int(tileChange.X, tileChange.Y)].gameObject = newGameobject;
							callback(tileChange);
						}
					}
					else
					{

						callback(tileChange);
					}

				}
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

		private IEnumerator DownloadIDMappingData(TileChange tileChange, GameObject newGameobject, System.Action<TileChange> callback = null)
		{
			Tile tile = tiles[new Vector2Int(tileChange.X, tileChange.Y)];
			ObjectData oldObjectMapping = tile.gameObject.GetComponent<ObjectData>();
			GameObject newTile = newGameobject;
			string name = newTile.GetComponent<MeshFilter>().mesh.name;
			Debug.Log(name);
			string dataName = name.Replace(" Instance", "");
			dataName = dataName.Replace("mesh", "building");
			dataName = dataName.Replace("-", "_") + "-data";
			string dataURL = Constants.TILE_METADATA_URL + dataName;
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
					data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];

					objectMapping.highlightIDs = oldObjectMapping.highlightIDs;
					objectMapping.hideIDs = oldObjectMapping.hideIDs;
					objectMapping.ids = data.ids;
					objectMapping.uvs = data.uvs;
					objectMapping.vectorMap = data.vectorMap;
					objectMapping.mappedUVs = data.mappedUVs;
					objectMapping.mesh = newTile.GetComponent<MeshFilter>().mesh;
					objectMapping.triangleCount = data.triangleCount;
					objectMapping.ApplyDataToIDsTexture();
					newAssetBundle.Unload(true);
				}
			}
			yield return new WaitUntil(() => pauseLoading == false);
			RemoveGameObjectFromTile(tileChange);
			tiles[new Vector2Int(tileChange.X, tileChange.Y)].gameObject = newGameobject;
			//activeTileChanges.Remove(new Vector3Int(tileChange.X, tileChange.Y, tileChange.layerIndex));
			yield return null;
			callback(tileChange);

		}

		private GameObject CreateNewGameObject(AssetBundle assetBundle, TileChange tileChange)
		{
			GameObject container = new GameObject();
			
			container.name = tileChange.X.ToString() + "-" + tileChange.Y.ToString();
			container.transform.parent = transform.gameObject.transform;
			container.layer = container.transform.parent.gameObject.layer;
			container.transform.position = CoordConvert.RDtoUnity(new Vector2(tileChange.X + 500, tileChange.Y + 500));
			//Material defaultMaterial = DefaultMaterial;
			container.SetActive(isEnabled);
			Mesh[] meshesInAssetbundle = new Mesh[0];
			try
			{
				meshesInAssetbundle = assetBundle.LoadAllAssets<Mesh>();
			}
			catch (Exception)
			{
				Destroy(container);
				assetBundle.Unload(true);
				return null;
			}
			Mesh mesh = meshesInAssetbundle[0];
			Vector2 uv = new Vector2(0.33f, 0.5f);
			int count = mesh.vertexCount;

			List<Vector2> uvs = new List<Vector2>();
			Vector2 defaultUV = new Vector2(0.33f, 0.6f);
			for (int i = 0; i < count; i++)
			{
				uvs.Add(defaultUV);
			}
			mesh.uv2 = uvs.ToArray();

			container.AddComponent<MeshFilter>().mesh = mesh;
			container.AddComponent<MeshRenderer>().sharedMaterials = DefaultMaterialList.ToArray();
			if (createMeshcollider)
			{
				container.AddComponent<MeshCollider>().sharedMesh = mesh;
			}

			assetBundle.Unload(false);

			return container;
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
			string dataURL = Constants.TILE_METADATA_URL + dataName;

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
					data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];
					int idIndex = data.vectorMap[vertexIndex];
					id = data.ids[idIndex];
					objectMapping.ids = data.ids;
					objectMapping.uvs = data.uvs;
					objectMapping.vectorMap = data.vectorMap;
					objectMapping.mappedUVs = data.mappedUVs;

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
					objectData.ApplyDataToIDsTexture();
				}
			}
			pauseLoading = false;
			yield return null;
		}

	}
}
