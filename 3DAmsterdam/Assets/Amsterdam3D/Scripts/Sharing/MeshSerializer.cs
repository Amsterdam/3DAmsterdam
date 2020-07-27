using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshSerializer : MonoBehaviour
{
    
    public static float[] FlattenVector3Array(Vector3[] array)
    {
        float[] flatArray = new float[array.Length*3];
        for(var i = 0; i<array.Length; i+=3){
            flatArray[i] = array[i].x;
            flatArray[i+1] = array[i].y;
            flatArray[i+2] = array[i].z;
        }
        return flatArray;
    }

    public static float[] FlattenVector2Array(Vector2[] array)
    {
        float[] flatArray = new float[array.Length * 2];
        for (var i = 0; i < array.Length; i += 2)
        {
            flatArray[i] = array[i].x;
            flatArray[i + 1] = array[i].y;
        }
        return flatArray;
    }


}
