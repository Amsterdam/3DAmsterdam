using UnityEngine;
using System.Collections;
 
public static class TransformExtensions
{
	/// <summary>
	/// Returns a transform point world scale coordinates inside a transform but ignores the scale of the transform
	/// </summary>
	/// <param name="transform">Transform with the coordinate system our point lives in</param>
	/// <param name="position">The transform point location within our transform</param>
	/// <returns>Location of this point in world space</returns>
	public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
	{
	 var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
	 return localToWorldMatrix.MultiplyPoint3x4(position);
	}

	/// <summary>
	/// Inverse transform point calculation that ignores the scale
	/// </summary>
	/// <param name="transform">Transform with the coordinate system</param>
	/// <param name="position">The position relative to the unscaled coordinate system</param>
	/// <returns>The position relative to the unscaled coordinate system</returns>
	public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
	{
	 var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
	 return worldToLocalMatrix.MultiplyPoint3x4(position);
	}
}