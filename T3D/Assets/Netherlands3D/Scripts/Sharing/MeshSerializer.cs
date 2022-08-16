using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Netherlands3D.Sharing
{
    public class MeshSerializer
    {
        public static float[] FlattenVector3Array(Vector3[] array)
        {
            float[] flatArray = new float[array.Length * 3];
            for (var i = 0; i < array.Length; i++)
            {
                flatArray[i*3] = array[i].x;
                flatArray[(i*3) + 1] = array[i].y;
                flatArray[(i*3) + 2] = array[i].z;
            }
            return flatArray;
        }
        public static Vector3[] SeperateVector3Array(float[] array)
        {
            Vector3[] seperatedArray = new Vector3[array.Length / 3];
            for (var i = 0; i < seperatedArray.Length; i ++)
            {
                seperatedArray[i] = new Vector3(array[i*3], array[(i * 3) + 1], array[(i * 3) + 2]);
            }
            return seperatedArray;
        }

        public static float[] FlattenVector2Array(Vector2[] array)
        {
            float[] flatArray = new float[array.Length * 2];
            for (var i = 0; i < array.Length; i ++)
            {
                flatArray[i*2] = array[i].x;
                flatArray[(i*2) + 1] = array[i].y;
            }
            return flatArray;
        }
        public static Vector2[] SeperateVector2Array(float[] array)
        {
            Vector2[] seperatedArray = new Vector2[array.Length / 2];
            for (var i = 0; i < seperatedArray.Length; i++)
            {
                seperatedArray[i] = new Vector2(array[i * 2], array[(i * 2) + 1]);
            }
            return seperatedArray;
        }
    }
}
