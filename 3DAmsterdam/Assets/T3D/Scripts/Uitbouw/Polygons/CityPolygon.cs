using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public class CityPolygon
    {
        public int[] LocalBoundaries { get; set; }
        public Vector3[] Vertices { get; set; } // used by the CityJSONFormatter to add to the total vertices object
        public int VertOffset { get; set; } //offset to convert LocalBoundaries to AbsoluteBoundaries in the CityJSON object. Must be set while generating JSON since vertices are dynamically collapsed to a single array at that time
        public Dictionary<int, int> LocalToAbsoluteBoundaryConverter { get; set; }  //note: this is only valid when generating the json

        public CityPolygon(Vector3[] vertices, int[] localBoundaries)
        {
            Vertices = vertices;
            LocalBoundaries = localBoundaries;
        }

        public void UpdateVertices(Vector3[] vertices)
        {
            Vertices = vertices;
        }

        public JSONNode GetJSONPolygon()
        {
            //AbsoluteBoundaries = new Dictionary<int, int>();
            //int[] absoluteBoundaries = CityJSONFormatter.ConvertBoundaryIndices(LocalBoundaries, VertOffset);
            int[] absoluteBoundaries = new int[LocalBoundaries.Length];
            for (int i = 0; i < LocalBoundaries.Length; i++)
            {
                absoluteBoundaries[i] = LocalToAbsoluteBoundaryConverter[i];
            }

            var boundaryArray = new JSONArray(); // defines a polygon (1st is surface, 2+ is holes in first surface)
            for (int i = 0; i < absoluteBoundaries.Length; i++)
            {
                boundaryArray.Add(absoluteBoundaries[i]);
            }
            return boundaryArray;
        }
    }
}
