using UnityEngine;
using System.Collections;
 
public static class MeshExtensions
{
	public static void BakeChildrenIntoMesh(this Mesh targetMesh, Transform targetTransform)
	{
		Mesh newBakedMesh = targetMesh;

		targetMesh = targetMesh;
	}
}