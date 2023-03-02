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

namespace Netherlands3D.Visualisers
{
    public class PointCloudVisualiser : MonoBehaviour
    {
        [SerializeField]
        private Vector3ListEvent receiveDrawPointsEvent;

        [SerializeField]
        private Material pointCloudMaterial;

        void Awake()
        {
            if (receiveDrawPointsEvent) receiveDrawPointsEvent.AddListenerStarted(DrawPointsInWorld);
        }

        private void DrawPointsInWorld(List<Vector3> positions)
        {
            var indices = new int[positions.Count];
			for (int i = 0; i < positions.Count; i++)
			{
                indices[i] = i;
            }

            Mesh pointCloudMesh = new Mesh();
            pointCloudMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            pointCloudMesh.vertices = positions.ToArray();
            pointCloudMesh.SetIndices(indices,MeshTopology.Points,0);

            gameObject.AddComponent<MeshFilter>().mesh = pointCloudMesh;
            gameObject.AddComponent<MeshRenderer>().material = pointCloudMaterial;
        }
    }
}