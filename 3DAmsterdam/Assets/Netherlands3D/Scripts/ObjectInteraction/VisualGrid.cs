using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.ObjectInteraction
{
	public class VisualGrid : MonoBehaviour
	{
		public static VisualGrid Instance;

		[SerializeField]
		private Material gridMaterial;

		private float gridPlaneMeshSize = 10.0f;

		[SerializeField]
		private float cellSize = 100.0f;
		public float CellSize { get => cellSize; }

		[SerializeField]
		private float largeCellSize = 1000.0f;
		public float LargeCellSize { get => largeCellSize; }

		[SerializeField]
		private MeshRenderer meshRenderer;

		void Awake()
		{
			Instance = this;
		}

		public void Show()
		{
			meshRenderer.enabled = true;
		}

		public void Hide()
		{
			meshRenderer.enabled = false;
		}
	}
}