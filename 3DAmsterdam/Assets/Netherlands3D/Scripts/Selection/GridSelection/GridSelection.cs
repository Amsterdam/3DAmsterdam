using Netherlands3D.Cameras;
using Netherlands3D.InputHandler;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Netherlands3D.Interface
{
	public class GridSelection : Interactable
	{
		[SerializeField]
		private Material gridMaterial;

		[SerializeField]
		private GameObject gridSelectionBlock;

		[SerializeField]
		private float gridSize = 100.0f; //Meter

		[SerializeField]
		private float gridPlaneSize = 10000.0f;

		private Vector3 gridBlockPosition;

		private Vector3 voxelPosition;

		private IAction drawAction;
		private IAction clearAction;

		private bool drawing = false;
		private bool add = true;

		private Dictionary<Vector3Int, GameObject> voxels;

		private Vector3Int mouseGridPosition;

		private int maxVoxels = 200;

		void Start()
		{
			mouseGridPosition = new Vector3Int();
			voxels = new Dictionary<Vector3Int, GameObject>();

			this.transform.position = new Vector3(0, Config.activeConfiguration.zeroGroundLevelY, 0);
			SetGridSize();

			ActionMap = ActionHandler.actions.GridSelection;

			drawAction = ActionHandler.instance.GetAction(ActionHandler.actions.GridSelection.Draw);
			drawAction.SubscribePerformed(Drawing);
			drawAction.SubscribeCancelled(Drawing);

			clearAction = ActionHandler.instance.GetAction(ActionHandler.actions.GridSelection.ClearDrawing);
			clearAction.SubscribePerformed(Clear);
			clearAction.SubscribeCancelled(Clear);
		}

		private void Drawing(IAction action)
		{
			if (action.Cancelled)
			{
				drawing = false;
			}
			else if (action.Performed)
			{
				drawing = true;
				add = true;
			}
		}
		private void Clear(IAction action)
		{
			if (action.Cancelled)
			{
				drawing = false;
			}
			else if (action.Performed)
			{
				drawing = true;
				add = false;
			}
		}

		private void OnEnable()
		{
			TakeInteractionPriority();
		}
		private void OnDisable()
		{
			StopInteraction();
		}

		private void Update()
		{
			if (Selector.Instance.HoveringInterface())
			{
				//Hide block
			}
			else
			{
				MoveSelectionBlock();
				if (drawing)
				{
					DrawVoxelUnderMouse();
				}
			}
		}

		private void DrawVoxelUnderMouse()
		{
			voxelPosition = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
			voxelPosition.x += (gridSize * 0.5f);
			voxelPosition.z += (gridSize * 0.5f);

			voxelPosition.x = (Mathf.Round(voxelPosition.x / gridSize) * gridSize) - (gridSize * 0.5f);
			voxelPosition.z = (Mathf.Round(voxelPosition.z / gridSize) * gridSize) - (gridSize * 0.5f);

			mouseGridPosition.x = Mathf.RoundToInt(voxelPosition.x);
			mouseGridPosition.y = Mathf.RoundToInt(gridSize * 0.5f);
			mouseGridPosition.z = Mathf.RoundToInt(voxelPosition.z);

			if (!voxels.ContainsKey(mouseGridPosition) && add && voxels.Count < maxVoxels)
			{
				voxels.Add(mouseGridPosition, Instantiate(gridSelectionBlock, new Vector3(mouseGridPosition.x, mouseGridPosition.y, mouseGridPosition.z), Quaternion.identity, gridSelectionBlock.transform.parent));
			}
			else if (!add && voxels.ContainsKey(mouseGridPosition))
			{
				Destroy(voxels[mouseGridPosition]);
				voxels.Remove(mouseGridPosition);
			}
		}

		private void MoveSelectionBlock()
		{
			gridBlockPosition = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
			//Offset to make up for grid object origin (centered)
			gridBlockPosition.x += (gridSize * 0.5f);
			gridBlockPosition.z += (gridSize * 0.5f);

			//Snap block to grid
			gridBlockPosition.x = (Mathf.Round(gridBlockPosition.x / gridSize) * gridSize) - (gridSize * 0.5f);
			gridBlockPosition.z = (Mathf.Round(gridBlockPosition.z / gridSize) * gridSize) - (gridSize * 0.5f);

			gridSelectionBlock.transform.position = gridBlockPosition;
			gridSelectionBlock.transform.Translate(Vector3.up * (gridSize * 0.5f));
		}

		private void SetGridSize()
		{
			gridMaterial.SetTextureScale("_MainTex", Vector2.one * (gridPlaneSize / (gridSize * 0.1f)));
		}

		public void OnValidate()
		{
			SetGridSize();
		}

	}
}