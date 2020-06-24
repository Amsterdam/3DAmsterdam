using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Amsterdam3D.FreeShape
{
	public class FreeShape : ObjectManipulation
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

		[SerializeField]
		private float margin = 10.0f;

		private Mesh customMesh;
		private MeshCollider collider;

		private ScaleHandle[] handles;

		public Transform FloorOrigin { get => floorOrigin; set => floorOrigin = value; }

		[SerializeField]
		private TextMesh shapeHeightText;

		[SerializeField]
		private TextMesh shapeDepthText;

		[SerializeField]
		private TextMesh shapeLengthText;

		private Vector3 clickOffset;

		private void Start()
		{
			handles = GetComponentsInChildren<ScaleHandle>();
			collider = shape.GetComponent<MeshCollider>();

			CreateCustomMesh();
			UpdateShape();

			ApplyOriginOffset();

			DisplayHandles(true);
		}

		private void Update()
		{
			TextLengthBetweenVerts(shapeHeightText, shapeVertices[21], shapeVertices[20]);
			TextLengthBetweenVerts(shapeDepthText, shapeVertices[18], shapeVertices[21]);
			TextLengthBetweenVerts(shapeLengthText, shapeVertices[8], shapeVertices[21]);
		}

		public override void OnMouseDown()
		{
			base.OnMouseDown();
			FreeShape[] shapes = FindObjectsOfType<FreeShape>();
			foreach (FreeShape freeShape in shapes)
				freeShape.DisplayHandles(false);

			clickOffset = GetWorldPositionOnPlane(Input.mousePosition, this.transform.position.y) - this.transform.position;

			DisplayHandles(true);
		}

		private void OnMouseDrag()
		{
			this.transform.position = GetWorldPositionOnPlane(Input.mousePosition, this.transform.position.y) - clickOffset;
		}

		private void DisplayHandles(bool display)
		{
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

		private void ApplyOriginOffset()
		{
			this.transform.Translate(0.0f, -FloorOrigin.localPosition.y, 0.0f);
		}

		private void UpdateShapeVerts()
		{
			//Here we set the vert position axes to their corresponding handle positions
			//Using the arbitrary internal vertex order indices for the Unity Cube mesh. 
			//Note that 3 verts share the same location, because the cube is flat shaded.
			OverrideVertPosition(new int[] { 16, 14, 1 }, handleXMin.localPosition.x - margin, FloorOrigin.localPosition.y, handleZPlus.localPosition.z + margin);
			OverrideVertPosition(new int[] { 19, 15, 7 }, handleXMin.localPosition.x - margin, FloorOrigin.localPosition.y, handleZMin.localPosition.z - margin);
			OverrideVertPosition(new int[] { 17, 9, 3 }, handleXMin.localPosition.x - margin, handleY.transform.localPosition.y + margin, handleZPlus.localPosition.z + margin);
			OverrideVertPosition(new int[] { 18, 11, 5 }, handleXMin.localPosition.x - margin, handleY.transform.localPosition.y + margin, handleZMin.localPosition.z - margin);

			OverrideVertPosition(new int[] { 22, 8, 2 }, handleXPlus.localPosition.x + margin, handleY.transform.localPosition.y + margin, handleZPlus.localPosition.z + margin);
			OverrideVertPosition(new int[] { 21, 10, 4 }, handleXPlus.localPosition.x + margin, handleY.transform.localPosition.y + margin, handleZMin.localPosition.z - margin);
			OverrideVertPosition(new int[] { 23, 13, 0 }, handleXPlus.localPosition.x + margin, FloorOrigin.localPosition.y, handleZPlus.localPosition.z + margin);
			OverrideVertPosition(new int[] { 20, 12, 6 }, handleXPlus.localPosition.x + margin, FloorOrigin.localPosition.y, handleZMin.localPosition.z - margin);

			customMesh.SetVertices(shapeVertices);
			customMesh.RecalculateBounds();
		}

		private void TextLengthBetweenVerts(TextMesh textMesh, Vector3 vertA, Vector3 vertB)
		{
			textMesh.text = Vector3.Distance(vertA, vertB).ToString("F2") + "m";
			textMesh.transform.localPosition = Vector3.Lerp(vertA, vertB, 0.5f);

			var lookatTarget = new Vector3(Camera.main.transform.position.x, textMesh.transform.position.y, Camera.main.transform.position.z);
			textMesh.transform.LookAt(lookatTarget);
			textMesh.transform.Rotate(0, 180.0f, 0);
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
			rotationHandle1.localPosition = Vector3.Lerp(shapeVertices[16], shapeVertices[17],0.5f);
			rotationHandle2.localPosition = Vector3.Lerp(shapeVertices[20], shapeVertices[21], 0.5f);
			rotationHandle3.localPosition = Vector3.Lerp(shapeVertices[18], shapeVertices[19], 0.5f);
			rotationHandle4.localPosition = Vector3.Lerp(shapeVertices[22], shapeVertices[23], 0.5f);

			collider.sharedMesh = customMesh;
		}

		private void AlignOtherScaleHandles(ScaleHandle ignoreHandle)
		{
			var center = GetCenterFromOppositeHandle(ignoreHandle);
			foreach (ScaleHandle handle in handles)
			{
				if (handle.Axis != ignoreHandle.Axis)
				{
					handle.transform.localPosition = new Vector3(
						(handle.Axis.x != 0) ? handle.transform.localPosition.x : center.x,
						(handle.Axis.y != 0) ? handle.transform.localPosition.y : center.y,
						(handle.Axis.z != 0) ? handle.transform.localPosition.z : center.z
					);
				}
			}
		}

		private Vector3 GetCenterFromOppositeHandle(ScaleHandle fromHandle)
		{
			foreach (ScaleHandle handle in handles)
			{
				if (handle != fromHandle && handle.Axis == fromHandle.Axis)
				{
					var center = Vector3.Lerp(handle.transform.localPosition, fromHandle.transform.localPosition, 0.5f);
					UnityEngine.Debug.DrawLine(center, center + Vector3.up);
					return center;
				}
			}
			return Vector3.zero;
		}
		#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (!customMesh) return;
			for (int i = 0; i < customMesh.vertices.Length; i++)
			{
				Handles.Label(this.transform.position + customMesh.vertices[i] + Vector3.up * i, "<b>vert:" + i + "</b>", new GUIStyle() { richText = true });
			}
		}
		#endif
	}
}
