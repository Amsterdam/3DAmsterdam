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
    public class PolygonVisualiser : MonoBehaviour
    {
        [Header("Listen to events:")]
        [SerializeField]
        private Vector3ListsEvent drawPolygonEvent;
        [SerializeField]
        private FloatEvent setExtrusionHeightEvent;

        [SerializeField]
        private Material defaultMaterial;

        [SerializeField]
        private float extrusionHeight = 100.0f;

        private int maxPolygons = 10000;
        private int polygonCount = 0;

        [SerializeField]
        private bool setUVCoordinates = false;
        [SerializeField]
        private Vector2 uvCoordinate = Vector2.zero;

        void Awake()
        {
            if (drawPolygonEvent) drawPolygonEvent.started.AddListener(CreatePolygon);
            if (setExtrusionHeightEvent) setExtrusionHeightEvent.started.AddListener(SetExtrusionHeight);
        }

		public void SetExtrusionHeight(float extrusionHeight)
		{
            this.extrusionHeight = extrusionHeight;
        }

		//Treat first contour as outer contour, and extra contours as holes
		public void CreatePolygon(List<IList<Vector3>> contours)
        {
            if (polygonCount >= maxPolygons) return;
            polygonCount++;

            var polygon = new Poly2Mesh.Polygon();
            polygon.outside = (List<Vector3>)contours[0];

            for (int i=0; i< polygon.outside.Count; i++)
            {
                polygon.outside[i] = new Vector3(polygon.outside[i].x, polygon.outside[i].y, polygon.outside[i].z);
            }

            if (contours.Count > 1)
            {
                for (int i = 1; i < contours.Count; i++)
                {
                    polygon.holes.Add((List<Vector3>)contours[i]);
                }
            }
            var newPolygonMesh = Poly2Mesh.CreateMesh(polygon, extrusionHeight);
            if(newPolygonMesh) newPolygonMesh.RecalculateNormals();

            if(setUVCoordinates)
			{
				SetUVCoordinates(newPolygonMesh);
			}

			var newPolygonObject = new GameObject();
            newPolygonObject.name = name;
            newPolygonObject.AddComponent<MeshFilter>().sharedMesh = newPolygonMesh;
            newPolygonObject.AddComponent<MeshRenderer>().material = defaultMaterial;
            newPolygonObject.AddComponent<MeshCollider>();
            newPolygonObject.transform.SetParent(this.transform);
            newPolygonObject.transform.Translate(0, extrusionHeight, 0);
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