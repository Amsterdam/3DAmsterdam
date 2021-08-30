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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    public class UIQuad : MaskableGraphic
    {
        [SerializeField]
        private Vector2[] quadVertices;
		public Vector2[] QuadVertices { get => quadVertices; set => quadVertices = value; }

        private UIVertex[] uiVertexList = new UIVertex[4];

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            UpdateQuadVertexList();
            vertexHelper.AddUIVertexQuad(uiVertexList);
        }

        public void Redraw()
        {
            SetVerticesDirty();
            CalculateBounds();
        }
        private void CalculateBounds()
        {
            //Make sure our graphic width/height is set to the max distance of our verts, so culling works properly
            //Now based from centered origin
            rectTransform.sizeDelta = new Vector2(
                Mathf.Abs(2 * Mathf.Max(QuadVertices[0].x, QuadVertices[1].x, QuadVertices[2].x, QuadVertices[3].x)),
                Mathf.Abs(2 * Mathf.Min(QuadVertices[0].y, QuadVertices[1].y, QuadVertices[2].y, QuadVertices[3].y))
            );
        }

        private void UpdateQuadVertexList()
        {
            var vert0 = UIVertex.simpleVert;
            var vert1 = UIVertex.simpleVert;
            var vert2 = UIVertex.simpleVert;
            var vert3 = UIVertex.simpleVert;

            vert0.color = color;
            vert1.color = color;
            vert2.color = color;
            vert3.color = color;

            vert0.position = QuadVertices[0];
            vert1.position = QuadVertices[1];
            vert2.position = QuadVertices[2];
            vert3.position = QuadVertices[3];

            uiVertexList[0] = vert0;
            uiVertexList[1] = vert1;
            uiVertexList[2] = vert2;
            uiVertexList[3] = vert3;
        }
    }
}
