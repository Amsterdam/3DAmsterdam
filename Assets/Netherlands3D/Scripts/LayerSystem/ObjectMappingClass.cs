using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMappingClass : ScriptableObject
{
    public List<string> ids; //List of all Bag ID's in this tile
    public Vector2[] uvs; //List of all vert UV coordinates
    public List<int> vectorMap; //List of all uv/verts belonging to what bag ID in ids
    public List<int> objectVertIndexCount; //List of all uv/verts belonging to what bag ID in ids

    public List<int> ToOldVectorMap()
    {
        var oldVectorMap = new List<int>();
        var newVmapCount = 0;

        for (var i = 0; i < vectorMap.Count; i++)
        {
            var firstIndex = vectorMap[i];
            var vicount = objectVertIndexCount[i];

            for (int v = 0; v < vicount; v++)
            {
                oldVectorMap.Add(i);
                newVmapCount++;
            }
        }
        return oldVectorMap;
    }

}