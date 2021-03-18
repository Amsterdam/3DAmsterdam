using Netherlands3D.Cameras;
using Netherlands3D.InputHandler;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Netherlands3D.Interface.SidePanel;
using System.Linq;

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
		private Vector3Int mouseGridPosition;

		private IAction toggleAction;
		private IAction drawAction;
		private IAction clearAction;

		private bool drawing = false;
		private bool add = true;

		private Dictionary<Vector3Int, GameObject> voxels;
		private int maxVoxels = 200;

		[SerializeField]
		private bool freePaint = false;
		private Vector3Int startGridPosition;

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
			if (freePaint)
			{
				DrawVoxelsUnderMouse(true);
			}
			else{
				ScaleSingleVoxelUnderMouse(false);
			}

			FinishSelection();
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
				if (!freePaint)
				{
					startGridPosition = GetGridPosition(CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld());
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
						DrawVoxelsUnderMouse();
					}
					else
					{
						ScaleSingleVoxelUnderMouse();
					}				
				}
			}
		}

		private void DrawVoxelsUnderMouse(bool toggled = false)
		{
			if (Selector.Instance.HoveringInterface()) return;
			
			mouseGridPosition = GetGridPosition(CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld());
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

		private void ScaleSingleVoxelUnderMouse(bool calculateScale = true)
		{
			if (Selector.Instance.HoveringInterface()) return;

			//Just make sure there is one voxel that we can scale
			if (voxels.Count == 0)
			{
				voxels.Add(mouseGridPosition, Instantiate(gridSelectionBlock, new Vector3(mouseGridPosition.x, mouseGridPosition.y, mouseGridPosition.z), Quaternion.identity, gridSelectionBlock.transform.parent));
			}

			var scaleBlock = voxels.First().Value;
			mouseGridPosition = GetGridPosition(CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld());		
			scaleBlock.transform.position = mouseGridPosition;

			if (calculateScale)
			{
				var xDifference = (mouseGridPosition.x - startGridPosition.x);
				var zDifference = (mouseGridPosition.z - startGridPosition.z);
				print("X " + xDifference);
				print("Z " + zDifference);

				scaleBlock.transform.position = startGridPosition;
				scaleBlock.transform.Translate(xDifference / 2.0f, 0, zDifference / 2.0f);
				scaleBlock.transform.localScale = new Vector3(
						(mouseGridPosition.x - startGridPosition.x) + ((xDifference < 0 ) ? -gridSize : gridSize),
						gridSize,
						(mouseGridPosition.z - startGridPosition.z) + ((zDifference < 0) ? -gridSize : gridSize)
				);
			}
			else{
				//Just make sure it is default size
				scaleBlock.transform.localScale = Vector3.one * gridSize;
			}
		}

		private Vector3Int GetGridPosition(Vector3 samplePosition)
		{
			samplePosition.x += (gridSize * 0.5f);
			samplePosition.z += (gridSize * 0.5f);

			samplePosition.x = (Mathf.Round(samplePosition.x / gridSize) * gridSize) - (gridSize * 0.5f);
			samplePosition.z = (Mathf.Round(samplePosition.z / gridSize) * gridSize) - (gridSize * 0.5f);

			Vector3Int roundedPosition = new Vector3Int
			{
				x = Mathf.RoundToInt(samplePosition.x),
				y = Mathf.RoundToInt(Config.activeConfiguration.zeroGroundLevelY + (gridSize * 0.5f)),
				z = Mathf.RoundToInt(samplePosition.z)
			};

			return roundedPosition;
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