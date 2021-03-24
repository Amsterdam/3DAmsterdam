using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using netDxf;
using netDxf.Entities;
using netDxf.Blocks;
using netDxf.Tables;
using ConvertCoordinates;
using System.IO;
using System.Runtime.InteropServices;

public class DxfFile 
{
    private DxfDocument doc;

    private void SetupDXF()
    {
        doc = new DxfDocument();
        doc.DrawingVariables.InsUnits = netDxf.Units.DrawingUnits.Meters;
    }

    public void AddLayer(List<Vector3RD>triangleVertices,string layerName)
    {
        // create Mesh
        PolyfaceMeshVertex[] pfmVertices = new PolyfaceMeshVertex[triangleVertices.Count];
        PolyfaceMeshFace[] pfmFaces = new PolyfaceMeshFace[triangleVertices.Count/3];
        for (int i = 0; i < triangleVertices.Count; i+=3)
        {
            pfmVertices[i] = new PolyfaceMeshVertex(triangleVertices[i].x,triangleVertices[i].y,triangleVertices[i].z);
            pfmFaces[i / 3] = new PolyfaceMeshFace(new List<short>() { (short)i, (short)(i + 1), (short)(i + 2) });

        }
        PolyfaceMesh pfm = new PolyfaceMesh(pfmVertices, pfmFaces);

        //create Layer"
        Layer Laag = new Layer(layerName);
        pfm.Layer = Laag;
        doc.AddEntity(pfm);
        MemoryStream stream = new MemoryStream();
        if (doc.Save(stream))
        {
            DownloadFile(stream.ToArray(), stream.ToArray().Length, "testfile.dxf"); ;
        }
        else
        {
            Debug.Log("cant write file");
        }
        
    }
    [DllImport("__Internal")]
    private static extern void DownloadFile(byte[] array, int byteLength, string fileName);
}
