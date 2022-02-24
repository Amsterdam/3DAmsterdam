using HeatMonitor;
using Netherlands3D;
using Netherlands3D.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class HeatMonitorLayer : MonoBehaviour
{

    /// <summary>
    /// Reference to FRD so we can change stencil settings.
    /// </summary>
    [SerializeField]
    private ForwardRendererData forwardRenderer;




    [SerializeField]
    private ApplicationConfiguration applicationConfiguration;

    /// <summary>
    /// Heat material.
    /// </summary>
    [SerializeField]
    private Material heatMaterial;

    // Parent object of the tiles.
    [SerializeField]
    private Transform targetParent;

    // UVs are always the same for quad; so allocate it once.
    private static Vector2[] uvQuads =   {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
        };


    // Indices are always the same. Use preallocated list:
    private static int[] indicesQuad =   {
            0,
            2,
            1,
            2,
            3,
            1
        };


    // Render settings:
    [SerializeField]
    private float minValue = 0.0f;

    [SerializeField]
    private float maxValue = 2.0f;

    /// <summary>
    /// If using the projector option, we use this prefab.
    /// </summary>
    [SerializeField]
    private GameObject prefabProjector;

    [SerializeField]
    private bool UseProjector = true;

    [Header("Projector tile offset (half tile), default 500")]
    [SerializeField]
    private float ProjectorTileOffset = 500;

    private bool geometryGenerated = false;

    [SerializeField]
    private string activeLayer;

    /// <summary>
    /// Dictionary that we use to keep track of the loaded tiles for updating the textures if needed.
    /// </summary>
    private Dictionary<System.Tuple<int, int>, GameObject> RDToTileDictionary;

    [SerializeField]
    private string BaseURL = @"http://127.0.0.1:8080/hitte-monitor-tiles/";

    private Coroutine activeCoroutine = null;

    /// <summary>
    /// Layer information using id as lookup.
    /// </summary>
    private Dictionary<string, HeatMonitor.HeatMonitorJSON.Layer> layerInformation;

    /// <summary>
    /// We actually get the active layer from the togglegroup.
    /// </summary>
    [SerializeField]
    private ToggleGroup toggleGroup;

    /// <summary>
    /// Tile size
    /// </summary>
    [SerializeField]
    private float tileSize = 1000;

    /// <summary>
    /// Reference to shader id.
    /// </summary>
    private int idGlobalHeatGradient;
    private int idGlobalMinValue;
    private int idGlobalMaxValue;

    /// <summary>
    /// Stencil render feature we look for.
    /// </summary>
    [SerializeField]
    private string stencilRenderFeatureName = "StencilRender";

    /// <summary>
    /// Quick hack to make layermask updates possible.
    /// </summary>
    

    public void ChangeStencilSettings(LayerMask mask)
    {
        // Try to find the render feature.

        for (int i = 0; i < forwardRenderer.rendererFeatures.Count; i++)
        {
            if (string.Compare(forwardRenderer.rendererFeatures[i].name, stencilRenderFeatureName, true) == 0)
            {
                var renderObjects = (RenderObjects)forwardRenderer.rendererFeatures[i];

                if (renderObjects != null)
                {
                    
                    renderObjects.settings.filterSettings.LayerMask = mask;

                    forwardRenderer.SetDirty();
                    
                    return;
                }
            }
        }
    }


    public void ProjectOnBuildings(bool state)
    {
        if (state == true)
        {
            ChangeStencilSettings((1 << LayerMask.NameToLayer("Buildings") | 1 << LayerMask.NameToLayer("Terrain")));
        }
        else
        {
            ChangeStencilSettings(1 << LayerMask.NameToLayer("Terrain"));
        }

    }

    /// <summary>
    /// Change the active layer.
    /// </summary>
    /// <param name="layer"></param>
    public void SetActiveLayer(string layer)
    {
        // First active toggle.
        Toggle t = toggleGroup.GetFirstActiveToggle();

#if UNITY_EDITOR
        Debug.Log("first active: " + t.name);
#endif
        activeLayer = t.name;

        // Use the rawimage in the toggle for the legend.
        var legendImage = (Texture2D)t.GetComponentInChildren<RawImage>(true).texture;

        Shader.SetGlobalTexture(idGlobalHeatGradient, legendImage);

        // If we are active we have to update our tiles.
        if (isActiveAndEnabled == true)
        {

            // If true, already existed.
            if (CheckGeometry() == true)
            {
                // Just update tiles.
                if (activeCoroutine != null)
                {
                    StopCoroutine(activeCoroutine);
                    activeCoroutine = StartCoroutine(UpdateTiles());
                }
            }
        }
    }

    /// <summary>
    /// Reset tiles (when changing scale)
    /// </summary>
    private void ResetTiles()
    {
        foreach (var kvp in RDToTileDictionary)
        {
            kvp.Value.SetActive(false);
        }
    }

    /// <summary>
    /// Get the layer configuration.
    /// </summary>
    /// <returns></returns>

    private IEnumerator GetLayerConfigurationJSON()
    {

        Debug.Log("Fetching layer info from base URL: " + BaseURL);

        UnityWebRequest www = UnityWebRequest.Get(BaseURL + @"tiles.json");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            FetchLayerInfoJSON(www.downloadHandler.text);
        }

    }

    /// <summary>
    /// Fetch the layer information JSON.
    /// </summary>
    private void FetchLayerInfoJSON(string data)
    {
        layerInformation.Clear();

        var layerInfo = JsonUtility.FromJson<HeatMonitor.HeatMonitorJSON>(data);
        if (layerInfo != null)
        {

#if UNITY_EDITOR
            Debug.Log("Received layer info");
#endif

            // Build lookup table.
            for (int i = 0; i < layerInfo.layers.Count; i++)
            {
                var layer = layerInfo.layers[i];
                layerInformation.Add(layer.id, layer);
            }
        }

    }

    /// <summary>
    /// Called by unity
    /// </summary>
    private void Start()
    {
        StartCoroutine(GetLayerConfigurationJSON());
    }


    /// <summary>
    /// Called by Unity
    /// </summary>
    private void Awake()
    {
        layerInformation = new Dictionary<string, HeatMonitor.HeatMonitorJSON.Layer>();

        // We get the property id's once for efficiency
        idGlobalHeatGradient = Shader.PropertyToID("_HeatGradientTex");
        idGlobalMinValue = Shader.PropertyToID("_minValue");
        idGlobalMaxValue = Shader.PropertyToID("_maxValue");

        Shader.SetGlobalFloat(idGlobalMinValue, minValue);
        Shader.SetGlobalFloat(idGlobalMaxValue, maxValue);



    }

    /// <summary>
    /// Returns tile URL
    /// </summary>
    /// <param name="whichLayer">which layer is requested</param>
    /// <returns></returns>
    private string GetTileURL(string whichLayer)
    {
        return BaseURL + whichLayer + @"/";
    }


    /// <summary>
    /// GO is disabled.
    /// </summary>
    private void OnDisable()
    {
        // Hide generated tiles.
        targetParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// GO is enabled.
    /// </summary>
    private void OnEnable()
    {
        targetParent.gameObject.SetActive(true);
    }

    private bool CheckGeometry()
    {
        // Only generated if needed.
        if (geometryGenerated == false)
        {
            // Only when we have a layer...
            GenerateGeometry();
            geometryGenerated = true;

            // Update tiles:
            SetActiveLayer(string.Empty);

            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);

            }

            activeCoroutine = StartCoroutine(UpdateTiles());

            // Geometry was not generated before
            return false;


        }

        // It was generated before.
        return true;
    }



    /// <summary>
    /// Generate geometry.
    /// </summary>
    private void GenerateGeometry()
    {
        GameObject tile = null;
        Material matInstance = null;

        var rdBL = applicationConfiguration.ConfigurationFile.BottomLeftRD;
        var rdTR = applicationConfiguration.ConfigurationFile.TopRightRD;


        // Create tiles:
        int xTiles = Mathf.CeilToInt((float)(rdTR.x - rdBL.x) / tileSize);
        int yTiles = Mathf.CeilToInt((float)(rdTR.y - rdBL.y) / tileSize);


        string tileName = @"heat_";
        string baseURL = GetTileURL(activeLayer);

        RDToTileDictionary = new Dictionary<System.Tuple<int, int>, GameObject>();

        // Create tiles:
        for (int y = 0; y < yTiles; y++)
        {
            float yPosition = (y * tileSize);

            for (int x = 0; x < xTiles; x++)
            {

                // Load appropriate image
                float xPosition = (x * tileSize);


                // [TODO] We should only build tiles that we know that exist!
                {

                    if (UseProjector == true)
                    {
                        // Instantiate the projector prefab instead.
                        tile = Instantiate(prefabProjector);

                        // Set position: (probably needs half tile offset?)
                        tile.transform.position = CoordConvert.RDtoUnity(new Vector3RD(rdBL.x + xPosition + ProjectorTileOffset, rdBL.y + yPosition + ProjectorTileOffset, 0));

                        // We have to use a material per tile unfortunately.
                        var mr = tile.GetComponent<MeshRenderer>();
                        matInstance = Instantiate(mr.sharedMaterial);
                        mr.sharedMaterial = matInstance;
                    }
                    else
                    {
                        matInstance = Instantiate(heatMaterial);

                        Vector3 vTL = CoordConvert.RDtoUnity(new Vector3RD(rdBL.x + xPosition, rdBL.y + yPosition, 0));
                        Vector3 vTR = CoordConvert.RDtoUnity(new Vector3RD(rdBL.x + (xPosition + tileSize), rdBL.y + yPosition, 0));
                        Vector3 vBL = CoordConvert.RDtoUnity(new Vector3RD(rdBL.x + xPosition, rdBL.y + (yPosition + tileSize), 0));
                        Vector3 vBR = CoordConvert.RDtoUnity(new Vector3RD(rdBL.x + (xPosition + tileSize), rdBL.y + (yPosition + tileSize), 0));

                        tile = BuildQuad(vTL, vTR, vBL, vBR, matInstance);
                    }

#if UNITY_EDITOR
                    // In the editor we like a name, just to make debugging easier.
                    tile.name = tileName + "_" + Mathf.RoundToInt((float)rdBL.x + xPosition) + "_" + Mathf.RoundToInt((float)(rdBL.y + yPosition));
#endif
                    tile.transform.SetParent(targetParent);

                    // Store in dictionary.
                    RDToTileDictionary.Add(new System.Tuple<int, int>(x, y), tile);


                }

            }
        }

        activeCoroutine = null;

    }

    /// <summary>
    /// Update tiles of the layer.
    /// </summary>
    private IEnumerator UpdateTiles()
    {
        // First reset tiles:
        ResetTiles();

        var rdBL = applicationConfiguration.ConfigurationFile.BottomLeftRD;
        var rdTR = applicationConfiguration.ConfigurationFile.TopRightRD;

        // Create tiles:
        int xTiles = Mathf.CeilToInt((float)(rdTR.x - rdBL.x) / tileSize);
        int yTiles = Mathf.CeilToInt((float)(rdTR.y - rdBL.y) / tileSize);


        string baseURL = GetTileURL(activeLayer);

        Debug.Log("Base url: " + baseURL);

        HeatMonitorJSON.Layer activeLayerInfo = null;

        if (layerInformation.TryGetValue(activeLayer, out activeLayerInfo) == false)
        {
            // No information found for this layer.
            Debug.Log("No layer information for: " + activeLayer);

        }
        else
        {
            // Scale is always 0 - 1 (0-255) but we need to update the UI?
            Shader.SetGlobalFloat(idGlobalMinValue, 0); // activeLayerInfo.legend.min);
            Shader.SetGlobalFloat(idGlobalMaxValue, 1); // activeLayerInfo.legend.max);
        }


        // Create tiles:
        for (int y = 0; y < yTiles; y++)
        {
            int tileY = (int)(rdBL.y + (y * tileSize));

            for (int x = 0; x < xTiles; x++)
            {
                // Load appropriate image
                int tileX = (int)(rdBL.x + (x * tileSize));

                // Check if we can ignore this tile if it is not in our layer bounds
                if (activeLayerInfo != null)
                {
                    if (WithinLayer(tileX, tileY, ref activeLayerInfo) == false)
                    {
                        continue;
                    }
                }

                string fileURL = baseURL + tileX + "_" + tileY + ".png";

                // If the file exist we load it.
                {
                    // Get item from the tuple if present.
                    var key = new Tuple<int, int>(x, y);
                    if (RDToTileDictionary.TryGetValue(key, out var tileGO) == true)
                    {
                        // [TODO] We should actually use a coroutine queue which loads 4 at the same time (webserver recommended value)

                        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(fileURL))
                        {

                            yield return uwr.SendWebRequest();

                            if (uwr.result != UnityWebRequest.Result.Success)
                            {
                                //Debug.Log($"Does not exist: {fileURL}");

                                // Disable this tile.
                                tileGO.SetActive(false);
                            }
                            else
                            {
                                // Get downloaded asset bundle
                                var loadedTexture = (Texture2D)DownloadHandlerTexture.GetContent(uwr);
                                loadedTexture.wrapMode = TextureWrapMode.Clamp;

                                // Update texture
                                tileGO.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = loadedTexture;
                                tileGO.SetActive(true);
                                // Tiny bit of delay for webserver.
                                yield return new WaitForSeconds(0.01f);
                            }
                        }



                    }
                }

            }
        }
    }

    /// <summary>
    /// Are the tile coordinates with in the layer?
    /// </summary>
    /// <param name="x">x RD coordinate</param>
    /// <param name="y">y RD coordinate</param>
    /// <param name="layer">Layer to check</param>
    /// <returns>true if within bounds, else false</returns>
    private bool WithinLayer(int x, int y, ref HeatMonitorJSON.Layer layer)
    {
        return x >= layer.tiles.startx && y >= layer.tiles.starty && x <= layer.tiles.endx && y <= layer.tiles.endy;
    }

    /// <summary>
    /// Change legend range: max value.
    /// </summary>
    /// <param name="value">New max value</param>
    public void ChangeLegendMax(string value)
    {
        if (float.TryParse(value, out var f) == true)
        {
            maxValue = f;
            Shader.SetGlobalFloat(idGlobalMaxValue, maxValue);
        }
    }

    /// <summary>
    /// Change legend range: min value.
    /// </summary>
    /// <param name="value">New min value</param>
    public void ChangeLegendMin(string value)
    {
        if (float.TryParse(value, out var f) == true)
        {
            minValue = f;
            Shader.SetGlobalFloat(idGlobalMinValue, minValue);
        }

    }

    /// <summary>
    /// Build a rectangular quad based on bounds. Only used when not using the projector method.
    /// </summary>
    /// <param name="b">Coordinates of corners</param>
    /// <returns>Mesh at the specified coordinates</returns>
    public static GameObject BuildQuad(Vector3 tl, Vector3 tr, Vector3 bl, Vector3 br, Material material)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = tl;
        vertices[1] = tr;
        vertices[2] = bl;
        vertices[3] = br;

        Mesh gridMesh = new Mesh()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt16,
            vertices = vertices,
            triangles = indicesQuad,
            uv = uvQuads
        };


        GameObject go = new GameObject();
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();

        mf.sharedMesh = gridMesh;
        mr.sharedMaterial = material;

        // [TODO] we know all normals are up anyway?
        gridMesh.RecalculateNormals();
        gridMesh.RecalculateBounds();

        return go;

    }

}
