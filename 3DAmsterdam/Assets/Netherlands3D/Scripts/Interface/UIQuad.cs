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
            
            //Make sure our graphic width/height is set to the max distance of our verts, so culling works properly
            /*rectTransform.sizeDelta = new Vector2(
                Mathf.Abs(Mathf.Max(QuadVertices[0].x, QuadVertices[1].x, QuadVertices[2].x, QuadVertices[3].x)),
                Mathf.Abs(Mathf.Max(QuadVertices[0].y, QuadVertices[1].y, QuadVertices[2].y, QuadVertices[3].y))
            );*/
        }

        private void UpdateQuadVertexList()
        {
            var vert0 = UIVertex.simpleVert;
            var vert1 = UIVertex.simpleVert;
            var vert2 = UIVertex.simpleVert;
            var vert3 = UIVertex.simpleVert;

            vert0.color = color;
            vert1.color = color;
            vert2.color = Color.clear;
            vert3.color = Color.clear;

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
