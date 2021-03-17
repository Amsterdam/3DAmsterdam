using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using LayerSystem;

public class exportDXF : MonoBehaviour
{
    public double bottomLeftX;
    public double bottomLeftY;
    public double topRightX;
    public double topRightY;

    public Layer BuildingsLayer;

    public void Start()
    {
        GetLayerMesh();
    }

    public void GetLayerMesh()
    {
        int LayerTileSize = BuildingsLayer.tileSize;
        List<GameObject> overlappingTiles = new List<GameObject>();
        foreach (var item in BuildingsLayer.tiles)
        {
            if (TileOverlaps(item.Key.x,item.Key.y,LayerTileSize))
            {
                ClipMesh(item.Value.gameObject);
            }
        }
    }

    private List<Vector3RD> ClipMesh(GameObject tileGameObject)
    {
        double width = topRightX - bottomLeftX;
        double height = topRightX = bottomLeftY;
        Mesh mesh = tileGameObject.GetComponent<MeshFilter>().mesh;
        Vector3 GameObjectOrigin = tileGameObject.transform.position;
        Vector3[] meshVertices = mesh.vertices;
        Vector3RD[] movedRDVertices = new Vector3RD[meshVertices.Length];
        List<Vector3RD> movedRDTriangles = new List<Vector3RD>();
        List<Vector3RD> clippedRDTriangles = new List<Vector3RD>();
        Vector3RD vectorRD = new Vector3RD();
        int[] meshIndices = mesh.GetIndices(0);

        Vector3 bottomLeftUnity = CoordConvert.RDtoUnity(new Vector3RD(bottomLeftX,bottomLeftY,0));
        movedRDVertices = getMovedCoordinatesUnity(bottomLeftX, bottomLeftY, meshVertices, tileGameObject.transform.position);
        movedRDVertices = getMovedCoordinatesRD(bottomLeftX, bottomLeftY, meshVertices, tileGameObject.transform.position);
        List<Vector3> vectors = new List<Vector3>();
        vectors.Capacity = 3;
        List<Vector3> defshape = new List<Vector3>();
        defshape.Capacity = 6;
        int insideCounter = 0;
        List<Vector3> clipboundary = CreateClippingPolygon((float)width, (float)height);
        for (int i = 0; i < meshIndices.Length; i+=3)
        {
            insideCounter = 0;
            if (PointISInsideArea(movedRDVertices[i], width, height)) { insideCounter++; }
            if( PointISInsideArea(movedRDVertices[i+1], width, height)) { insideCounter++; }
            if( PointISInsideArea(movedRDVertices[i+2], width, height)) { insideCounter++; }

            if(insideCounter ==3) // triangle is completely inside boundingbox
            {
                clippedRDTriangles.Add(movedRDVertices[i]);
                clippedRDTriangles.Add(movedRDVertices[i + 1]);
                clippedRDTriangles.Add(movedRDVertices[i+2]);
                continue;
            }
            if(insideCounter>0) //triangle is partly inside boundingbox, needs clipping
            {
                // flip y and z-axis so clippingtool works
                vectors.Clear();
                vectors.Add(new Vector3((float)(movedRDVertices[i].x ), (float)movedRDVertices[i].y, (float)movedRDVertices[i].z));
                vectors.Add(new Vector3((float)(movedRDVertices[i+1].x), (float)movedRDVertices[i+1].y, (float)movedRDVertices[i+1].z));
                vectors.Add(new Vector3((float)(movedRDVertices[i+2].x), (float)movedRDVertices[i+2].y, (float)movedRDVertices[i+2].z));

                defshape = TriangleClipping.SutherlandHodgman.ClipPolygon(vectors, clipboundary);
                if (defshape.Count < 3) // errortrapping, 
                {
                    continue;
                }

                if (defshape[0].x.ToString() == "NaN")
                {
                    continue;
                }
                
                // add first three vectors

                vectorRD.x = defshape[0].x;
                vectorRD.y = defshape[0].z;
                vectorRD.z = defshape[0].y;
                clippedRDTriangles.Add(vectorRD);

                vectorRD.x = defshape[1].x;
                vectorRD.y = defshape[1].z;
                vectorRD.z = defshape[1].y;
                clippedRDTriangles.Add(vectorRD);
                vectorRD.x = defshape[2].x;
                vectorRD.y = defshape[2].z;
                vectorRD.z = defshape[2].y;
                clippedRDTriangles.Add(vectorRD);

                // add extra vectors. vector makes a triangle with the first and the previous vector.
                for (int j = 3; j < defshape.Count; j++)
                {
                    vectorRD.x = defshape[0].x;
                    vectorRD.y = defshape[0].z;
                    vectorRD.z = defshape[0].y;
                    clippedRDTriangles.Add(vectorRD);

                    vectorRD.x = defshape[j - 1].x;
                    vectorRD.y = defshape[j - 1].z;
                    vectorRD.z = defshape[j - 1].y;
                    clippedRDTriangles.Add(vectorRD);

                    vectorRD.x = defshape[j].x;
                    vectorRD.y = defshape[j].z;
                    vectorRD.z = defshape[j].y;
                    clippedRDTriangles.Add(vectorRD);
                }

            }
        }
        return clippedRDTriangles;
    }
    private bool PointISInsideArea(Vector3RD point, double Width, double Height)
    {

        if (point.x < 0 || point.x > (Width))
        {
            return false;
        }
        if (point.y < 0 || point.y > (Height))
        {
            return false;
        }

        return true;
    }
    private List<Vector3> CreateClippingPolygon(float width, float height)
    {
        List<Vector3> polygon = new List<Vector3>();
        polygon.Capacity = 4;
        polygon.Add(new Vector3(0, 0, 0));
        polygon.Add(new Vector3(width, 0, 0));
        polygon.Add(new Vector3(width, 0, height));
        polygon.Add(new Vector3(0, 0, height));
        return polygon;
    }
    private bool TileOverlaps(int bottomX, int bottomY,int tileSize)
    {
        if (bottomX + tileSize < bottomLeftX){ return false;}
        if (bottomX > topRightX) { return false; }
        if (bottomY + tileSize < bottomLeftY) { return false; }
        if (bottomY > topRightY) { return false; }
        return true;
    }

    private Vector3RD[] getMovedCoordinatesUnity(double bottomLeftX, double bottomLeftY, Vector3[] meshVertices, Vector3 GameObjectOrigin)
    {
        Vector3 bottomLeftUnity = CoordConvert.RDtoUnity(new Vector3RD(bottomLeftX, bottomLeftY, 0));
        Vector3RD vertexposition;
        Vector3 tempVertexPosition;
        Vector3RD[] movedRDVertices = new Vector3RD[meshVertices.Length];
        for (int i = 0; i < meshVertices.Length; i++)
        {
            tempVertexPosition = meshVertices[i] + GameObjectOrigin;

            vertexposition.x = tempVertexPosition.x - bottomLeftUnity.x;
            vertexposition.y = tempVertexPosition.y - bottomLeftUnity.y;
            vertexposition.z = tempVertexPosition.z;
            movedRDVertices[i] = vertexposition;
        }
        return movedRDVertices;
    }
    private Vector3RD[] getMovedCoordinatesRD(double bottomLeftX, double bottomLeftY, Vector3[] meshVertices, Vector3 GameObjectOrigin)
    {
        Vector3 bottomLeftUnity = CoordConvert.RDtoUnity(new Vector3RD(bottomLeftX, bottomLeftY, 0));
        Vector3RD vertexposition;
        Vector3 tempVertexPosition;
        Vector3RD[] movedRDVertices = new Vector3RD[meshVertices.Length];
        for (int i = 0; i < meshVertices.Length; i++)
        {
            vertexposition = CoordConvert.UnitytoRD(meshVertices[i] + GameObjectOrigin);

            vertexposition.x -=  bottomLeftX;
            vertexposition.y -= bottomLeftY;
            movedRDVertices[i] = vertexposition;
        }
        return movedRDVertices;
    }
}
