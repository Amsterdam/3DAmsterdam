/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Netherlands3D.Events
{
    /// <summary>
    /// Use countour lists as input to draw solidified 2D shapes in the 3D world
    /// </summary>
    public class PolygonVisualiser : MonoBehaviour
    {
        [Header("Listen to events:")]
        [SerializeField]
        private Vector3ListsEvent drawPolygonEvent;
        [SerializeField]
        private StringEvent setDrawingObjectName;
        [SerializeField]
        private FloatEvent setExtrusionHeightEvent;

        [SerializeField]
        private Material defaultMaterial;

        [SerializeField]
        private float extrusionHeight = 100.0f;

        private int maxPolygons = 10000;
        private int polygonCount = 0;

        private string currentObjectName = "";

        [SerializeField]
        private bool setUVCoordinates = false;
        [SerializeField]
        private Vector2 uvCoordinate = Vector2.zero;

        [SerializeField]
        private bool reverseWindingOrder = true;

        [SerializeField]
        private bool addColliders = false;

        [Header("Invoke events")]
        [SerializeField]
        GameObjectEvent createdPolygonGameObject;

        void Awake()
        {
            if (setDrawingObjectName) setDrawingObjectName.started.AddListener(SetName);
            if (drawPolygonEvent) drawPolygonEvent.started.AddListener(CreatePolygon);
            if (setExtrusionHeightEvent) setExtrusionHeightEvent.started.AddListener(SetExtrusionHeight);
        }

		public void SetExtrusionHeight(float extrusionHeight)
		{
            this.extrusionHeight = extrusionHeight;
        }

        public void SetName(string drawingObjectName)
        {
            currentObjectName = drawingObjectName;
        }

        private void CreatePolygon(List<IList<Vector3>> contours)
        {
            CreateAndReturnPolygon(contours);
        }

        //Treat first contour as outer contour, and extra contours as holes
        public GameObject CreateAndReturnPolygon(List<IList<Vector3>> contours)
        {
            if (polygonCount >= maxPolygons) return null;
            polygonCount++;

            var polygon = new Poly2Mesh.Polygon();
            var outerContour = (List<Vector3>)contours[0];
            FixSequentialDoubles(outerContour);
            if (outerContour.Count < 3) return null;

            if (reverseWindingOrder) outerContour.Reverse();

            polygon.outside = outerContour;

            for (int i = 0; i < polygon.outside.Count; i++)
            {
                polygon.outside[i] = new Vector3(polygon.outside[i].x, polygon.outside[i].y, polygon.outside[i].z);
            }

            if (contours.Count > 1)
            {
                for (int i = 1; i < contours.Count; i++)
                {
                    var holeContour = (List<Vector3>)contours[i];
                    FixSequentialDoubles(holeContour);
                    
                    if (holeContour.Count > 2)
                    {
                        if (reverseWindingOrder) holeContour.Reverse();
                        polygon.holes.Add(holeContour);
                    }
                }
            }
            var newPolygonMesh = Poly2Mesh.CreateMesh(polygon, extrusionHeight);
            if (newPolygonMesh) newPolygonMesh.RecalculateNormals();

            if (setUVCoordinates)
            {
                SetUVCoordinates(newPolygonMesh);
            }

            var newPolygonObject = new GameObject();
#if UNITY_EDITOR
            //Do not bother setting object name outside of Editor untill we need it.
            newPolygonObject.name = currentObjectName;
#endif
            newPolygonObject.AddComponent<MeshFilter>().sharedMesh = newPolygonMesh;
            newPolygonObject.AddComponent<MeshRenderer>().material = defaultMaterial;
            if (addColliders)
                newPolygonObject.AddComponent<MeshCollider>().sharedMesh = newPolygonMesh;

            newPolygonObject.transform.SetParent(this.transform);
            newPolygonObject.transform.Translate(0, extrusionHeight, 0);

            if (createdPolygonGameObject) createdPolygonGameObject.Invoke(newPolygonObject);
            return newPolygonObject;
        }

        /// <summary>
        /// Poly2Mesh has problems with polygons that have points in the same position.
        /// Lets move them a bit.
        /// </summary>
        /// <param name="contour"></param>
        private static void FixSequentialDoubles(List<Vector3> contour)
        {
            var removedSomeDoubles = false;
            for (int i = contour.Count - 2; i >= 0; i--)
            {
                if (contour[i] == contour[i + 1])
                {
                    contour.RemoveAt(i+1);
                    removedSomeDoubles = true;
                }
            }
            if (removedSomeDoubles) Debug.Log("Removed some doubles");
        }

        private void SetUVCoordinates(Mesh newPolygonMesh)
		{
			var uvs = new Vector2[newPolygonMesh.vertexCount];
			for (int i = 0; i < uvs.Length; i++)
			{
				uvs[i] = uvCoordinate;
			}
			newPolygonMesh.uv = uvs;
		}
	}
}