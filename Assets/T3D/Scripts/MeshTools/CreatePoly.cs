using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

public class CreatePoly : MonoBehaviour
{
	public Material material;
	public bool m_FlipNormals = false;

	ProBuilderMesh m_Mesh;

	void Start()
	{		
		var go = new GameObject();
		m_Mesh = go.gameObject.AddComponent<ProBuilderMesh>();
		go.GetComponent<MeshRenderer>().material = material;
		
		

		Build();
	}

	void Build()
	{
		// Create a circle of points with randomized distance from origin.
		List<Vector3> verts = new List<Vector3>();
		verts.Add(new Vector3(0, 0, 0));
		verts.Add(new Vector3(0, 0.25f, 1));
		verts.Add(new Vector3(1, 0.5f, 1));
		verts.Add(new Vector3(1, 0.75f, 0));



		// CreateShapeFromPolygon is an extension method that sets the pb_Object mesh data with vertices and faces
		// generated from a polygon path.
		m_Mesh.CreateShapeFromPolygon(verts.ToArray(), 1, m_FlipNormals);
	}
}
