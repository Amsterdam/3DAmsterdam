using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Netherlands3D.TileSystem;
using Netherlands3D.Cameras;
using Netherlands3D.Core;

namespace Netherlands3D.Interface.Selection
{
	// currently works as MVP, still has a bunch of TODOs for better usage.

	//TODO: Move Single click tool to this class, or to a selection tool?
	public class SelectionTools : MonoBehaviour
	{
		private GameObject canvas;
		[SerializeField]
		private SelectionTool selectionTool;

		[SerializeField]
		private Bounds bounds;
		private List<Vector3> vertices;

		private TileHandler tileHandler;

		[SerializeField]
		private Layer[] selectableLayers;

		[SerializeField]
		private LayerMask buildingLayer;


		private Vector3 selectionCentroid;

		private void Start()
		{
			canvas = FindObjectOfType<Canvas>().gameObject;
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

			GetPointsCentroid();
			var maxDistance = 0.0f;
			foreach (var position in vertices)
			{
				var distance = Vector3.Distance(position, selectionCentroid);
				if (distance > maxDistance)
				{
					maxDistance = distance;
				}
			}

			//foreach (AssetbundleMeshLayer layer in selectableLayers)
			//{
			//	foreach (Transform tile in layer.transform)
			//	{
			//		if (Vector3.Distance(tile.transform.position, selectionCentroid) < (maxDistance + layer.tileSize))
			//		{
			//			layer.GetIDData(tile.gameObject, 0);
			//		}
			//	}
			//}
		}

		private Vector3 GetPointsCentroid()
		{
			if (vertices.Count <= 0) return default;

			selectionCentroid = new Vector3(0, 0, 0);
			foreach (var point in vertices)
			{
				selectionCentroid += point;
			}

			selectionCentroid /= vertices.Count;

			return selectionCentroid;
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