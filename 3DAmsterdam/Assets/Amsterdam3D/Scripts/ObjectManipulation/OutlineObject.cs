using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineObject : MonoBehaviour
{
	[SerializeField]
	private MeshFilter meshFilter;

	private void Awake()
	{
		this.transform.localRotation = Quaternion.identity;
		this.transform.localPosition = Vector3.zero;

		//Just use my parent's mesh at initialization
		SetMesh(transform.parent.GetComponent<MeshFilter>().mesh);
	}

	public void SetMesh(Mesh mesh)
	{
		meshFilter.sharedMesh = mesh;
	}
}
