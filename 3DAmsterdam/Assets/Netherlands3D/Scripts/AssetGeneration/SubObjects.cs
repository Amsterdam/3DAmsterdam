using Netherlands3D.Cameras;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SubObjects : MonoBehaviour
{
	[System.Serializable]
	public struct SubOjectIndices{
		public string objectID;
		public int startIndex;
		public int length;
	}

	[SerializeField]
	private List<SubOjectIndices> subObjectIndices;

	private int currentLODIndices = 0;
	private Mesh mesh;

	private void Awake()
	{
		subObjectIndices = new List<SubOjectIndices>();
		LoadObjectSeperation();
	}

	public void RegisterClick()
	{
		if(subObjectIndices.Count == 0)
		{
			LoadObjectSeperation();
		}
	}
	
	void LoadObjectSeperation()
	{
		mesh = this.GetComponent<MeshFilter>().sharedMesh;
		StartCoroutine(LoadMetaData(mesh));
	}

	private IEnumerator LoadMetaData(Mesh mesh)
	{
		var metaDataName = mesh.name.Replace(".br","").Replace(".bin","-data.bin");
		var webRequest = UnityWebRequest.Get(metaDataName);
		yield return webRequest.SendWebRequest();

		if (webRequest.result != UnityWebRequest.Result.Success)
		{
			Debug.Log("No metadata on path: " + metaDataName);
		}
		else
		{
			byte[] results = webRequest.downloadHandler.data;
			ReadMetaDataFile(results);
			DrawRandomVertexColors();
		}
	}

	public string SelectByVertexIndex(int vertexIndex)
	{
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

	public void HightlightBuildingWithID(string id, Color highlightColor)
	{
		Color[] colors = new Color[mesh.vertexCount];

		//Find all subobject ranges, and color the verts at those indices
		for (int i = 0; i < subObjectIndices.Count; i++)
		{
			var subObject = subObjectIndices[i];
			if (subObject.objectID == id)
			{
				//apply color to object range
				for (int j = 0; j < subObject.length; j++)
				{
					colors[subObject.startIndex + j] = highlightColor;
				}
			}
		}

		mesh.colors = colors;
	}

	public void DrawVertexColorsAccordingToHeight(float max, Color minColor, Color maxColor)
	{
		Color[] colors = new Color[mesh.vertexCount];
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
				colors[subObject.startIndex + j] = randomColor;
			}
		}

		mesh.colors = colors;
	}

	public void DrawRandomVertexColors()
	{
		Color[] colors = new Color[mesh.vertexCount];

		//Find all subobject ranges, and color the verts at those indices
		for (int i = 0; i < subObjectIndices.Count; i++)
		{
			var subObject = subObjectIndices[i];
			Color randomColor = new Color(Random.value, Random.value, Random.value, 1.0f);
			for (int j = 0; j < subObject.length; j++)
			{
				colors[subObject.startIndex+j] = randomColor;
			}
		}

		mesh.colors = colors;
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
					subObjectIndices.Add(new SubOjectIndices()
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
}
