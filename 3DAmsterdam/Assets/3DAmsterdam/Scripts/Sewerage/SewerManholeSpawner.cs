using Netherlands3D.TileSystem;
using UnityEngine;

namespace Amsterdam3D.Sewerage
{
	public class SewerManholeSpawner : MonoBehaviour
	{
		[SerializeField]
		private LayerMask sewerHeightCheckLayerMask;

		[SerializeField]
		private LayerMask terrainHeightCheckLayerMask;
		[SerializeField]
		private BinaryMeshLayer terrainLayer;
		[SerializeField]
		private SewerageObjectPool manHoleObjectPool;
		public GameObject manholePrefab;

		private const float maxRayCastDistance = 50.0f;

		private RaycastHit hit;

		/// <summary>
		/// Create a Manhole (rioolput)
		/// uses TerrainHeight for elevation as default, uses y-value of param:position if no terrainheight found
		/// requiers sewerpipes to be present for accurate depth
		/// </summary>
		/// <param name="position">Unity-coordinates of the top-center of the manhole</param>
		/// <param name="manholePrefab">GameObject with manholeTemplate, default height=1, origin = top-center, local x-direction is down</param>
		/// <param name="defaultDepth">default manhole-depth if no sewerpipes are found</param>
		/// <returns></returns>
		public GameObject CreateManhole(Vector3 position, float defaultDepth = 1.50f, GameObject parent = null)
		{
			GameObject ParentObject = parent;
			if (parent == null)
			{
				ParentObject = transform.gameObject;
			}
			// get top-center position
			Vector3 lidPosition = position;
			// get manhole-height
			float depth = GetDepthAtPosition(lidPosition, defaultDepth);
			// create manhole
			GameObject manHole = manHoleObjectPool.GetPooledObject();
			manHole.SetActive(true);
			manHole.transform.parent = ParentObject.transform;
			// move manhole
			manHole.transform.localPosition = lidPosition;
			// scale manhole to correct height
			Vector3 scale = manHole.transform.localScale;
			scale.y = depth;
			manHole.transform.localScale = scale;
			// return manhole
			return manHole;
		}
		/// <summary>
		/// Get the position of top-center of the manhole, prioritize on terrain-layer
		/// </summary>
		/// <param name="position">Vector3 theoretical position of manhole according to WFS</param>
		/// <returns>Vector3 top-center position of the manhole</returns>
		private Vector3 GetPositionAtSurface(Vector3 position)
		{
			Vector3 foundPosition = new Vector3();

			// set RaycastOrigin to 10 above theoretical manhole-position
			Vector3 rayCastPosition = position + new Vector3(0, 10, 0);
			terrainLayer.AddMeshColliders(rayCastPosition);
			if (Physics.Raycast(rayCastPosition, Vector3.down, out hit, maxRayCastDistance, terrainHeightCheckLayerMask.value))
			{
				// set foundposition to hitpoint if terrain is found
				foundPosition = hit.point;
				// lift position a little bit for better visibility
				foundPosition += new Vector3(0, 0.02f, 0);
			}
			else
			{
				// keep theoretical position if terrain is not found
				foundPosition = position;
			}
			return foundPosition;

		}
		/// <summary>
		/// Get the required depth of the manhole
		/// </summary>
		/// <param name="position">Vector3 top-center postion of manhole</param>
		/// <param name="defaultDepth">Default depth if to sewerpipes are detected</param>
		/// <returns>required height of manhole</returns>
		private float GetDepthAtPosition(Vector3 position, float defaultDepth)
		{
			float height = defaultDepth;

			if (Physics.Raycast(position + Vector3.down * 20.0f, Vector3.up, out hit, maxRayCastDistance, sewerHeightCheckLayerMask.value))
			{
				// if sewerpipe found, depth is distance between top-center and hit.point plus 0.5m for bottom-part of manhole
				height = (position.y - hit.point.y) + 0.5f;
			}

			return height;
		}
	}
}