using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.ObjectInteraction
{
	public class OutlineObject : MonoBehaviour
	{
		[SerializeField]
		private MeshFilter meshFilter;

		private void Awake()
		{
			//Make sure our position mathes that of our parent transform
			this.transform.localRotation = Quaternion.identity;
			this.transform.localPosition = Vector3.zero;

			//Just use my parent's mesh at initialization
			SetMesh(transform.parent.GetComponent<MeshFilter>().sharedMesh);
		}

		/// <summary>
		/// Swap the mesh used by our outline
		/// </summary>
		/// <param name="mesh">A target shared mesh</param>
		public void SetMesh(Mesh mesh)
		{
			meshFilter.sharedMesh = mesh;
		}
	}
}