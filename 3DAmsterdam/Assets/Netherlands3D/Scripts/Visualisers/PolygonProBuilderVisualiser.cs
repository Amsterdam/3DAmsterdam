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
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace Netherlands3D.Visualisers
{
    public class PolygonProBuilderVisualiser : MonoBehaviour
    {
        [SerializeField]
        private Vector3ListsEvent drawPolygonEvent;

        [SerializeField]
        private Material defaultMaterial;

        [SerializeField]
        private float extrusionHeight = 100.0f;

        void Start()
        {
            if (drawPolygonEvent) drawPolygonEvent.started.AddListener(CreatePolygon);
        }

        //Treat first contour as outer contour, and extra contours as holes
        public void CreatePolygon(List<IList<Vector3>> contours)
        {
            ProBuilderMesh probuilderObject = new GameObject().AddComponent<ProBuilderMesh>();
            probuilderObject.CreateShapeFromPolygon(contours[0], extrusionHeight, false, contours.GetRange(1, contours.Count - 1));
            probuilderObject.GetComponent<MeshRenderer>().material = defaultMaterial;
            probuilderObject.transform.SetParent(this.transform);
        }
    }
}