using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Metadata : MonoBehaviour
{
	[System.Serializable]
	public struct SubbjectIndices{
		public string objectID;
		public int startIndex;
		public int length;
	}

	[SerializeField]
	private List<SubbjectIndices> subObjectIndices;

	private void Awake()
	{
		subObjectIndices = new List<SubbjectIndices>();
	}

	// Update is called once per frame
	void LoadObjectSeperation()
	{
		var mesh = this.GetComponent<MeshFilter>().mesh;
		var binaryMeshUrl = mesh.name;
		StartCoroutine(LoadMetaData(mesh));
	}

	private IEnumerator LoadMetaData(Mesh mesh)
	{
		var metaDataName = mesh.name.Replace(".bin", "-data.bin");

		var webRequest = UnityWebRequest.Get(metaDataName);
		yield return webRequest.SendWebRequest();

		if (webRequest.result != UnityWebRequest.Result.Success)
		{
			Debug.Log("No metadata for file " + mesh.name);
		}
		else
		{
			byte[] results = webRequest.downloadHandler.data;
			using (var stream = new MemoryStream(results))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					var version = reader.ReadInt32();
					var subObjects = reader.ReadInt32();

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
}
