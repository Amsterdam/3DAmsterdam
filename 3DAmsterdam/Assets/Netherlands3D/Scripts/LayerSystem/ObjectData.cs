using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.LayerSystem
{
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

		//Our color ID map has certain pixel colors assigned to certain properties in our shader:
		public static Color NEUTRAL_COLOR = Color.green;
		public static Color HIDDEN_COLOR = Color.red;
		public static Color HIGHLIGHTED_COLOR = Color.blue;

		private Vector2Int textureSize;
		public Texture2D colorIDMap;
		public Material instancedMaterial;

		private void OnDestroy()
		{
			//Clean up the color ID map, and the instanced material, if we created it.
			if (colorIDMap) Destroy(colorIDMap);
			if (instancedMaterial) Destroy(instancedMaterial);
		}

		public Color GetUVColorID(Vector2 uvCoordinate)
		{
			if (!colorIDMap) Debug.LogWarning("Cant get UV color ID. There is no color ID map.");

			return colorIDMap.GetPixel(Mathf.FloorToInt(uvCoordinate.x * colorIDMap.width), Mathf.FloorToInt(uvCoordinate.y * colorIDMap.height));
		}

		/// <summary>
		/// Applies the highlighted/hidden lists to their corresponding pixels in the texture map
		/// </summary>
		public void ApplyDataToIDsTexture()
		{
			if (ids == null) return;
			mesh = GetComponent<MeshFilter>().sharedMesh;
			textureSize = ObjectIDMapping.GetTextureSize(ids.Count);

			//Reapply the mesh collider so it has the new proper UV's
			mesh.uv2 = uvs;
			GetComponent<MeshFilter>().sharedMesh = mesh;
			var meshRenderer = GetComponent<MeshRenderer>();

			if (!instancedMaterial)
				instancedMaterial = meshRenderer.material;

			//Create an ID map if it doesnt exist yet (our black placeholder is named "Black")
			colorIDMap = (Texture2D)instancedMaterial.GetTexture("_HighlightMap");
			if (!colorIDMap || colorIDMap.name == "Black")
			{
				colorIDMap = new Texture2D(textureSize.x, textureSize.y, TextureFormat.RGBA32, false);
				colorIDMap.filterMode = FilterMode.Point;
			}

			//Compare out list of ID's with the highlighted, and hidden list, and give their pixels the corresponding color.
			Color pixelColor;
			for (int i = 0; i < ids.Count; i++)
			{
				var myTargetId = ids[i];	

				if (hideIDs.Contains(myTargetId))
				{
					pixelColor = HIDDEN_COLOR;
				}
				else if (highlightIDs.Contains(myTargetId))
				{
					pixelColor = HIGHLIGHTED_COLOR;
				}
				else
				{
					pixelColor = NEUTRAL_COLOR;
				}

				Vector2 uvCoordinate = ObjectIDMapping.GetUV(i, textureSize);
				colorIDMap.SetPixel(Mathf.FloorToInt(uvCoordinate.x * textureSize.x), Mathf.FloorToInt(uvCoordinate.y * textureSize.y), pixelColor);
			}
			colorIDMap.Apply();

			//Apply our texture to the highlightmap slot
			meshRenderer.material.SetTexture("_HighlightMap", colorIDMap);
		}
	}
}