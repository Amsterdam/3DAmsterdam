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
using UnityEngine.UI;
using UnityEngine.Events;
using Netherlands3D.Core;
using Netherlands3D.Events;

namespace Netherlands3D.Interface
{
	public class GridSelection : Interactable
	{
		[SerializeField]
		private GameObject gridSelectionBlock;

		private Vector3 gridBlockPosition;
		private Vector3Int mouseGridPosition;

		private IAction toggleAction;
		private IAction drawAction;

		[SerializeField]
		private bool drawing = false;

		[SerializeField]
		private Material coloringMaterial;

		private GameObject scaleBlock;
		private Dictionary<Vector3Int, GameObject> selectionBlocks;

		private Vector3Int startGridPosition;

		[Header("Listen to events")]
		[SerializeField] private TriggerEvent requestGridSelection;

		[Header("Trigger events")]
		[SerializeField] private ObjectEvent onGridSelected;
		[SerializeField] private ObjectEvent setMaterialColor;
		[SerializeField] private TriggerEvent abortSelection;
		

		private Coordinate bottomLeftCoordinateVisual;
		private Coordinate topRightCoordinateVisual;

		[SerializeField]
		private MeshRenderer selectionBlockMeshRenderer;

		private bool doneInitialisation = false;

		private void Awake()
		{
			selectionBlocks = new Dictionary<Vector3Int, GameObject>();

			requestGridSelection.AddListenerStarted(StartSelection);
			abortSelection.AddListenerStarted(AbortSelection);
			setMaterialColor.AddListenerStarted(SetMaterialColor);
		}

		private void Start()
		{
			ActionMap = ActionHandler.actions.GridSelection;
			toggleAction = ActionHandler.instance.GetAction(ActionHandler.actions.GridSelection.ToggleVoxel);
			toggleAction.SubscribePerformed(Toggle);

			drawAction = ActionHandler.instance.GetAction(ActionHandler.actions.GridSelection.DrawVoxels);
			drawAction.SubscribePerformed(Drawing);
			drawAction.SubscribeCancelled(Drawing);

			this.transform.position = new Vector3(0, Config.activeConfiguration.zeroGroundLevelY, 0);
			SetGridSize();

			doneInitialisation = true;
		}

		public void StartSelection()
		{
			gameObject.SetActive(true);
			//Fresh start, clear a previous selection block visual
			if (scaleBlock) Destroy(scaleBlock);
		}

		private void Toggle(IAction action)
		{
			if (action.Performed)
			{
				if (Selector.Instance.HoveringInterface()) return;

				startGridPosition = GetGridPosition(CameraModeChanger.Instance.CurrentCameraControls.GetPointerPositionInWorld());

				ScaleSelectionBlockUnderPointer(false);
				Debug.Log("TOGGLE PERFORMED");
				FinishSelection();
			}
		}

		private void Drawing(IAction action)
		{
			if (action.Cancelled)
			{
				drawing = false;
				FinishSelection();
			}
			else if (!Selector.Instance.HoveringInterface() && action.Performed)
			{
				drawing = true;

				startGridPosition = GetGridPosition(CameraModeChanger.Instance.CurrentCameraControls.GetPointerPositionInWorld(Selector.pointerPosition));
				Debug.Log($"Startgrid: {startGridPosition}");
			}
		}

		public void SetMaterialColor(object color)
		{
			coloringMaterial.SetColor("_TintColor", (Color)color);
		}

		private void OnEnable()
		{
			if(!bottomLeftCoordinateVisual && CoordinateNumbers.Instance)
			{
				bottomLeftCoordinateVisual = CoordinateNumbers.Instance.CreateCoordinateNumber();
				topRightCoordinateVisual = CoordinateNumbers.Instance.CreateCoordinateNumber();
			}

			if(bottomLeftCoordinateVisual)
				bottomLeftCoordinateVisual.gameObject.SetActive(true);

			if (topRightCoordinateVisual)
				topRightCoordinateVisual.gameObject.SetActive(true);

			if(VisualGrid.Instance)
				VisualGrid.Instance.Show();

			if(ActionHandler.instance)
				ActionMap = ActionHandler.actions.GridSelection;

			TakeInteractionPriority();
		}

		private void OnDisable()
		{
			if (bottomLeftCoordinateVisual)
				bottomLeftCoordinateVisual.gameObject.SetActive(false);

			if (topRightCoordinateVisual)
				topRightCoordinateVisual.gameObject.SetActive(false);

			if (VisualGrid.Instance)
				VisualGrid.Instance.Hide();

			StopInteraction();
		}

		public override void Escape()
		{
			if (!doneInitialisation) return;

			base.Escape();
			gameObject.SetActive(false);

			abortSelection.InvokeStarted();
		}

		private void AbortSelection()
        {
			base.Escape();
			gameObject.SetActive(false);
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
					ScaleSelectionBlockUnderPointer();			
					selectionBlockMeshRenderer.enabled = false;
				}
				else{
					selectionBlockMeshRenderer.enabled = true;
				}
			}
		}

		private void ScaleSelectionBlockUnderPointer(bool calculateScale = true)
		{
			if (Selector.Instance.HoveringInterface()) return;

			//Just make sure there is one voxel that we can scale
			if (!scaleBlock)
			{
				selectionBlocks.Clear();
				selectionBlocks.Add(mouseGridPosition, Instantiate(gridSelectionBlock, new Vector3(mouseGridPosition.x, mouseGridPosition.y, mouseGridPosition.z), Quaternion.identity, gridSelectionBlock.transform.parent));
				scaleBlock = selectionBlocks.First().Value;
			}
			
			mouseGridPosition = GetGridPosition(CameraModeChanger.Instance.CurrentCameraControls.GetPointerPositionInWorld(Selector.pointerPosition));

			scaleBlock.transform.position = mouseGridPosition;

			if (calculateScale)
			{
				var xDifference = (mouseGridPosition.x - startGridPosition.x);
				var zDifference = (mouseGridPosition.z - startGridPosition.z);

				scaleBlock.transform.position = startGridPosition;
				scaleBlock.transform.Translate(xDifference / 2.0f, 0, zDifference / 2.0f);
				scaleBlock.transform.localScale = new Vector3(
						(mouseGridPosition.x - startGridPosition.x) + ((xDifference < 0 ) ? -VisualGrid.Instance.CellSize : VisualGrid.Instance.CellSize),
						VisualGrid.Instance.CellSize,
						(mouseGridPosition.z - startGridPosition.z) + ((zDifference < 0) ? -VisualGrid.Instance.CellSize : VisualGrid.Instance.CellSize)
				);
			}
			else{
				//Just make sure it is default size
				scaleBlock.transform.localScale = Vector3.one * VisualGrid.Instance.CellSize;
			}

			//Draw coordinate numbers
			var bounds = scaleBlock.GetComponent<MeshRenderer>().bounds;
			DrawCoordinateNumbers(bounds, true);
		}

		private Vector3Int GetGridPosition(Vector3 samplePosition)
		{
			samplePosition.x += (VisualGrid.Instance.CellSize * 0.5f);
			samplePosition.z += (VisualGrid.Instance.CellSize * 0.5f);

			samplePosition.x = (Mathf.Round(samplePosition.x / VisualGrid.Instance.CellSize) * VisualGrid.Instance.CellSize) - (VisualGrid.Instance.CellSize * 0.5f);
			samplePosition.z = (Mathf.Round(samplePosition.z / VisualGrid.Instance.CellSize) * VisualGrid.Instance.CellSize) - (VisualGrid.Instance.CellSize * 0.5f);

			Vector3Int roundedPosition = new Vector3Int
			{
				x = Mathf.RoundToInt(samplePosition.x),
				y = Mathf.RoundToInt(Config.activeConfiguration.zeroGroundLevelY + (VisualGrid.Instance.CellSize * 0.5f)),
				z = Mathf.RoundToInt(samplePosition.z)
			};

			return roundedPosition;
		}

		private void SetGridSize()
		{
			gridSelectionBlock.transform.localScale = new Vector3(VisualGrid.Instance.CellSize, VisualGrid.Instance.CellSize, VisualGrid.Instance.CellSize);
		}

		private void MoveSelectionBlock()
		{
			gridBlockPosition = CameraModeChanger.Instance.CurrentCameraControls.GetPointerPositionInWorld();
			//Offset to make up for grid object origin (centered)
			gridBlockPosition.x += (VisualGrid.Instance.CellSize * 0.5f);
			gridBlockPosition.z += (VisualGrid.Instance.CellSize * 0.5f);

			//Snap block to grid
			gridBlockPosition.x = (Mathf.Round(gridBlockPosition.x / VisualGrid.Instance.CellSize) * VisualGrid.Instance.CellSize) - (VisualGrid.Instance.CellSize * 0.5f);
			gridBlockPosition.z = (Mathf.Round(gridBlockPosition.z / VisualGrid.Instance.CellSize) * VisualGrid.Instance.CellSize) - (VisualGrid.Instance.CellSize * 0.5f);

			gridSelectionBlock.transform.position = gridBlockPosition;
			gridSelectionBlock.transform.Translate(Vector3.up * (VisualGrid.Instance.CellSize * 0.5f));
			DrawCoordinateNumbers(gridSelectionBlock.GetComponent<MeshRenderer>().bounds, true);
		}

		private void DrawCoordinateNumbers(Bounds bounds, bool zeroHeight = false)
		{
			var min = bounds.min;
			if(zeroHeight) min.y = 0;

			var max = bounds.max;
			if (zeroHeight) max.y = 0;

			if (bottomLeftCoordinateVisual)
				bottomLeftCoordinateVisual.DrawCoordinate(min);

			if (topRightCoordinateVisual)
				topRightCoordinateVisual.DrawCoordinate(max);
		}

		private void FinishSelection()
		{
			if (scaleBlock)
			{
				var bounds = scaleBlock.GetComponent<MeshRenderer>().bounds;
				Debug.Log("bbox="+(bounds.min.x + Config.activeConfiguration.RelativeCenterRD.x)+"," + (bounds.min.z + Config.activeConfiguration.RelativeCenterRD.y) + "," + (bounds.max.x + Config.activeConfiguration.RelativeCenterRD.x) + ","+(bounds.max.z + Config.activeConfiguration.RelativeCenterRD.y));
				onGridSelected.InvokeStarted(scaleBlock.GetComponent<MeshRenderer>().bounds);
			}
		}
	}
}