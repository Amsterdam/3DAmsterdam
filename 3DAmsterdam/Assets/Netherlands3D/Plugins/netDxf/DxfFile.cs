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

    public void SetupDXF()
    {
        doc = new DxfDocument();
        doc.DrawingVariables.InsUnits = netDxf.Units.DrawingUnits.Meters;
    }

    public void AddLayer(List<Vector3RD>triangleVertices,string layerName)
    {
        // TODO 
        // check if there are 3 triangles or less, if that is the case a polyfaceMesh cannot be built, seperate triangles have to be added to the dxf.

        // create Mesh
        List<PolyfaceMeshVertex> pfmVertices = new List<PolyfaceMeshVertex>();
           pfmVertices.Capacity= triangleVertices.Count;
        List<PolyfaceMeshFace> pfmFaces = new List<PolyfaceMeshFace>();
        pfmFaces.Capacity = triangleVertices.Count/3;
        int facecounter = 0;
        for (int i = 0; i < triangleVertices.Count; i+=3)
        {
            
            pfmVertices.Add(new PolyfaceMeshVertex(triangleVertices[i].x,triangleVertices[i].y,triangleVertices[i].z));
            PolyfaceMeshFace pfmFace = new PolyfaceMeshFace(new List<short>() { (short)i, (short)(i + 1), (short)(i + 2) });
            
            pfmFaces.Add(pfmFace);
            facecounter++;
        }
        PolyfaceMesh pfm = new PolyfaceMesh(pfmVertices, pfmFaces);


        //create Layer"
        Layer Laag = new Layer(layerName);
        pfm.Layer = Laag;
        Laag.Color = netDxf.AciColor.LightGray;
        
        doc.AddEntity(pfm);
        
        
    }
    public void Save()
    {
#if UNITY_EDITOR

        doc.Save("D:/testDXF.dxf");
        return;
#endif
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

    public void AddTriangles(List<Vector3RD> triangleVertices, string layerName)
    {
        netDxf.Vector3 vertex1 = new netDxf.Vector3(triangleVertices[0].x, triangleVertices[0].y, triangleVertices[0].z);
        netDxf.Vector3 vertex2 = new netDxf.Vector3(triangleVertices[1].x, triangleVertices[1].y, triangleVertices[1].z);
        netDxf.Vector3 vertex3 = new netDxf.Vector3(triangleVertices[2].x, triangleVertices[2].y, triangleVertices[2].z);
        Face3d face = new Face3d(vertex1, vertex2, vertex3);
        Layer laag = new Layer(layerName);
        face.Layer = laag;
        doc.AddEntity(face);
    }

    [DllImport("__Internal")]
    private static extern void DownloadFile(byte[] array, int byteLength, string fileName);
}
