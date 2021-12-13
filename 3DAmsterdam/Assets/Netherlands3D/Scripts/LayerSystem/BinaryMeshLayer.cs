using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using UnityEngine.Networking;
using System;
using System.Linq;
using UnityEngine.Rendering;

namespace Netherlands3D.LayerSystem
{
    public class BinaryMeshLayer : Layer
    {
		public List<Material> DefaultMaterialList = new List<Material>();
		public bool createMeshcollider = false;
		public bool addHighlightuvs = false;
		public ShadowCastingMode tileShadowCastingMode = ShadowCastingMode.On;
		
		private GameObject container;
		private Mesh mesh;
		private MeshRenderer meshRenderer;
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
			tiles[tileKey].runningCoroutine = StartCoroutine(DownloadBinaryMesh(tileChange, callback));
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
		private IEnumerator DownloadBinaryMesh(TileChange tileChange, System.Action<TileChange> callback = null)
		{
			var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
			int lod = tiles[tileKey].LOD;
			string url = Config.activeConfiguration.webserverRootPath + Datasets[lod].path;
            if (Datasets[lod].path.StartsWith("https://") || Datasets[lod].path.StartsWith("file://"))
            {
				url = Datasets[lod].path;
			}

			url = url.ReplaceXY(tileChange.X, tileChange.Y);
			Debug.Log(url);
			//On WebGL we request brotli encoded files instead. We might want to base this on browser support.
#if !UNITY_EDITOR && UNITY_WEBGL
			url += Config.activeConfiguration.brotliCompressedAssetFileExtention;
#endif
			var webRequest = UnityWebRequest.Get(url);
#if !UNITY_EDITOR && UNITY_WEBGL
			webRequest.SetRequestHeader("Accept-Encoding", "br");
#endif

			tiles[tileKey].runningWebRequest = webRequest;
			yield return webRequest.SendWebRequest();

			if (!tiles.ContainsKey(tileKey)) yield break;

			tiles[tileKey].runningWebRequest = null;

			if (webRequest.result != UnityWebRequest.Result.Success)
			{
				RemoveGameObjectFromTile(tileKey);
				callback(tileChange);
			}
			else
			{
				byte[] results = webRequest.downloadHandler.data;

				yield return new WaitUntil(() => pauseLoading == false);
				GameObject newGameobject = CreateNewGameObject(url,results, tileChange);
				if (newGameobject != null)
				{
					if (TileHasSubObjectAltered(tileChange))
					{
						yield return SyncSubObjects(tileChange, newGameobject,callback);
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
			foreach(var renderer in existingTiles)
			{
				renderer.shadowCastingMode = tileShadowCastingMode;
			}
		}

		private bool TileHasSubObjectAltered(TileChange tileChange)
		{
			Tile tile = tiles[new Vector2Int(tileChange.X, tileChange.Y)];
			if (tile.gameObject == null)
			{
				return false;
			}

			var subObjects = tile.gameObject.GetComponent<SubObjects>();
			if (subObjects == null)
			{
				return false;
			}
			if (subObjects.Altered == false)
			{
				return false;
			}

			return true;
		}

		private IEnumerator SyncSubObjects(TileChange tileChange, GameObject newGameobject, System.Action<TileChange> callback = null)
		{
			Tile tile = tiles[new Vector2Int(tileChange.X, tileChange.Y)];
			SubObjects oldObjectMapping = tile.gameObject.GetComponent<SubObjects>();
			SubObjects newObjectMapping = newGameobject.AddComponent<SubObjects>();

			yield return newObjectMapping.LoadMetaDataAndApply(oldObjectMapping.SubObjectsData);

			yield return new WaitUntil(() => pauseLoading == false);
			RemoveGameObjectFromTile(tile.tileKey);
			tiles[tile.tileKey].gameObject = newGameobject;

			yield return null;
			callback(tileChange);
		}

		private GameObject CreateNewGameObject(string source,byte[] binaryMeshData, TileChange tileChange)
		{
			container = new GameObject();
			
			container.name = tileChange.X.ToString() + "-" + tileChange.Y.ToString();
			container.transform.parent = transform.gameObject.transform;
			container.layer = container.transform.parent.gameObject.layer;
			container.transform.position = CoordConvert.RDtoUnity(new Vector2(tileChange.X + (tileSize/2), tileChange.Y + (tileSize/2)));
			
			container.SetActive(isEnabled);

			mesh = BinaryMeshConversion.ReadBinaryMesh(binaryMeshData, out int[] submeshIndices);
			mesh.name = source;
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
	}
}
