using Netherlands3D.Cameras;
using Netherlands3D.InputHandler;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Netherlands3D.Interface.SidePanel;

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

		private IAction toggleAction;
		private IAction drawAction;
		private IAction clearAction;

		private bool drawing = false;
		private bool add = true;

		private Dictionary<Vector3Int, GameObject> voxels;
		private int maxVoxels = 200;

		[SerializeField]
		private bool freePaint = false;
		private Vector3Int startVoxel;

		private void Awake()
		{
			ActionMap = ActionHandler.actions.GridSelection;

			toggleAction = ActionHandler.instance.GetAction(ActionHandler.actions.GridSelection.ToggleVoxel);
			toggleAction.SubscribePerformed(Toggle);

			drawAction = ActionHandler.instance.GetAction(ActionHandler.actions.GridSelection.DrawVoxels);
			drawAction.SubscribePerformed(Drawing);
			drawAction.SubscribeCancelled(Drawing);

			clearAction = ActionHandler.instance.GetAction(ActionHandler.actions.GridSelection.EraseVoxels);
			clearAction.SubscribePerformed(Clear);
			clearAction.SubscribeCancelled(Clear);

			voxels = new Dictionary<Vector3Int, GameObject>();
		}

		void Start()
		{
			this.transform.position = new Vector3(0, Config.activeConfiguration.zeroGroundLevelY, 0);
			SetGridSize();
		}
		private void Toggle(IAction action)
		{
			print("Tap toggle voxel");
			DrawVoxelUnderMouse(true);
		}
		private void Drawing(IAction action)
		{
			if (action.Cancelled)
			{
				drawing = false;
				FinishSelection();
			}
			else if (action.Performed)
			{
				drawing = true;
				add = true;
				if (freePaint)
				{
					startVoxel = GetVoxel(CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld());
					MoveSelectionBlock();
				}
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
				gridSelectionBlock.SetActive(false);
			}
			else
			{
				gridSelectionBlock.SetActive(true);
				MoveSelectionBlock();
				if (drawing)
				{
					if (freePaint)
					{
						DrawVoxelUnderMouse();
					}
					else
					{
						ScaleSingleVoxelUnderMouse();
					}
						
				}
			}
		}

		private void DrawVoxelUnderMouse(bool toggled = false)
		{
			var mouseGridPosition = GetVoxel(CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld());
			if (!voxels.ContainsKey(mouseGridPosition) && add && voxels.Count < maxVoxels)
			{
				voxels.Add(mouseGridPosition, Instantiate(gridSelectionBlock, new Vector3(mouseGridPosition.x, mouseGridPosition.y, mouseGridPosition.z), Quaternion.identity, gridSelectionBlock.transform.parent));
			}
			else if ((toggled || !add) && voxels.ContainsKey(mouseGridPosition))
			{
				Destroy(voxels[mouseGridPosition]);
				voxels.Remove(mouseGridPosition);
			}
		}
		private void ScaleSingleVoxelUnderMouse(bool toggled = false)
		{
			var mouseGridPosition = GetVoxel(CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld());
			gridSelectionBlock.transform.position = startVoxel;
			gridSelectionBlock.transform.Translate((mouseGridPosition.x - startVoxel.x) / 2.0f,	0,(mouseGridPosition.z - startVoxel.z) / 2.0f);
			gridSelectionBlock.transform.localScale = new Vector3(
					((mouseGridPosition.x - startVoxel.x) / 2.0f) * gridSize,
					gridSize,
					((mouseGridPosition.z - startVoxel.z) / 2.0f) * gridSize
			);
		}


		private Vector3Int GetVoxel(Vector3 voxelPosition)
		{
			voxelPosition.x += (gridSize * 0.5f);
			voxelPosition.z += (gridSize * 0.5f);

			voxelPosition.x = (Mathf.Round(voxelPosition.x / gridSize) * gridSize) - (gridSize * 0.5f);
			voxelPosition.z = (Mathf.Round(voxelPosition.z / gridSize) * gridSize) - (gridSize * 0.5f);

			Vector3Int mouseGridPosition = new Vector3Int
			{
				x = Mathf.RoundToInt(voxelPosition.x),
				y = Mathf.RoundToInt(Config.activeConfiguration.zeroGroundLevelY + (gridSize * 0.5f)),
				z = Mathf.RoundToInt(voxelPosition.z)
			};

			return mouseGridPosition;
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

		private void FinishSelection()
		{
			//TODO: send this boundingbox to the mesh selection logic, and draw the sidepanel
			PropertiesPanel.Instance.OpenPanel("Grid selectie", true);
			PropertiesPanel.Instance.AddActionCheckbox("Gebouwen", true, (action) =>
			{

			});
			PropertiesPanel.Instance.AddActionCheckbox("Bomen", true, (action) =>
			{

			});
			PropertiesPanel.Instance.AddActionCheckbox("Maaiveld", true, (action) =>
			{

			});
			PropertiesPanel.Instance.AddActionCheckbox("Ondergrond", true, (action) =>
			{

			});
			PropertiesPanel.Instance.AddActionButtonBig("Downloaden", (action) =>
			{
				//Do the download action
			});
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