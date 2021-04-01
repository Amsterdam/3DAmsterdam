using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadUVData : MonoBehaviour
{
    public ObjectMappingClass objectMapping;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshFilter>().mesh.uv = objectMapping.uvs;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
