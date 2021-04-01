using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMappingClass : ScriptableObject
{
    public List<string> ids; //List of all Bag ID's in this tile
    public Vector2[] uvs;
    public List<int> vectorMap;
}