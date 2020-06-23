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
		private Handle handleXMin;
		[SerializeField]
		private Handle handleXPlus;
		[SerializeField]
		private Handle handleZMin;
		[SerializeField]
		private Handle handleZPlus;

		[SerializeField]
		private Handle handleY;

		[SerializeField]
		private Transform floorOrigin;

		private Vector3[] shapeVertices;

		[SerializeField]
		private float defaultSize = 1.0f;

		private void Start()
		{
			shapeVertices = shape.sharedMesh.vertices;
			for (int i = 0; i < shapeVertices.Length; i++)
			{
				shapeVertices[i] = shapeVertices[i] * defaultSize;
			}
			shape.sharedMesh.SetVertices(shapeVertices);
		}

		private void UpdateShapeVerts(){
			//Using Unity's internal vertex order indices for the Cube mesh. 
			//Note that 3 verts share the same location, because the cube is flat shaded.
			OverrideVertPosition(new int[] { 16, 14, 1 }, handleXMin.transform.localPosition.x, floorOrigin.localPosition.y, handleZPlus.transform.localPosition.z);
			OverrideVertPosition(new int[] { 19, 15, 7 }, handleXMin.transform.localPosition.x, floorOrigin.localPosition.y, handleZMin.transform.localPosition.z);
			OverrideVertPosition(new int[] { 17, 9, 3 }, handleXMin.transform.localPosition.x, handleY.transform.localPosition.y, handleZPlus.transform.localPosition.z);
			OverrideVertPosition(new int[] { 18, 11, 5 }, handleXMin.transform.localPosition.x, handleY.transform.localPosition.y, handleZMin.transform.localPosition.z);
			
			OverrideVertPosition(new int[] { 22, 8, 2 }, handleXPlus.transform.localPosition.x, handleY.transform.localPosition.y, handleZPlus.transform.localPosition.z);
			OverrideVertPosition(new int[] { 23, 13, 0 }, handleXPlus.transform.localPosition.x, floorOrigin.localPosition.y, handleZPlus.transform.localPosition.z);
			OverrideVertPosition(new int[] { 20, 12, 6 }, handleXPlus.transform.localPosition.x, floorOrigin.localPosition.y, handleZMin.transform.localPosition.z);
			OverrideVertPosition(new int[] { 21, 10, 4 }, handleXPlus.transform.localPosition.x, handleY.transform.localPosition.y, handleZMin.transform.localPosition.z);

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

		public void ScaleShape()
		{
			shape.transform.localPosition = new Vector3(
				Mathf.Lerp(handleXMin.transform.localPosition.x, handleXPlus.transform.localPosition.x, 0.5f),
				Mathf.Lerp(-handleY.transform.localPosition.y, handleY.transform.localPosition.y, 0.5f),
				Mathf.Lerp(handleZMin.transform.localPosition.z, handleZPlus.transform.localPosition.z, 0.5f)
			);
	
			/*shape.transform.localScale = new Vector3(
				Vector3.Distance(handleXMin.transform.localPosition, handleXPlus.transform.localPosition) - handleXPlus.StartOffset.x*2.0f,
				Vector3.Distance(-handleY.transform.localPosition, handleY.transform.localPosition) - handleY.StartOffset.y * 2.0f,
				Vector3.Distance(handleXMin.transform.localPosition, handleXPlus.transform.localPosition)  - handleXPlus.StartOffset.x*2.0f
			);*/
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
