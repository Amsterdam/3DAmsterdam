using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.ObjectInteraction
{
	public class VisualGrid : MonoBehaviour, IUniqueService
	{
		[SerializeField]
		private Material gridMaterial;

		private float gridPlaneMeshSize = 10.0f;

		[SerializeField]
		private float cellSize = 100.0f;
		public float CellSize { get => cellSize; }

		private void Start()
		{
			UpdateVisual();
			Hide();
		}

		private void OnValidate()
		{
			SetGridSize(CellSize);
		}

		public void SetGridSize(float gridSize = 0)
		{
			cellSize = gridSize;
			UpdateVisual();
		}

		private void UpdateVisual()
		{
			gridMaterial.SetTextureScale("_MainTex", Vector2.one * (gridPlaneMeshSize * this.transform.localScale.x / cellSize));
		}

		public void Show()
		{
			gameObject.SetActive(true);
		}

		public void Hide()
		{
			gameObject.SetActive(false);
		}
	}
}