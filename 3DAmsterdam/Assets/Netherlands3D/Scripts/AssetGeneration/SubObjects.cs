using Netherlands3D;
using Netherlands3D.Cameras;
using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class SubObjects : MonoBehaviour
{
	[System.Serializable]
	public class SubOjectData{
		public string objectID;
		public int startIndex;
		public int length;
		public Color color = Color.white;
		public bool hidden = false;
	}

	[SerializeField]
	private List<SubOjectData> subObjectIndices;

	private Mesh mesh;
	private Color[] vertexColors;

	private bool downloadingSubObjects = false;

	private void Awake()
	{
		subObjectIndices = new List<SubOjectData>();
		mesh = this.GetComponent<MeshFilter>().sharedMesh;
		vertexColors = new Color[mesh.vertexCount];
		for (int i = 0; i < vertexColors.Length; i++)
		{
			vertexColors[i] = Color.white;
		}
	}
	
	public void Select(int selectedVert, System.Action<string> callback)
	{
		//Select using vertex index, or download metadata first
		if (subObjectIndices.Count > 0 && !downloadingSubObjects)
		{
			callback(GetIDByVertexIndex(selectedVert));
		}
		else
		{
			downloadingSubObjects = false;
			StartCoroutine(
				LoadMetaData(mesh, selectedVert, callback)
			);
		}
	}

	private void ReadMetaDataFile(byte[] results)
	{
		using (var stream = new MemoryStream(results))
		{
			using (BinaryReader reader = new BinaryReader(stream))
			{
				var version = reader.ReadInt32();
				var subObjects = reader.ReadInt32();
				Debug.Log("Metadata subobject count: " + subObjects);
				int currentMeshIndex = 0;
				for (int i = 0; i < subObjects; i++)
				{
					var id = reader.ReadString();
					var indicesLength = reader.ReadInt32();
					subObjectIndices.Add(new SubOjectData()
					{
						objectID = id,
						startIndex = currentMeshIndex,
						length = indicesLength
					});
					currentMeshIndex += indicesLength;
				}
			}
		}
	}

	private IEnumerator LoadMetaData(Mesh mesh, int selectedVertAfterLoading, System.Action<string> callback)
	{
		var metaDataName = mesh.name.Replace(".bin","-data.bin");
		var webRequest = UnityWebRequest.Get(metaDataName);
#if !UNITY_EDITOR && UNITY_WEBGL
		webRequest.SetRequestHeader("Accept-Encoding", "br");
#endif

		yield return webRequest.SendWebRequest();

		if (webRequest.result != UnityWebRequest.Result.Success)
		{
			Debug.Log("No metadata on path: " + metaDataName);
		}
		else
		{
			byte[] results = webRequest.downloadHandler.data;
			ReadMetaDataFile(results);
			if (selectedVertAfterLoading > -1) 
			{
				callback(GetIDByVertexIndex(selectedVertAfterLoading));
			}

			downloadingSubObjects = false;
		}
	}

	private string GetIDByVertexIndex(int vertexIndex)
	{
		Debug.Log($"SelectByVertexIndex {vertexIndex}");
		//Find all subobject ranges, and color the verts at those indices
		for (int i = 0; i < subObjectIndices.Count; i++)
		{
			var subObject = subObjectIndices[i];
			if (vertexIndex >= subObject.startIndex && vertexIndex < subObject.startIndex+ subObject.length)
			{
				return subObject.objectID;
			}
		}
		return "";
	}

	public void ColorAll(Color highlightColor)
	{
		for (int i = 0; i < subObjectIndices.Count; i++)
		{
			var subObject = subObjectIndices[i];
			subObject.color = highlightColor;

			for (int j = 0; j < subObject.length; j++)
			{
				vertexColors[subObject.startIndex + j] = subObject.color;
			}
		}

		mesh.colors = vertexColors;
	}

	public void ColorWithIDs(List<string> ids, Color highlightColor)
	{
		//Find all subobject ranges, and color the verts at those indices
		for (int i = 0; i < subObjectIndices.Count; i++)
		{
			var subObject = subObjectIndices[i];
			if (ids.Contains(subObject.objectID))
			{
				subObject.color = highlightColor;
			}
			else if(!subObject.hidden){
				subObject.color = Color.white;
			}
			for (int j = 0; j < subObject.length; j++)
			{
				vertexColors[subObject.startIndex + j] = subObject.color;
			}
		}
		mesh.colors = vertexColors;
	}

	public void HideWithIDs(List<string> ids){
		for (int i = 0; i < subObjectIndices.Count; i++)
		{
			var subObject = subObjectIndices[i];
			if (ids.Contains(subObject.objectID))
			{
				subObject.hidden = true;
				subObject.color = new Color(subObject.color.r, subObject.color.g, subObject.color.b, 0.0f);
			}
			else{
				subObject.hidden = false;
				subObject.color = new Color(subObject.color.r, subObject.color.g, subObject.color.b, 1.0f);
			}
			for (int j = 0; j < subObject.length; j++)
			{
				vertexColors[subObject.startIndex + j] = subObject.color;
			}
		}
		mesh.colors = vertexColors;
	}

	public void UnhideAll()
	{
		for (int i = 0; i < subObjectIndices.Count; i++)
		{
			var subObject = subObjectIndices[i];
			subObject.color = Color.white;
			subObject.hidden = false;
			for (int j = 0; j < subObject.length; j++)
			{
				vertexColors[subObject.startIndex + j] = subObject.color;
			}
		}
		mesh.colors = vertexColors;
	}

	public void DrawVertexColorsAccordingToHeight(float max, Color minColor, Color maxColor)
	{
		Vector3[] vertices = mesh.vertices;

		//Find all subobject ranges, and color the verts at those indices
		for (int i = 0; i < subObjectIndices.Count; i++)
		{
			var subObject = subObjectIndices[i];
			Color randomColor;

			//determine height of vertex
			float height = 0.0f;
			for (int h = 0; h < subObject.length; h++)
			{
				if (vertices[h].y > height) height = vertices[h].y;
			}
			randomColor = Color.Lerp(minColor, maxColor, height / max);

			//apply color
			for (int j = 0; j < subObject.length; j++)
			{
				vertexColors[subObject.startIndex + j] = randomColor;
			}
		}

		mesh.colors = vertexColors;
	}

	public void DrawRandomVertexColors()
	{
		//Find all subobject ranges, and color the verts at those indices
		for (int i = 0; i < subObjectIndices.Count; i++)
		{
			var subObject = subObjectIndices[i];
			Color randomColor = new Color(Random.value, Random.value, Random.value, 1.0f);
			subObject.color = randomColor;

			for (int j = 0; j < subObject.length; j++)
			{
				vertexColors[subObject.startIndex+j] = randomColor;
			}
		}

		mesh.colors = vertexColors;
	}
}
