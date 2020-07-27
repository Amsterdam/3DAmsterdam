using Packages.Rider.Editor.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Sharing{ 
	[System.Serializable]
	public struct SerializableMesh{
		public int meshBitType;
		public SerializableSubMesh[] subMeshes;
		public float[] verts;
		public float[] uvs;
		public float[] normals;
	}
	[System.Serializable]
	public struct SerializableSubMesh
	{
		public string materialId;
		public int[] triangles; //vert ids, per 3
	}
}