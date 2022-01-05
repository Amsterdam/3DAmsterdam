using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using BruTile;
//using Terrain.ExtensionMethods;
using UnityEngine.Networking;
//using Terrain.Tiles;
using System.IO;
using System;
using Netherlands3D.Core;

using UnityEditor;

public class ClipQmTiles : MonoBehaviour
{
    public int areaXmin = 105000;
    public int areaXmax = 145000;
    public int areaYmin = 470000;
    public int areaYmax = 505000;

    public int zoomLevel = 18;
    public float tileSize = 500;
    public int concurrentTiles = 10;
    private int activeTiles = 0;

    public Vector2 bottomLeft = new Vector2(121000, 487000);
    private Mesh newMesh;
    List<Mesh> submeshes;

    Vector4 extentRD = new Vector4();
    Vector4 extentWGS = new Vector4();
    // Start is called before the first frame update

    public bool areaLoadingFinished = true;
    void Start()
    {

        //StartCoroutine(LoadAreas());
        
    }


    //private IEnumerator LoadAreas()
    //{
    //    for (int x = areaXmin; x < areaXmax; x+=(int)tileSize)
    //    {
    //        for (int y = areaYmin; y < areaYmax; y += (int)tileSize)
    //        {
    //            yield return new WaitWhile(() => areaLoadingFinished == false);
    //            areaLoadingFinished = false;

    //            LoadArea((float)x, (float)y);
    //        }
    //    }





    //}

    //void LoadArea(float rdX, float rdY)
    //{
    //    submeshes = new List<Mesh>();

    //    extentRD.x = rdX;
    //    extentRD.y = rdY;
    //    extentRD.z = extentRD.x + tileSize;
    //    extentRD.w = extentRD.y + tileSize;


    //    Vector3WGS RDVector = CoordConvert.RDtoWGS84(extentRD.x, extentRD.y);
    //    extentWGS.x = (float)RDVector.lon;
    //    extentWGS.y = (float)RDVector.lat;
    //    RDVector = CoordConvert.RDtoWGS84(extentRD.z, extentRD.w);
    //    extentWGS.z = (float)RDVector.lon;
    //    extentWGS.w = (float)RDVector.lat;

    //    StartCoroutine(IdentifyQMTileNames(extentWGS, (didError) => {
    //        HandleTiles();
    //    }));
    //}


    //private void HandleTiles()
    //{
    //    int submeshcount = submeshes.Count;
    //    CombineInstance[] combine = new CombineInstance[submeshcount];
    //    for (int i = 0; i < submeshcount; i++)
    //    {
    //        combine[i].mesh = submeshes[i];
    //    }
    //    Vector3 position = CoordConvert.RDtoUnity(new Vector3RD(extentRD.x, (extentRD.y), 0));
    //    position.y = 0;
    //    Mesh combinedMesh = new Mesh();
    //    combinedMesh.CombineMeshes(combine,true,false);
    //    Debug.Log(combinedMesh.bounds.extents);
    //    GameObject go = new GameObject();
    //    go.transform.position = position;

    //    go.AddComponent<MeshFilter>().mesh = combinedMesh;
    //    SaveMesh(combinedMesh);
    //    Debug.Log("Tile Handled");
    //    areaLoadingFinished = true;
    //}

    //private void SaveMesh(Mesh mesh)
    //{
    //    string meshname = "Terrain_"+extentRD.x.ToString()+"_"+extentRD.y.ToString() + "_lod0.mesh";
    //    string meshFolderName = "Assets/Terrain/LOD0/";
    //    AssetDatabase.CreateAsset(mesh, meshFolderName + meshname);
    //    AssetDatabase.SaveAssets();
    //    AssetImporter.GetAtPath(meshFolderName + meshname).SetAssetBundleNameAndVariant(extentRD.x.ToString() + "_" + extentRD.y.ToString() + "_terrain_lod0","");
    //    AssetDatabase.SaveAssets();
    //}

    //private void CreateClippedMesh(Mesh mesh, int X, int Y)
    //{
  
    //    Debug.Log(X + "-" + Y + " :overlapTile");
    //    Vector3WGS originWGS = TerrainTileCenterWGS(X, Y);
    //    Vector3 unityOrigin = CoordConvert.WGS84toUnity(extentWGS.x, extentWGS.y);

    //    Vector3RD[] verticesRD = getVerticesRD(terrainTile, X, Y, originWGS);
    //    int[] originalTriangles = getTriangles(terrainTile);
    //    List<int> insideTriangles = GetInsideTriangles(verticesRD, originalTriangles);


    //    Vector3[] verticesUnity = VerticesRDtoUnity(verticesRD, unityOrigin);
    //    CreateOverlapTriangles(verticesRD, originalTriangles, verticesUnity, unityOrigin);
    //    Mesh newSubMesh = new Mesh();
    //    newSubMesh.vertices = verticesUnity;
    //    newSubMesh.triangles = insideTriangles.ToArray();
    //    newSubMesh.uv = getUVs(verticesRD);
    //    newSubMesh.Optimize();
    //    newSubMesh.RecalculateNormals();
    //    submeshes.Add(newSubMesh);

    //    activeTiles--;

    //}

    private List<Vector3> CreateClippingPolygon(Vector4 extendRD, Vector3RD coordinateOffset)
    {
        List<Vector3> clippingPolygon = new List<Vector3>();
        Vector3 vector = new Vector3();
        vector.x = (float)(extentRD.x - coordinateOffset.x);
        vector.z = (float)(extentRD.y - coordinateOffset.y);
        clippingPolygon.Add(vector);
        vector = new Vector3();
        vector.x = (float)(extentRD.z - coordinateOffset.x);
        vector.z = (float)(extentRD.y - coordinateOffset.y);
        clippingPolygon.Add(vector);
        vector = new Vector3();
        vector.x = (float)(extentRD.z - coordinateOffset.x);
        vector.z = (float)(extentRD.w - coordinateOffset.y);
        clippingPolygon.Add(vector);
        vector = new Vector3();
        vector.x = (float)(extentRD.x - coordinateOffset.x);
        vector.z = (float)(extentRD.w - coordinateOffset.y);
        clippingPolygon.Add(vector);
        return clippingPolygon;
    }

    private void CreateOverlapTriangles(Vector3RD[] verticesRD, int[] originalTriangles, Vector3[] verticesUnity, Vector3 unityOrigin, Vector3RD coordinateOffset)
    {
        

        List<Vector3> clippingPolygon = new List<Vector3>();
        Vector3 vector = new Vector3();
        vector.x = (float)(extentRD.x - coordinateOffset.x);
        vector.z = (float)(extentRD.y - coordinateOffset.y);
        clippingPolygon.Add(vector);
        vector = new Vector3();
        vector.x = (float)(extentRD.z - coordinateOffset.x);
        vector.z = (float)(extentRD.y - coordinateOffset.y);
        clippingPolygon.Add(vector);
        vector = new Vector3();
        vector.x = (float)(extentRD.z - coordinateOffset.x);
        vector.z = (float)(extentRD.w - coordinateOffset.y);
        clippingPolygon.Add(vector);
        vector = new Vector3();
        vector.x = (float)(extentRD.x - coordinateOffset.x);
        vector.z = (float)(extentRD.w - coordinateOffset.y);
        clippingPolygon.Add(vector);


        int originalTriangleCount = originalTriangles.Length - 2;
        for (int i = 0; i < originalTriangleCount; i += 3)
        {
            Vector3RD vector1 = verticesRD[originalTriangles[i]];
            Vector3RD vector2 = verticesRD[originalTriangles[i + 1]];
            Vector3RD vector3 = verticesRD[originalTriangles[i + 2]];
            if (Vector3RDIsInside(vector1) && Vector3RDIsInside(vector2) && Vector3RDIsInside(vector3))
            {

            }
            else
            {
                List<Vector3> vectors = new List<Vector3>();
                // to use overlap.cs y en z must be flipped and winding-order must be counter-clockwise
                // to prevent inaccuracy due to large number we offset the coordinates so the first vertex is at 0,0,0
                vector = new Vector3();
                vector.x = (float)(verticesRD[originalTriangles[i + 2]].x - coordinateOffset.x);
                vector.y = (float)(verticesRD[originalTriangles[i + 2]].z - coordinateOffset.z);
                vector.z = (float)(verticesRD[originalTriangles[i + 2]].y - coordinateOffset.y);
                vectors.Add(vector);
                vector = new Vector3();
                vector.x = (float)(verticesRD[originalTriangles[i + 1]].x - coordinateOffset.x);
                vector.y = (float)(verticesRD[originalTriangles[i + 1]].z - coordinateOffset.z);
                vector.z = (float)(verticesRD[originalTriangles[i + 1]].y - coordinateOffset.y);
                vectors.Add(vector);
                vector = new Vector3();
                vector.x = (float)(verticesRD[originalTriangles[i]].x - coordinateOffset.x);
                vector.y = (float)(verticesRD[originalTriangles[i]].z - coordinateOffset.z);
                vector.z = (float)(verticesRD[originalTriangles[i]].y - coordinateOffset.y);
                vectors.Add(vector);

                CreateClippedMesh(vectors, clippingPolygon, coordinateOffset, unityOrigin);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vectors">triangleVertices (unity-format, so Y is up)</param>
    /// <param name="clipboundary">coordinates of vertices of (square) clipboundary</param>
    /// <param name="coordinateOffset"></param>
    /// <param name="unityOrigin"></param>
    private void CreateClippedMesh(List<Vector3> vectors,List<Vector3>clipboundary, Vector3RD coordinateOffset, Vector3 unityOrigin)
    {
        List<Vector3> defshape = TriangleClipping.SutherlandHodgman.ClipPolygon(vectors, clipboundary);
        if (defshape.Count<3)
        {
            return;
        }
        List<Vector3RD> verticesRD = new List<Vector3RD>();
        Vector3RD vector;
        //restore coordinates
        for (int i = 0; i < defshape.Count; i++)
        {
            vector = new Vector3RD();
            vector.x = defshape[i].x + coordinateOffset.x;
            vector.y = defshape[i].z + coordinateOffset.y;
            vector.z = defshape[i].y + coordinateOffset.z;
            verticesRD.Add(vector);
        }
        verticesRD.Reverse();


        List<int> indices = new List<int>();
        indices.Add(0);
        indices.Add(1);
        indices.Add(2);
        for (int i = 3; i < defshape.Count; i++)
        {
            indices.Add(0);
            indices.Add(indices[indices.Count - 2]);
            indices.Add(i);
        }
        Vector3[] verticesUnity = VerticesRDtoUnity(verticesRD.ToArray(), unityOrigin);

        Mesh newSubMesh = new Mesh();
        newSubMesh.vertices = verticesUnity;
        newSubMesh.triangles = indices.ToArray();
        newSubMesh.uv = getUVs(verticesRD.ToArray());
        newSubMesh.Optimize();
        newSubMesh.RecalculateNormals();
        submeshes.Add(newSubMesh);

    }
    private List<int> GetInsideTriangles(Vector3RD[] verticesRD, int[] originalTriangles)
    {
        List<int> Triangles = new List<int>();
        int originalTriangleCount = originalTriangles.Length - 2;
        for (int i = 0; i < originalTriangleCount; i += 3)
        {
            Vector3RD vector1 = verticesRD[originalTriangles[i]];
            Vector3RD vector2 = verticesRD[originalTriangles[i + 1]];
            Vector3RD vector3 = verticesRD[originalTriangles[i + 2]];
            if (Vector3RDIsInside(vector1) && Vector3RDIsInside(vector2) && Vector3RDIsInside(vector3))
            {
                //winding-order is clockwise
                Triangles.Add(originalTriangles[i]);
                Triangles.Add(originalTriangles[i + 1]);
                Triangles.Add(originalTriangles[i + 2]);
            }
            else
            {

            }
        }
        return Triangles;
    }

    private bool Vector3RDIsInside(Vector3RD vector)
    {
        double vector1Pos = 0;
        if (vector.x > extentRD.x) { vector1Pos += 1; } ; //positive if vector1.x > extent minX
        if (vector.y > extentRD.y) { vector1Pos += 1; }; //positive if vector1.y > extent minY
        if (extentRD.z >vector.x) { vector1Pos += 1; }; // postive if vector1.x < extent maxX
        if (extentRD.w > vector.y) { vector1Pos += 1; }; // postive if vector1.y < extent maxY

        if (vector1Pos<4)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private Vector2[] getUVs(Vector3RD[] verticesRD)
    {
        int vertexcount = verticesRD.Length;
        Vector2[] uvs = new Vector2[vertexcount];
        for (int i = 0; i < vertexcount; i++)
        {
            uvs[i].x = (float)(verticesRD[i].x - bottomLeft.x) / tileSize;
            uvs[i].y = (float)(verticesRD[i].y - bottomLeft.y) / tileSize;
        }

        return uvs;
    }

   //private void HandleInsideTile(TerrainTile terrainTile, int X, int Y)
   // {
   //     if (terrainTile == null)
   //     {
   //         activeTiles--;
   //         return;
   //     }
   //     Debug.Log(X + "-" + Y + " :insideTile");
   //     Vector3WGS centerWGS = TerrainTileCenterWGS(X, Y);
   //     Vector3 centerUnity = CoordConvert.WGS84toUnity(extentWGS.x, extentWGS.y);
   //     centerUnity.y = 0;
   //     Vector3RD[] verticesRD = getVerticesRD(terrainTile, X, Y, centerWGS);
   //     Vector3[] verticesUnity = VerticesRDtoUnity(verticesRD, centerUnity);
   //     int[] triangles = getTriangles(terrainTile);
   //     Mesh newSubMesh = new Mesh();
   //     newSubMesh.vertices = verticesUnity;
   //     newSubMesh.triangles = triangles;
   //     newSubMesh.uv = getUVs(verticesRD);
   //     newSubMesh.RecalculateNormals();
   //    submeshes.Add(newSubMesh);
   //     activeTiles--;
   // }
    private Vector3[] VerticesRDtoUnity(Vector3RD[] verticesRD, Vector3 unityOrigin)
    {
        
        int vertexcount = verticesRD.Length;
        Vector3[] verticesUnity = new Vector3[vertexcount];
        for (int i = 0; i < vertexcount; i++)
        {
            verticesUnity[i] = CoordConvert.RDtoUnity(verticesRD[i])-unityOrigin;
        }
        return verticesUnity;
    }

    //private int[] getTriangles(TerrainTile terraintile)
    //{
        
    //    return terraintile.GetTriangles(0);
    //}

    private Vector3WGS TerrainTileCenterWGS(int X, int Y)
    {
        Vector3WGS origin = new Vector3WGS();
        double tileSize = 180 / Math.Pow(2, zoomLevel);
        origin.lon = ((X+0.5) * tileSize) - 180;
        origin.lat = ((Y+0.5) * tileSize) - 90;
        return origin;
    }

   // private Vector3RD[] getVerticesRD(TerrainTile terrainTile, int X, int Y, Vector3WGS centerWGS)
   //{
   //     //Vector3[] verticesRAW = terrainTile.GetVertices();  // origin =  tilecenter, x and y coordinates in range -90 to 90, flips y and H 
   //     Vector3RD[] verticesRD = new Vector3RD[terrainTile.VertexData.vertexCount];
        
   //     double MinHeight = terrainTile.Header.MinimumHeight;
   //     double MaxHeight = terrainTile.Header.MaximumHeight;
   //     VertexData verts = terrainTile.VertexData;

   //     int MAX = 32767;

   //     double tileSizeDegrees = 180 / Math.Pow(2, zoomLevel);

   //     for (int i = 0; i < verts.vertexCount; i++)
   //     {
   //         //lerp vertices
   //         var xCoor = verts.u[i];
   //         var yCoor = verts.v[i];
   //         var height = verts.height[i];

   //         var x1 = Terrain.Tiles.Mathf.Lerp(-0.5, 0.5, ((double)(xCoor) / MAX));
   //         x1 = centerWGS.lon + (tileSizeDegrees * x1);
   //         var y1 = Terrain.Tiles.Mathf.Lerp(-0.5, 0.5, ((double)(yCoor) / MAX));
   //         y1 = centerWGS.lat + (tileSizeDegrees * y1);
   //         var h1 = Terrain.Tiles.Mathf.Lerp(MinHeight, MaxHeight, ((double)height / MAX));
   //         verticesRD[i] = CoordConvert.WGS84toRD(x1, y1);
   //         verticesRD[i].z = h1 + CoordConvert.ReferenceRD.z;
   //         //vertices[i] = new Vector3((float)x1, (float)h1, (float)y1);
   //     }


    //    //int vertexcount = verticesRAW.Length;
    //    //Vector3WGS coordinate = new Vector3WGS();
    //    //for (int i = 0; i < vertexcount; i++)
    //    //{
    //    //    coordinate.lon = centerWGS.lon + (tileSizeDegrees * verticesRAW[i].x / 90);
    //    //    coordinate.lat = centerWGS.lat + (tileSizeDegrees * verticesRAW[i].z / 90);
    //    //    coordinate.h = verticesRAW[i].y;
    //    //    verticesRD[i] = CoordConvert.WGS84toRD(coordinate.lon,coordinate.lat);
    //    //    verticesRD[i].z = coordinate.h + CoordConvert.ReferenceRD.z;
    //    //}
    //    return verticesRD;
    //}


    //private IEnumerator IdentifyQMTileNames(Vector4 extentWGS, System.Action<int> callbackOnFinish)
    //{

    //    double TegelAfmeting = 180 / (Math.Pow(2, zoomLevel)); //tegelafmeting in graden bij zoomniveau gekozen zoomlevel;
    //    int tegelMinX = (int)Math.Floor((extentWGS.x  + 180) / TegelAfmeting);
    //    int tegelMaxX = (int)Math.Floor((extentWGS.z + 180) / TegelAfmeting);
    //    int tegelMinY = (int)Math.Floor((extentWGS.y + 90) / TegelAfmeting);
    //    int tegelMaxY = (int)Math.Floor((extentWGS.w + 90) / TegelAfmeting);

        
       
    //    for (int X = tegelMinX; X < tegelMaxX + 1; X++)
    //    {

    //        for (int Y = tegelMinY; Y < tegelMaxY + 1; Y++)
    //        {
    //            yield return new WaitUntil(() => concurrentTiles >= activeTiles);
    //            string TileName = zoomLevel+"/" + X + "/" + Y + ".terrain";
    //            if (X == tegelMinX || X==tegelMaxX || Y==tegelMinY || Y==tegelMaxY)
    //            {
                    
    //                activeTiles++;
                    
    //                StartCoroutine(downloadQMTiles(TileName, X,Y,(tileName,Xout,Yout) =>
    //                {
    //                    HandleOverlapTile(tileName, Xout, Yout);
    //                }));
    //            }
    //            else
    //            {
    //                activeTiles++;
    //                StartCoroutine(downloadQMTiles(TileName,X,Y, (tileName, Xout, Yout) =>
    //                {
    //                    HandleInsideTile(tileName,Xout,Yout);
    //                }));
    //            }
    //        }
    //    }
    //    yield return new WaitUntil(() => activeTiles==0);
    //    callbackOnFinish(0);
    //}

    //private IEnumerator downloadQMTiles(string TileName,int X, int Y,System.Action<TerrainTile,int,int> callbackOnFinish)
    //{
    //    string Baseurl = "https://acc.3d.amsterdam.nl/web/data/develop/terrain/";

    //        DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
    //        TerrainTile terrainTile;

    //        ///QM-tile downloaden
    //        UnityWebRequest www = new UnityWebRequest(Baseurl + TileName);

    //        www.downloadHandler = handler;
    //        yield return www.SendWebRequest();

    //        if (!www.isNetworkError && !www.isHttpError)
    //        {
    //            //get data
    //            MemoryStream stream = new MemoryStream(www.downloadHandler.data);

    //            //parse into tile data structure
    //            terrainTile = TerrainTileParser.Parse(stream);
    //            callbackOnFinish(terrainTile,X,Y);
    //    }
    //        else
    //    {
    //        callbackOnFinish(null, X, Y);
    //    }

        
    //}


}

