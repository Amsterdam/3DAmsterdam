using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectData : MonoBehaviour
{
	public List<string> highlightIDs = new List<string>();
	public List<string> ids;
	public List<string> hideIDs = new List<string>();
	public Vector2[] uvs;
	public List<int> vectorMap;
	public List<Vector2> mappedUVs;
	public Mesh mesh;
	public List<int> triangleCount;

	private Vector2 defaultUV = new Vector2(0.33f, 0.6f);
	private Vector2 highlightUV = new Vector2(0.66f, 0.9f);
	private Vector2 hideUV = new Vector2(0.3f, 0.2f);

	public void UpdateUVs()
	{
		StopAllCoroutines();
		StartCoroutine(DetermineUVs());
	}

	/// <summary>
	/// Determine whether the UV's should move to hidden, highlighted or default position.
	/// Hidden should get prio over selected. (You cant select what you dont see)
	/// </summary>
	private IEnumerator DetermineUVs()
	{
		if (ids == null) yield break;

		int vertexcount = mesh.vertexCount;
		int idcount = ids.Count;
		List<Vector2> itemUVs = new List<Vector2>();
		for (int i = 0; i < idcount; i++)
		{
			if (hideIDs.Contains(ids[i]))
			{
				itemUVs.Add(hideUV);
			}
			else if(highlightIDs.Contains(ids[i]))
			{
				itemUVs.Add(highlightUV);
			}
			else
			{
				itemUVs.Add(defaultUV);
			}
		}

		Vector2[] itemUVArray = itemUVs.ToArray();
		Vector2[] highlightUVs = new Vector2[vertexcount];

		int item = 0;
		foreach (int vectormap in vectorMap)
		{
			highlightUVs[item] = itemUVArray[vectormap];
			item++;
			if (item % 10000 == 0)
			{
				yield return null;
			}
		}
		mesh.uv2 = highlightUVs;
	}


}


