using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;
using UnityEngine.Networking;
using System;
using System.Linq;
using UnityEngine.Rendering;

namespace Netherlands3D.TileSystem
{
    public class BinaryPointMeshLayer : Layer
    {
		public Mesh[] instanceableMeshes;
		public Material sharedMaterial;

		public ShadowCastingMode tileShadowCastingMode = ShadowCastingMode.On;

		public string brotliCompressedExtention = ".br";

		private GameObject container;
		private Mesh mesh;

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
			string url = Datasets[lod].path;
            if (Datasets[lod].path.StartsWith("https://") || Datasets[lod].path.StartsWith("file://"))
            {
				url = Datasets[lod].path;
			}

			url = url.ReplaceXY(tileChange.X, tileChange.Y);

			//On WebGL we request brotli encoded files instead. We might want to base this on browser support.

#if !UNITY_EDITOR && UNITY_WEBGL
			if(brotliCompressedExtention.Length>0)
				url += brotliCompressedExtention;
#endif
			var webRequest = UnityWebRequest.Get(url);

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
				RemoveGameObjectFromTile(tileKey);
				tiles[tileKey].gameObject = newGameobject;

				callback(tileChange);
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

			container.AddComponent<DrawInstancedMeshes>().DrawInstancedMeshesOnPoints(mesh, sharedMaterial, instanceableMeshes);

			return container;
		}
	}
}
