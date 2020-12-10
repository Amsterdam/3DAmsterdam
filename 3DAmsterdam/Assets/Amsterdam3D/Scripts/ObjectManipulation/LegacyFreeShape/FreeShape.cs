using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Amsterdam3D.FreeShape
{
	[RequireComponent(typeof(Draggable))]
	public class FreeShape : MonoBehaviour
	{
		[SerializeField]
		private MeshFilter shape;

		[Header("Scaling handles")]
		[SerializeField]
		private Transform handleXMin;
		[SerializeField]
		private Transform handleXPlus;
		[SerializeField]
		private Transform handleZMin;
		[SerializeField]
		private Transform handleZPlus;

		[SerializeField]
		private Transform handleY;

		[SerializeField]
		private Transform floorOrigin;

		[Header("Rotation handles")]
		[SerializeField]
		private Transform rotationHandle1;
		[SerializeField]
		private Transform rotationHandle2;
		[SerializeField]
		private Transform rotationHandle3;
		[SerializeField]
		private Transform rotationHandle4;

		private Vector3[] shapeVertices;

		private Mesh customMesh;
		private MeshCollider meshCollider;

		private ScaleHandle[] scaleHandles;

		[SerializeField]
		private float numberScreenSize = 10.0f;
		[SerializeField]
		private float shapeScreenStartScale = 0.0f;

		public Transform FloorOrigin { get => floorOrigin; set => floorOrigin = value; }

		[SerializeField]
		private TextMesh shapeHeightText;

		[SerializeField]
		private TextMesh shapeDepthText;

		[SerializeField]
		private TextMesh shapeLengthText;

		private void Awake()
		{
			meshCollider = shape.GetComponent<MeshCollider>();
			scaleHandles = GetComponentsInChildren<ScaleHandle>();
			shape = GetComponent<MeshFilter>();

			CreateCustomMesh();
			FitScaleHandesInView();
			UpdateShape();
			ApplyShappeOriginOffset();

			HideAllHandles();
			DisplayHandles(true);
		}

		private void Update()
		{
			ShowEdgeLengthNumbers();
		}

		public void OnMouseDown()
		{
			HideAllHandles();
			DisplayHandles(true);
		}

		private void OnDestroy()
		{
			Destroy(customMesh);
			Destroy(GetComponent<MeshRenderer>().material);
		}

		private static void HideAllHandles()
		{
			FreeShape[] shapes = FindObjectsOfType<FreeShape>();
			foreach (FreeShape freeShape in shapes)
				freeShape.DisplayHandles(false);
		}

		private void FitScaleHandesInView()
		{
			if (shapeScreenStartScale == 0.0f)
				return;
			
			foreach(ScaleHandle handle in scaleHandles){
				handle.transform.localPosition = handle.transform.localPosition * (CameraModeChanger.Instance.ActiveCamera.transform.position.y) * shapeScreenStartScale;
			}
		}

		private void DisplayHandles(bool display)
		{
			//All children are handles, so simply activate those.
			foreach (Transform child in transform)
				child.gameObject.SetActive(display);
		}

		private void CreateCustomMesh()
		{
			customMesh = new Mesh();
			customMesh.vertices = shape.sharedMesh.vertices;
			customMesh.normals = shape.sharedMesh.normals;
			customMesh.triangles = shape.sharedMesh.triangles;
			shape.mesh = customMesh;
			shapeVertices = customMesh.vertices;
		}

		private void ApplyShappeOriginOffset()
		{
			this.transform.Translate(0.0f, -FloorOrigin.localPosition.y, 0.0f);
		}

		private void UpdateShapeVerts()
		{
			//Here we set the vert position axes to their corresponding handle positions
			//Using the arbitrary internal vertex order indices for the Unity Cube mesh. 
			//Note that 3 verts share the same location, because the cube is flat shaded.
			OverrideVertPosition(new int[] { 16, 14, 1 }, handleXMin.localPosition.x, FloorOrigin.localPosition.y, handleZPlus.localPosition.z);
			OverrideVertPosition(new int[] { 19, 15, 7 }, handleXMin.localPosition.x, FloorOrigin.localPosition.y, handleZMin.localPosition.z);
			OverrideVertPosition(new int[] { 17, 9, 3 }, handleXMin.localPosition.x, handleY.transform.localPosition.y, handleZPlus.localPosition.z);
			OverrideVertPosition(new int[] { 18, 11, 5 }, handleXMin.localPosition.x, handleY.transform.localPosition.y, handleZMin.localPosition.z);

			OverrideVertPosition(new int[] { 22, 8, 2 }, handleXPlus.localPosition.x, handleY.transform.localPosition.y, handleZPlus.localPosition.z);
			OverrideVertPosition(new int[] { 21, 10, 4 }, handleXPlus.localPosition.x, handleY.transform.localPosition.y, handleZMin.localPosition.z);
			OverrideVertPosition(new int[] { 23, 13, 0 }, handleXPlus.localPosition.x, FloorOrigin.localPosition.y, handleZPlus.localPosition.z);
			OverrideVertPosition(new int[] { 20, 12, 6 }, handleXPlus.localPosition.x, FloorOrigin.localPosition.y, handleZMin.localPosition.z);

			customMesh.SetVertices(shapeVertices);
			customMesh.RecalculateBounds();
		}

		private void ShowEdgeLengthNumbers()
		{
			DrawEdgeLengthNumberText(shapeHeightText, shapeVertices[21], shapeVertices[20]);
			DrawEdgeLengthNumberText(shapeDepthText, shapeVertices[18], shapeVertices[21]);
			DrawEdgeLengthNumberText(shapeLengthText, shapeVertices[8], shapeVertices[21]);
		}

		private void DrawEdgeLengthNumberText(TextMesh numberTextMesh, Vector3 fromPosition, Vector3 toPosition)
		{
			numberTextMesh.text = Vector3.Distance(fromPosition, toPosition).ToString("F2") + "m";
			numberTextMesh.transform.localPosition = Vector3.Lerp(fromPosition, toPosition, 0.5f);

			//Always turn to camera
			numberTextMesh.transform.rotation = CameraModeChanger.Instance.ActiveCamera.transform.rotation;

			if (numberScreenSize > 0)
				numberTextMesh.transform.localScale = Vector3.one * Vector3.Distance(CameraModeChanger.Instance.ActiveCamera.transform.position, numberTextMesh.transform.position) * numberScreenSize;
		}

		private void OverrideVertPosition(int[] arrayPositions, float newX = 0.0f, float newY = 0.0f, float newZ = 0.0f)
		{
			foreach (int index in arrayPositions)
			{
				shapeVertices[index] = new Vector3(
					(newX != 0) ? newX : shapeVertices[index].x,
					(newY != 0) ? newY : shapeVertices[index].y,
					(newZ != 0) ? newZ : shapeVertices[index].z
				);
			}
		}

		public void UpdateShape(ScaleHandle controllingHandle = null)
		{
			UpdateShapeVerts();

			if (controllingHandle)
			{
				AlignOtherScaleHandles(controllingHandle);
			}

			//Move rotation handles onto the side edges
			rotationHandle1.localPosition = Vector3.Lerp(shapeVertices[16], shapeVertices[17], 0.5f);
			rotationHandle2.localPosition = Vector3.Lerp(shapeVertices[20], shapeVertices[21], 0.5f);
			rotationHandle3.localPosition = Vector3.Lerp(shapeVertices[18], shapeVertices[19], 0.5f);
			rotationHandle4.localPosition = Vector3.Lerp(shapeVertices[22], shapeVertices[23], 0.5f);

			meshCollider.sharedMesh = customMesh;
		}

		private void AlignOtherScaleHandles(ScaleHandle ignoreHandle)
		{
			var center = GetCenterFromOppositeHandle(ignoreHandle);
			foreach (ScaleHandle handle in scaleHandles)
			{
				if (handle.AxisConstraint != ignoreHandle.AxisConstraint)
				{
					handle.transform.localPosition = new Vector3(
						(handle.AxisConstraint.x != 0) ? handle.transform.localPosition.x : center.x,
						(handle.AxisConstraint.y != 0) ? handle.transform.localPosition.y : center.y,
						(handle.AxisConstraint.z != 0) ? handle.transform.localPosition.z : center.z
					);
				}
			}
		}

		private Vector3 GetCenterFromOppositeHandle(ScaleHandle fromHandle)
		{
			foreach (ScaleHandle handle in scaleHandles)
			{
				if (handle != fromHandle && handle.AxisConstraint == fromHandle.AxisConstraint)
				{
					var center = Vector3.Lerp(handle.transform.localPosition, fromHandle.transform.localPosition, 0.5f);
					UnityEngine.Debug.DrawLine(center, center + Vector3.up);
					return center;
				}
			}
			return Vector3.zero;
		}
	}
}
