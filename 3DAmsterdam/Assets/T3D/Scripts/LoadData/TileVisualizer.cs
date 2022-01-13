using ConvertCoordinates;
using Netherlands3D.Core;
using Netherlands3D.LayerSystem;
using Netherlands3D.T3D.Uitbouw;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TileVisualizer : MonoBehaviour
{
	public string DataPath;
	public string MetaDataPath;
	public Material TileMaterial;
	public float TileSize = 1000;
	public UnityEngine.Rendering.ShadowCastingMode ShadowCastingMode;

    private void Start()
    {
	}

    internal void LoadTile(double x, double y)
    {

		x =  Math.Floor(x/ TileSize) * TileSize;
		y = Math.Floor(y/ TileSize) * TileSize;

		StartCoroutine(DownloadBinaryMesh(x,y));
    }

	private IEnumerator DownloadBinaryMesh(double x, double y)
	{
		string url = DataPath.ReplaceXY(x, y);

		//On WebGL we request brotli encoded files instead. We might want to base this on browser support.
#if !UNITY_EDITOR && UNITY_WEBGL
			if(brotliCompressedExtention.Length>0)
				url += brotliCompressedExtention;
#endif
		var webRequest = UnityWebRequest.Get(url);
		/*#if !UNITY_EDITOR && UNITY_WEBGL
					webRequest.SetRequestHeader("Accept-Encoding", "br");
					//Not allowed for this unity version (but seems to be legacy)
		#endif*/
		
		yield return webRequest.SendWebRequest();

		if (webRequest.result != UnityWebRequest.Result.Success)
		{
			Debug.Log($"Could not download tile: {url}");
		}
		else
		{
			byte[] results = webRequest.downloadHandler.data;

			CreateTileGameObject(results, new Vector2((float)x,(float)y) , false);
		}
	}

	private void CreateTileGameObject(byte[] binaryMeshData, Vector2 position, bool addCollider)
	{
		var container = new GameObject();

		container.name = $"{position.x}-{position.y}";
		container.transform.parent = transform.gameObject.transform;
		container.layer = container.transform.parent.gameObject.layer;

		container.transform.position = CoordConvert.RDtoUnity(new Vector2(position.x + (TileSize / 2), position.y + (TileSize / 2)));

		container.SetActive(true);

		var mesh = BinaryMeshConversion.ReadBinaryMesh(binaryMeshData, out int[] submeshIndices);
		mesh.name = $"{position.x}-{position.y}";
		container.AddComponent<MeshFilter>().sharedMesh = mesh;

		var meshRenderer = container.AddComponent<MeshRenderer>();
		List<Material> materialList = new List<Material>();
		for (int i = 0; i < submeshIndices.Length; i++)
		{
			materialList.Add(TileMaterial);
		}
		meshRenderer.sharedMaterials = materialList.ToArray();
		meshRenderer.shadowCastingMode = ShadowCastingMode;

		if (addCollider)
		{
			container.AddComponent<MeshCollider>().sharedMesh = mesh;
		}

		container.transform.parent = transform;

		ObjectData objectdata = container.AddComponent<ObjectData>();

		StartCoroutine(DownloadBinaryMeshData(objectdata, position));

	}

	private IEnumerator DownloadBinaryMeshData(ObjectData objectdata, Vector2 position)
	{
		string url = MetaDataPath.ReplaceXY(position.x, position.y);
		var webRequest = UnityWebRequest.Get(url);

		yield return webRequest.SendWebRequest();

		if (webRequest.result != UnityWebRequest.Result.Success)
		{
			Debug.Log($"Could not download tile: {url}");
		}
		else
		{
			byte[] results = webRequest.downloadHandler.data;
			var objectmapping = BinaryMeshConversion.ReadBinaryMetaData(results);

			objectdata.ids = objectmapping.ids;

			//construct vectormap from compressed vectormap 
			var newVectorMapping = new List<int>();
			foreach(var vm in objectmapping.vectorMap)
            {

            }


			objectdata.vectorMap = objectmapping.vectorMap;

			objectdata.highlightIDs = new List<string>()
			{
				T3DInit.Instance.BagId
			};

			objectdata.ApplyDataToIDsTexture();

			var rd = new Vector3RD(position.x, position.y, 0);

			var tileOffset = CoordConvert.RDtoUnity(rd) + new Vector3(500, 0, 500);

			MetadataLoader.Instance.RaiseBuildingMetaDataLoaded(objectdata, tileOffset);

		}
	}

    
}
