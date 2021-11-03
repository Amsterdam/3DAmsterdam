using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class MetaData : MonoBehaviour
{
	[System.Serializable]
	public struct SubbjectIndices{
		public string objectID;
		public int startIndex;
		public int length;
	}

	[SerializeField]
	private List<SubbjectIndices> subObjectIndices;

	private int currentLODIndices = 0;
	private Mesh mesh;

	private void Awake()
	{
		subObjectIndices = new List<SubbjectIndices>();
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

			AddRandomVertexColors();
		}
	}

	private void AddRandomVertexColors()
	{
		Color[] colors = new Color[mesh.vertexCount];

		//Find all subobject ranges, and color the verts at those indices
		for (int i = 0; i < subObjectIndices.Count; i++)
		{
			var subObject = subObjectIndices[i];
			Color randomColor = new Color(Random.value, Random.value, Random.value, 1.0f);
			for (int j = subObject.startIndex; j < subObject.length; j++)
			{
				colors[j] = randomColor;
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
					subObjectIndices.Add(new SubbjectIndices()
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
