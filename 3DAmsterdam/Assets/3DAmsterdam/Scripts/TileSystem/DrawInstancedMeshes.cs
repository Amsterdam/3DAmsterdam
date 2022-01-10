using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class DrawInstancedMeshes : MonoBehaviour
{
	private MaterialPropertyBlock block;
	private Matrix4x4[] matrices;

	[SerializeField]
	private List<Matrix4x4[]> splitMatrices;
	[SerializeField]
	private List<Vector4[]> splitUvs;

	int pointCount;

	[SerializeField]
	private int textureRows = 6;

	private Mesh mesh;
	private Material material;

	private Mesh[] instanceableMeshes;

	private CommandBuffer buffer;

	private void Awake()
	{
		block = new MaterialPropertyBlock();
	}

	public void DrawInstancedMeshesOnPoints(Mesh pointsMesh,Material material, Mesh[] instanceableMeshes)
	{
		this.mesh = pointsMesh;
		this.material = material;
		this.instanceableMeshes = instanceableMeshes;

		pointCount = pointsMesh.vertexCount;
		Vector3[] vertices = pointsMesh.vertices;

		//Filter to fake points based dataset
		List<Vector3> filteredVertices = new List<Vector3>();
		for (int i = 0; i < pointCount; i+=24)
		{
			filteredVertices.Add(vertices[i]);
			filteredVertices.Add(vertices[i]);
		}
		vertices = filteredVertices.ToArray();
		var mainScale = Vector3.one * 2;
		var allMatrices = new Matrix4x4[vertices.Length];
		var allUvOffsets = new Vector4[vertices.Length];

		//For testing purposes skip about the length of a single obj test geometry
		for (int i = 0; i < vertices.Length; i++)
		{
			var position = vertices[i] + this.transform.position;
			var rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
			var scale = mainScale;

			var matrix = Matrix4x4.TRS(position, rotation, scale);
			allMatrices[i] = matrix;

			//Fill out UV coordinates (based on type input)
			var randomXSheet = (1.0f / textureRows) * Random.Range(0, textureRows);
			var randomYSheet = (1.0f / textureRows) * Random.Range(0, textureRows);
			allUvOffsets[i] = new Vector4(randomXSheet, randomYSheet,0,0);
		}

		//Limit of 1023 per DrawMeshInstanced call. So split array into limit. 
		splitMatrices = allMatrices.Select((x, i) => new { Index = i, Value = x })
			.GroupBy(x => x.Index / 1023)
			.Select(x => x.Select(v => v.Value).ToArray())
			.ToList();

		splitUvs = allUvOffsets.Select((x, i) => new { Index = i, Value = x })
			.GroupBy(x => x.Index / 1023)
			.Select(x => x.Select(v => v.Value).ToArray())
			.ToList();
	}

	private void Update()
	{
		for (int i = 0; i < splitMatrices.Count; i++)
		{
			var matrices = splitMatrices[i];
			var uvs = splitUvs[i];

			block.SetVectorArray("_Offsets", uvs);
			Graphics.DrawMeshInstanced(instanceableMeshes[0], 0, material, matrices, matrices.Length, block);
		}
	}
}