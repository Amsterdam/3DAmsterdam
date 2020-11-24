using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectData : MonoBehaviour
{
	public List<string> highlightIDs = new List<string>();
	public List<string> ids;
	public List<string> hideIDs = new List<string>();
	public Vector2[] uvs;


	/// <summary>
	/// The vectormap contains a list of numbers, with a number for every vertex. This number refers to the index in the ids list.
	/// So vectorMap[9] would contain the index for the ids' list, with the object ID for vertices[9].
	/// </summary>
	public List<int> vectorMap;
	public List<Vector2> mappedUVs;
	public Mesh mesh;
	public List<int> triangleCount;

	private Texture2D colorIDMap;

	private void OnDestroy()
	{
		if (colorIDMap) Destroy(colorIDMap);	
	}

	/// <summary>
	/// Applies the highlighted/hidden lists to their corresponding pixels in the texture map
	/// </summary>
	public void ApplyDataToIDsTexture()
	{
		if (ids == null) return;

		mesh = GetComponent<MeshFilter>().sharedMesh;

		Vector2Int textureSize = ObjectIDMapping.GetTextureSize(ids.Count);

		//Reapply the mesh collider so it has the new proper UV's
		mesh.uv2 = uvs;
		GetComponent<MeshFilter>().sharedMesh = mesh;
		var meshRenderer = GetComponent<MeshRenderer>();

		//Create an ID map if it doesnt exist yet, force a instance of the material to be created
		colorIDMap = (Texture2D)meshRenderer.material.GetTexture("_HighLightMap");
		if (!colorIDMap)
		{
			colorIDMap = new Texture2D(textureSize.x, textureSize.y, TextureFormat.RGBA32, false);
			colorIDMap.filterMode = FilterMode.Point;
		}

		//Compare out list of ID's with the highlighted, and hidden list, and give their pixels the corresponding color.
		Color pixelColor;
		for (int i = 0; i < ids.Count; i++)
		{
			var targetId = ids[i];
			
			if(hideIDs.Contains(targetId))
			{
				//RED pixels are hidden in the shader
				pixelColor = Color.red;
			}
			else if(highlightIDs.Contains(targetId))
			{
				//BLUE pixels are highlighted in the shader
				pixelColor = Color.blue;
			}
			else{
				//GREEN is default
				pixelColor = Color.green;
			}
			
			Vector2 uvCoordinate = ObjectIDMapping.GetUV(i, textureSize);
			colorIDMap.SetPixel(Mathf.FloorToInt(uvCoordinate.x * textureSize.x), Mathf.FloorToInt(uvCoordinate.y * textureSize.y), pixelColor);
		}
		
		colorIDMap.Apply();

		//Apply our texture to the highlightmap slot
		meshRenderer.material.SetTexture("_HighLightMap", colorIDMap);		
	}
}


