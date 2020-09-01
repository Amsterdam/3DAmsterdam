using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMappingClass : ScriptableObject
{
    public List<string> ids;
    public List<int> triangleCount;
    public List<Vector2> mappedUVs;
    public List<int> vectorMap;
    public Vector2[] uvs;
    public Vector2Int TextureSize;
}
