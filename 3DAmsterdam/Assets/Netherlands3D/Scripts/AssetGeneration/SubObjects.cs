using Netherlands3D;
using Netherlands3D.Cameras;
using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	private List<SubOjectData> subObjectDatas;
	public List<SubOjectData> SubObjectDatas { get => subObjectDatas; private set => subObjectDatas = value; }
	public bool Altered { get; private set; }

	private Mesh mesh;
	private Color[] vertexColors;

	private bool downloadingSubObjects = false;

	private void Awake()
	{
		SubObjectDatas = new List<SubOjectData>();
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
		if (SubObjectDatas.Count > 0 && !downloadingSubObjects)
		{
			callback(GetIDByVertexIndex(selectedVert));
		}
		else
		{
			downloadingSubObjects = false;
			StartCoroutine(
				LoadMetaDataAndSelect(selectedVert, callback)
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
					SubObjectDatas.Add(new SubOjectData()
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

	private IEnumerator LoadMetaDataAndSelect(int selectedVertAfterLoading, System.Action<string> callback)
	{	
		yield return LoadMetaData(mesh);

		var id = GetIDByVertexIndex(selectedVertAfterLoading);
		callback(id);
	}

	public IEnumerator LoadMetaDataAndApply(List<SubOjectData> prevousObjectDatas)
	{
		yield return LoadMetaData(mesh);
		
		//Sync parts of the data with our new objectdata 
		foreach(var subObject in SubObjectDatas)
		{
			foreach (var previousSubObject in prevousObjectDatas)
			{
				if(subObject.objectID == previousSubObject.objectID){
					subObject.color = previousSubObject.color;
					subObject.hidden = previousSubObject.hidden;
				}
			}
		}

		//Apply colors/hidden etc back to new geometry+data
		for (int i = 0; i < SubObjectDatas.Count; i++)
		{
			var subObject = SubObjectDatas[i];
			for (int j = 0; j < subObject.length; j++)
			{
				vertexColors[subObject.startIndex + j] = subObject.color;
			}
		}
		mesh.colors = vertexColors;
	}

	private IEnumerator LoadMetaData(Mesh mesh)
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
			downloadingSubObjects = false;
			yield return null;
		}
		yield return null;
	}


	private string GetIDByVertexIndex(int vertexIndex)
	{
		//Find all subobject ranges, and color the verts at those indices
		for (int i = 0; i < SubObjectDatas.Count; i++)
		{
			var subObject = SubObjectDatas[i];
			if (vertexIndex >= subObject.startIndex && vertexIndex < subObject.startIndex+ subObject.length)
			{
				return subObject.objectID;
			}
		}
		return "";
	}

	public void ColorAll(Color highlightColor)
	{
		Altered = true;

		for (int i = 0; i < SubObjectDatas.Count; i++)
		{
			var subObject = SubObjectDatas[i];
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
		Altered = true;

		//Find all subobject ranges, and color the verts at those indices
		for (int i = 0; i < SubObjectDatas.Count; i++)
		{
			var subObject = SubObjectDatas[i];
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
		Altered = true;

		for (int i = 0; i < SubObjectDatas.Count; i++)
		{
			var subObject = SubObjectDatas[i];
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
		Altered = false;

		for (int i = 0; i < SubObjectDatas.Count; i++)
		{
			var subObject = SubObjectDatas[i];
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
		Altered = true;

		Vector3[] vertices = mesh.vertices;

		//Find all subobject ranges, and color the verts at those indices
		for (int i = 0; i < SubObjectDatas.Count; i++)
		{
			var subObject = SubObjectDatas[i];
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
		Altered = true;

		//Find all subobject ranges, and color the verts at those indices
		for (int i = 0; i < SubObjectDatas.Count; i++)
		{
			var subObject = SubObjectDatas[i];
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
