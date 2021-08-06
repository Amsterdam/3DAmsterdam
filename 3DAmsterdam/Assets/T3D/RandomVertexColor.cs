using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomVertexColor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        var filter = GetComponent<MeshFilter>();

        Color[] colors = new Color[filter.mesh.vertexCount];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Random.ColorHSV();
        }

        filter.mesh.colors = colors;


    }

    
}
