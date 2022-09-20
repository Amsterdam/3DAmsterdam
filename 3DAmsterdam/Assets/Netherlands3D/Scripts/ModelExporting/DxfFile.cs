using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using netDxf;
using netDxf.Entities;
using netDxf.Blocks;
using netDxf.Tables;
using System.IO;
using System.Runtime.InteropServices;
using Netherlands3D.Core;

public class DxfFile 
{
    private DxfDocument dxfDocument;
    private Layer dxfLayer;

    [DllImport("__Internal")]
    private static extern void DownloadFile(byte[] array, int byteLength, string fileName);

    public void SetupDXF()
    {
        dxfDocument = new DxfDocument();
        dxfDocument.DrawingVariables.InsUnits = netDxf.Units.DrawingUnits.Meters;
    }

    public void AddLayer(List<Vector3RD>triangleVertices,string layerName, AciColor layerColor)
    {
        // TODO 
        // check if there are 3 triangles or less, if that is the case a polyfaceMesh cannot be built, seperate triangles have to be added to the dxf.
        dxfLayer = new Layer(layerName);
        dxfLayer.Color = layerColor;
        dxfDocument.Layers.Add(dxfLayer);
        
        //AddTriangles(triangleVertices, layerName);


        AddMesh(triangleVertices, layerName);
    }
    public void Save()
    {
#if UNITY_EDITOR

        var mydocs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        dxfDocument.Save(Path.Combine(mydocs, "testDXFBinary.dxf"), true);        
        return;
#endif
        using (var stream = new MemoryStream())
        {
            if (dxfDocument.Save(stream))
            {
                DownloadFile(stream.ToArray(), stream.ToArray().Length, "testfile.dxf");
            }
            else
            {
                Debug.Log("cant write file");
            }
        }
    }

    public void AddTriangles(List<Vector3RD> triangleVertices, string layerName)
    {
        Block block = new Block(layerName);

        for (int i = 0; i < triangleVertices.Count; i+=3)
        {
            netDxf.Vector3 vertex1 = new netDxf.Vector3(triangleVertices[i].x, triangleVertices[i].y, triangleVertices[i].z);
            netDxf.Vector3 vertex2 = new netDxf.Vector3(triangleVertices[i+1].x, triangleVertices[i+1].y, triangleVertices[i+1].z);
            netDxf.Vector3 vertex3 = new netDxf.Vector3(triangleVertices[i+2].x, triangleVertices[i+2].y, triangleVertices[i+2].z);
            Face3d face = new Face3d(vertex1, vertex3, vertex2);
            block.Entities.Add(face);
            //doc.AddEntity(face);
            //face.Layer = Laag;
        }
        Insert blokInsert = new Insert(block);
        blokInsert.Layer = dxfLayer;
        dxfDocument.AddEntity(blokInsert);
    }

    private void AddMesh(List<Vector3RD> triangleVertices, string layerName)
    {
        PolyfaceMesh pfm;
        // create Mesh
        List<PolyfaceMeshVertex> pfmVertices = new List<PolyfaceMeshVertex>();
        pfmVertices.Capacity = triangleVertices.Count;
        List<PolyfaceMeshFace> pfmFaces = new List<PolyfaceMeshFace>();
        pfmFaces.Capacity = triangleVertices.Count / 3;
        int facecounter = 0;
        Debug.Log(triangleVertices.Count);
        int vertexIndex = 0;

        for (int i = 0; i < triangleVertices.Count; i += 3)
        {

            
            pfmVertices.Add(new PolyfaceMeshVertex(triangleVertices[i].x, triangleVertices[i].y, triangleVertices[i].z));
            pfmVertices.Add(new PolyfaceMeshVertex(triangleVertices[i + 2].x, triangleVertices[i + 2].y, triangleVertices[i + 2].z));
            pfmVertices.Add(new PolyfaceMeshVertex(triangleVertices[i + 1].x, triangleVertices[i + 1].y, triangleVertices[i + 1].z));
            
            PolyfaceMeshFace pfmFace = new PolyfaceMeshFace(new List<short>() { (short)(vertexIndex+1), (short)(vertexIndex + 2), (short)(vertexIndex + 3) });
            vertexIndex += 3;
            pfmFaces.Add(pfmFace);
            facecounter++;
            if (facecounter % 10000 == 0)
            {
                pfm = new PolyfaceMesh(pfmVertices, pfmFaces);
                pfm.Layer = dxfLayer;
                dxfDocument.AddEntity(pfm);
                pfmVertices.Clear();
                pfmFaces.Clear();
                facecounter = 0;
                vertexIndex = 0;
            }
        }
        if (pfmFaces.Count>0)
        {
            pfm = new PolyfaceMesh(pfmVertices, pfmFaces);
            pfm.Layer = dxfLayer;
            dxfDocument.AddEntity(pfm);
        }
        
        
    }
}
