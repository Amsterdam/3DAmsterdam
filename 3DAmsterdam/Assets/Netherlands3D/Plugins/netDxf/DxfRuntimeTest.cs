using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;

public class DxfRuntimeTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        List<Vector3RD> verts = new List<Vector3RD>();
        verts.Add(new Vector3RD(0, 0, 0));
        verts.Add(new Vector3RD(0, 1, 0));
        verts.Add(new Vector3RD(0, 0, 1));
        verts.Add(new Vector3RD(0, 0, 0));
        verts.Add(new Vector3RD(0, 1, 0));
        verts.Add(new Vector3RD(0, 0, 1));
        verts.Add(new Vector3RD(0, 0, 0));
        verts.Add(new Vector3RD(0, 1, 0));
        verts.Add(new Vector3RD(0, 0, 1));

        DxfFile file = new DxfFile();
        file.SetupDXF();
        file.AddLayer(verts,"testlaag");
        file.Save();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
