using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Amsterdam3D.FreeShape
{
	public class FreeShape : MonoBehaviour
	{
		[SerializeField]
		private MeshFilter shape;

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

		private Vector3[] shapeVertices;

		[SerializeField]
		private float margin = 10.0f;

		private void Start()
		{
			shapeVertices = shape.sharedMesh.vertices;
			
		}

		private void UpdateShapeVerts(){
			//Here we set the vert position axes to their corresponding handle positions
			//Using the arbitrary internal vertex order indices for the Unity Cube mesh. 
			//Note that 3 verts share the same location, because the cube is flat shaded.
			OverrideVertPosition(new int[] { 16, 14, 1 }, handleXMin.localPosition.x - margin, floorOrigin.localPosition.y, handleZPlus.localPosition.z + margin);
			OverrideVertPosition(new int[] { 19, 15, 7 }, handleXMin.localPosition.x - margin, floorOrigin.localPosition.y, handleZMin.localPosition.z - margin);
			OverrideVertPosition(new int[] { 17, 9, 3 }, handleXMin.localPosition.x - margin, handleY.transform.localPosition.y + margin, handleZPlus.localPosition.z + margin);
			OverrideVertPosition(new int[] { 18, 11, 5 }, handleXMin.localPosition.x - margin, handleY.transform.localPosition.y + margin, handleZMin.localPosition.z - margin);
			
			OverrideVertPosition(new int[] { 22, 8, 2 }, handleXPlus.localPosition.x + margin, handleY.transform.localPosition.y + margin, handleZPlus.localPosition.z + margin);
			OverrideVertPosition(new int[] { 21, 10, 4 }, handleXPlus.localPosition.x + margin, handleY.transform.localPosition.y + margin, handleZMin.localPosition.z - margin);
			OverrideVertPosition(new int[] { 23, 13, 0 }, handleXPlus.localPosition.x + margin, floorOrigin.localPosition.y, handleZPlus.localPosition.z + margin);
			OverrideVertPosition(new int[] { 20, 12, 6 }, handleXPlus.localPosition.x + margin, floorOrigin.localPosition.y, handleZMin.localPosition.z - margin);

			shape.sharedMesh.SetVertices(shapeVertices);
		}

		private void OverrideVertPosition(int[] arrayPositions, float newX = 0.0f, float newY = 0.0f, float newZ = 0.0f){
			foreach (int index in arrayPositions)
			{
				shapeVertices[index] = new Vector3(
					(newX != 0) ? newX : shapeVertices[index].x,
					(newY != 0) ? newY : shapeVertices[index].y,
					(newZ != 0) ? newZ : shapeVertices[index].z
				);
			}
		}

		private void Update()
		{
			UpdateShapeVerts();
		}

		private void OnDrawGizmos()
		{
			for (int i = 0; i < shape.sharedMesh.vertices.Length; i++)
			{
				Handles.Label(this.transform.position + shape.sharedMesh.vertices[i] + Vector3.up* i, "<b>vert:"+i+"</b>",new GUIStyle() { richText = true });
			}
		}
	}
}
