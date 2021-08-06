using ConvertCoordinates;
using Netherlands3D;
using Netherlands3D.Interface;
using Netherlands3D.LayerSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;


public class PerceelRenderer : MonoBehaviour
{
    public Material LineMaterial;
    public static PerceelRenderer Instance;
    public GameObject TerrainLayer;
    public GameObject BuildingsLayer;

    private void Awake()
    {
        Instance = this;
    }


    public IEnumerator GetAndRenderPerceel(Vector3RD position)
    {
        yield return null;
        var bbox = $"{ position.x - 0.5},{ position.y - 0.5},{ position.x + 0.5},{ position.y + 0.5}";

        var url = $"https://geodata.nationaalgeoregister.nl/kadastralekaart/wfs/v4_0?SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=kadastralekaartv4:perceel&STARTINDEX=0&COUNT=1&SRSNAME=urn:ogc:def:crs:EPSG::28992&BBOX={bbox},urn:ogc:def:crs:EPSG::28992&outputFormat=json";

        UnityWebRequest req = UnityWebRequest.Get(url);
        //getSceneRequest.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
        {
            WarningDialogs.Instance.ShowNewDialog("Perceel data kon niet opgehaald worden");
        }
        else
        {
            //Debug.Log("Perceel data: " + req.downloadHandler.text);

            List<Vector2[]> list = new List<Vector2[]>();

            //Deserialize WFS perceel data
            using (JsonTextReader reader = new JsonTextReader(new StringReader(req.downloadHandler.text)))
            {
                reader.SupportMultipleContent = true;
                var serializer = new JsonSerializer();
                JsonModels.WebFeatureService.WFSRootobject wfs = serializer.Deserialize<JsonModels.WebFeatureService.WFSRootobject>(reader);

                yield return null;

                foreach (var feature in wfs.features)
                {
                    List<Vector2> polygonList = new List<Vector2>();

                    var coordinates = feature.geometry.coordinates;
                    foreach (var points in coordinates)
                    {
                        foreach (var point in points)
                        {
                            polygonList.Add(new Vector2(point[0], point[1]));
                        }
                    }
                    list.Add(polygonList.ToArray());
                }
            }

            //TODO teken het perceel polygon
            //StartCoroutine(RenderPolygons(list));
            //RenderPolygonMesh(list);

            foreach (Transform gam in TerrainLayer.transform)
            {
                // Debug.Log(gam.name);
                gam.gameObject.AddComponent<MeshCollider>();
            }

            yield return null;

            StartCoroutine(RenderPolygons(list));

            //TODO raycast from position to building to hit the building and get the vertex index



            //let feature = data.features[0];
            //this.kadastraleGrootteWaarde = feature.properties.kadastraleGrootteWaarde;
            ////TODO support multiple polygons
            //this.polygon_rd = feature.geometry.coordinates[0];

        }
    }


    public IEnumerator HandlePosition(Vector3RD position)
    {
        StartCoroutine(GetAndRenderPerceel(position));

        StartCoroutine(HighlightBuilding(CoordConvert.RDtoUnity(position)));

        yield return null;

    }

    IEnumerator HighlightBuilding(Vector3 position)
    {
        //TODO detect when tile is loaded..
        yield return new WaitForSeconds(2);


        foreach (Transform gam in BuildingsLayer.transform)
        {
            var rd = gam.name.GetRDCoordinate();

            gam.gameObject.AddComponent<MeshCollider>();

            yield return null;

            RaycastHit hit;
            position.y += 100;
            Ray ray = new Ray(position, Vector3.down);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                int triangleVertexIndex = hit.triangleIndex * 3;
                var vertexIndex = gam.gameObject.GetComponent<MeshFilter>().mesh.GetIndices(0)[triangleVertexIndex];

                Mesh mesh = hit.collider.gameObject.GetComponent<MeshFilter>().mesh;
                if (vertexIndex > mesh.uv2.Length)
                {
                    Debug.LogWarning("UV index out of bounds. This object/LOD level does not contain highlight/hidden uv2 slot");
                    yield break;
                }

                StartCoroutine(DownloadObjectData(rd, vertexIndex, gam.gameObject ));
            }
            else
            {
                Debug.Log("no building was hit");
            }

            //Debug.Log($"building gameobject: {gam.name}");
        }
    }

    IEnumerator HighlightBuildingArea(List<Vector2[]> polygons, Vector3 position)
    {
        yield return null;
        var q = from i in polygons
                from p in i
                select CoordConvert.RDtoUnity(p) into v3
                select new Vector2(v3.x, v3.z);

        var polyPoints = q.ToArray();

        foreach (var point in polyPoints)
        {
            Debug.Log($"pointx:{point.x} pointy:{point.y}"); //unity absolute 
        }

        foreach (Transform gam in BuildingsLayer.transform)
        {
            var rd = gam.name.GetRDCoordinate();
            var tile = CoordConvert.RDtoUnity(rd);
            //Debug.Log($"tilex: {tile.x} tilez: {tile.z}");

            var filter = gam.GetComponent<MeshFilter>();
            var verts = filter.mesh.vertices;

            var xmin = tile.x + verts.Min(o => o.x) + 500;
            var xmax = tile.x + verts.Max(o => o.x) + 500;
            var zmin = tile.z + verts.Min(o => o.z) + 500;
            var zmax = tile.z + verts.Max(o => o.z) + 500;

            Debug.Log($"xmin:{xmin} xmax:{xmax} zmin:{zmin} zmax:{zmax}");  //unity absolute ( tile + relative verts pos)

            Color[] colors = new Color[filter.mesh.vertexCount];

            for (int i = 0; i < filter.mesh.vertexCount; i++)
            {
                var vertx = tile.x + verts[i].x + 500;
                var verty = tile.z + verts[i].z + 500;

                //var dist = Vector2.Distance(polyPoints[0], new Vector2(vertx, verty) );
                //distances.Add(dist);
                //check if vertexpoint is in the polygon, if so color it
                if (ContainsPoint(polyPoints, new Vector2(vertx, verty)))   //unity absolute
                //if (dist < 100)
                {
                    //colors[i] = new Color(1, (dist / 100), (dist / 100));
                    colors[i] = Color.red;
                }
                else
                {
                    colors[i] = Color.white;
                }

            }

            filter.mesh.colors = colors;

            Debug.Log($"building gameobject: {gam.name}"  );
        }
    }

    void RenderPolygonMesh(List<Vector2[]> polygons)
    {
        var perceelGameObject = new GameObject();

        var go = new GameObject();
        go.name = "Perceel mesh";
        go.transform.parent = perceelGameObject.transform;
        ProBuilderMesh m_Mesh = go.gameObject.AddComponent<ProBuilderMesh>();
        go.GetComponent<MeshRenderer>().material = LineMaterial;

        List<Vector3> verts = new List<Vector3>();

        foreach (var points in polygons)
        {
            foreach (var point in points)
            {
                var pos = CoordConvert.RDtoUnity(point);
                verts.Add(pos);
            }
        }

        m_Mesh.CreateShapeFromPolygon(verts.ToArray(), 5, false);  // TODO place op top of maaiveld            


        var go_rev = new GameObject();
        go_rev.transform.parent = perceelGameObject.transform;
        go_rev.name = "Perceel mesh_rev";
        ProBuilderMesh m_Mesh_rev = go_rev.gameObject.AddComponent<ProBuilderMesh>();
        go_rev.GetComponent<MeshRenderer>().material = LineMaterial;
        verts.Reverse();
        m_Mesh_rev.CreateShapeFromPolygon(verts.ToArray(), 5, false);

    }

    IEnumerator RenderPolygons(List<Vector2[]> polygons)
    {

        List<Vector2> vertices = new List<Vector2>();
        List<int> indices = new List<int>();

        int count = 0;
        foreach (var list in polygons)
        {
            for (int i = 0; i < list.Length - 1; i++)
            {
                indices.Add(count + i);
                indices.Add(count + i + 1);            
            }
            count += list.Length;
            vertices.AddRange(list);
        }

        GameObject gam = new GameObject();
        gam.name = "Perceel";
        gam.transform.parent = transform;

        MeshFilter filter = gam.AddComponent<MeshFilter>();
        gam.AddComponent<MeshRenderer>().material = LineMaterial;

        var verts = vertices.Select(o => CoordConvert.RDtoUnity(o)).ToArray();

        //use collider to place the polygon points on the terrain
        for(int i=0; i < verts.Length; i++)
        {
            var point = verts[i];
            var from = new Vector3(point.x, point.y + 10, point.z);


            Ray ray = new Ray(from, Vector3.down);
            RaycastHit hit;

            yield return null;

            //raycast from the polygon point to hit the terrain so we can place the outline so that it is visible
            if(Physics.Raycast( ray, out hit, Mathf.Infinity  ))
            {
                Debug.Log("we have a hit");
                verts[i].y = hit.point.y + 0.5f;
            }
            else
            {
                Debug.Log("raycast failed..");
            }
        }

        var mesh = new Mesh();
        mesh.vertices = verts;
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        filter.sharedMesh = mesh;
    }


    public bool ContainsPoint(Vector2[] polyPoints, Vector2 p)
    {
        var j = polyPoints.Length - 1;
        var inside = false;
        for (int i = 0; i < polyPoints.Length; j = i++)
        {
            var pi = polyPoints[i];
            var pj = polyPoints[j];
            if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                inside = !inside;
        }
        return inside;
    }

    //private IEnumerator DownloadObjectData(GameObject obj, int vertexIndex, System.Action<string> callback)
    private IEnumerator DownloadObjectData( Vector3RD rd, int vertexIndex, GameObject buildingGameObject )
    {

        var dataURL = $"{Config.activeConfiguration.buildingsMetaDataPath}/buildings_{rd.x}_{rd.y}.2.2-data";

        ObjectMappingClass data;
        string id = "null";

        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
            }
            else
            {
                ObjectData objectMapping = buildingGameObject.AddComponent<ObjectData>();
                AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];
                int idIndex = data.vectorMap[vertexIndex];
                id = data.ids[idIndex];
                objectMapping.ids = data.ids;
                objectMapping.uvs = data.uvs;
                objectMapping.vectorMap = data.vectorMap;
                
                objectMapping.highlightIDs = new List<string>()
                {
                    id
                };

                newAssetBundle.Unload(true);

                objectMapping.ApplyDataToIDsTexture();
            }
            
        }
        //callback?.Invoke(id);
        yield return null;
        
    }


}
