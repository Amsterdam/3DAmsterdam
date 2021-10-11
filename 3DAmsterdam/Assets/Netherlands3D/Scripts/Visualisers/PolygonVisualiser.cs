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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonVisualiser : MonoBehaviour
{
    [SerializeField]
    private Vector3ListsEvent drawPolygonEvent;

    [SerializeField]
    private Material defaultMaterial;

    [SerializeField]
    private float extrusionHeight = 100.0f;

    void Start()
    {
       if(drawPolygonEvent) drawPolygonEvent.unityEvent.AddListener(CreatePolygon);
    }

    //Treat first contour as outer contour, and extra contours as holes
    public void CreatePolygon(List<IList<Vector3>> contours)
    {
        var polygon = new Poly2Mesh.Polygon();
        polygon.outside = (List<Vector3>)contours[0];
        if(contours.Count > 1)
        {
			for (int i = 1; i < contours.Count; i++)
			{
                polygon.holes.Add((List<Vector3>)contours[i]);
            }
		}
        var newPolygonMesh = Poly2Mesh.CreateMesh(polygon);
        newPolygonMesh.RecalculateNormals();

        //Add rim

        var newPolygonObject = new GameObject();
        newPolygonObject.AddComponent<MeshFilter>().sharedMesh = newPolygonMesh;
        newPolygonObject.AddComponent<MeshRenderer>().material = defaultMaterial;
	}
}
