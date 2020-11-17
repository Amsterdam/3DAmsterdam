using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using LayerSystem;
using Amsterdam3D.CameraMotion;

namespace Amsterdam3D.Interface
{
	// currently works as MVP, still has a bunch of TODOs for better usage.

	//TODO: Move Single click tool to this class, or to a selection tool?
	public class SelectionTools : MonoBehaviour
	{
		[SerializeField]
		private GameObject canvas;
		[SerializeField]
		private SelectionTool selectionTool;

		[SerializeField]
		private Bounds bounds;
		private List<Vector3> vertices;

		[SerializeField]
		private TileHandler tileHandler;

		[SerializeField]
		private LayerMask buildingLayer;


		[SerializeField]
		private Layer layer;

		private Vector3 groundLevel = new Vector3(0, Constants.ZERO_GROUND_LEVEL_Y, 0);

		private void Start()
		{
			tileHandler = FindObjectOfType<TileHandler>();

			selectionTool.canvas = canvas;
			selectionTool.onSelectionCompleted.AddListener(OnSelectionFunction);
			selectionTool.EnableTool();
		}
		public Bounds GetBounds()
		{
			return bounds;
		}
		// NOTE: Only checks for X and Z positions, Y isn't taken into account
		public bool Contains(Vector3 position)
		{
			return selectionTool.ContainsPoint(position);
		}

		public ToolType GetCurrentToolType()
		{
			return selectionTool.toolType;
		}

		public List<Vector3> GetVertices()
		{
			// copy selection and return copy
			List<Vector3> returnValue = new List<Vector3>();
			returnValue.AddRange(selectionTool.vertices);
			return returnValue;
		}

		private void OnSelectionFunction()
		{
			vertices = GetVertices();

			float extends = Vector3.Distance(selectionTool.vertices[0], selectionTool.vertices[1]) *0.5f;
			layer.AddMeshColliders();
			StartCoroutine(BoxCastToFindTilesInRange(GetPointsCentroid(), new Vector3(extends, extends, extends)));
		}

		private Vector3 GetPointsCentroid()
		{
			if (vertices.Count <= 0) return default;

			Vector3 centroid = new Vector3(0, 0, 0);
			foreach (var point in vertices)
			{
				centroid += point;
			}

			centroid /= vertices.Count;

			return centroid;
		}

		private IEnumerator BoxCastToFindTilesInRange(Vector3 center, Vector3 extends)
		{
			//We wait one frame to make sure the colliders are there.
			yield return new WaitForEndOfFrame();
			var hits = Physics.BoxCastAll(center, extends, Vector3.down, Quaternion.identity, center.y + 1000.0f, buildingLayer);
			foreach (var hit in hits)
			{
				Debug.Log("HIT " + hit.collider.name);
				tileHandler.GetIDData(hit.collider.gameObject, hit.triangleIndex * 3);
			}
		}
	}

	public enum ToolType
	{
		Invalid,
		Box,
		Polygon,
		Circle
	}
}