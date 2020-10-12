using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LayerSystem;

public class GetBAGIDs : MonoBehaviour
{
    public TileHandler tileHandler;
    public GameObject BuildingContainer;
    public bool isBusy = false;
    private Ray ray;
    private string id = "";
    private GameObject selectedTile;
    private bool mouseReleased = true;

    private bool meshCollidersAttached = false;

    void Update()
    {
        if (isBusy)
        {
            return;
        }
        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0))
        {
            GetBagID();
        }
    }

    private void GetBagID()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        isBusy = true;
        StartCoroutine(GetIDData(ray, (value) => { UseObjectID(value); }));
    }

    private void UseObjectID(string id)
    {
        if (id == "null")
        {
            BuildingContainer.GetComponent<LayerSystem.Layer>().UnHighlightAll();
        }
        else
        {
            BuildingContainer.GetComponent<LayerSystem.Layer>().Highlight(id);
        }
        isBusy = false;
    }
    IEnumerator LoadMeshColliders()
    {
        MeshCollider meshCollider;
        MeshFilter[] meshFilters = BuildingContainer.GetComponentsInChildren<MeshFilter>();
        if (meshFilters == null)
        {
            isBusy = false;
            id = "null";
            meshCollidersAttached = true;
            yield break;
        }
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter == null)
            {

                isBusy = false;

                id = "null";

            }
            if (meshFilter == null)
            {

                isBusy = false;

                id = "null";
            }

            meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshFilter.gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
            }
        }
        meshCollidersAttached = true;
        Debug.Log("MeshColliders attached");
    }

    IEnumerator GetIDData(Ray ray, System.Action<string> callback)
    {
        tileHandler.pauseLoading = true;
        meshCollidersAttached = false;
        StartCoroutine(LoadMeshColliders());
        yield return new WaitUntil(() => meshCollidersAttached == true);
        yield return null;
        RaycastHit Hit;

        if (Physics.Raycast(ray, out Hit, 10000) == false)
        {
            id = "null";
            isBusy = false;
            tileHandler.pauseLoading = false;
            callback("null");
            yield break;
        }

        DisplayBAGData.Instance.PrepareUI();

        selectedTile = Hit.collider.gameObject;
        string name = Hit.collider.gameObject.GetComponent<MeshFilter>().mesh.name;
        Debug.Log(name);
        string dataName = name.Replace(" Instance", "");
        dataName = dataName.Replace("mesh", "building");
        dataName = dataName.Replace("-", "_") + "-data";
        string dataURL = "https://acc.3d.amsterdam.nl/web/data/feature-Link-BAGid/buildings/objectdata/" + dataName;
        Debug.Log(dataURL);
        ObjectMappingClass data;
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                callback("null");
                id = "null";
            }
            else
            {
                ObjectData objectMapping = Hit.collider.gameObject.GetComponent<ObjectData>();
                if (objectMapping is null)
                {
                    objectMapping = Hit.collider.gameObject.AddComponent<ObjectData>();
                }

                AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];
                int vertexIndex = Hit.triangleIndex * 3;
                int idIndex = data.vectorMap[vertexIndex];
                id = data.ids[idIndex];
                StartCoroutine(ImportBAG.Instance.CallAPI("https://api.data.amsterdam.nl/bag/v1.1/pand/", id, RetrieveType.Pand)); // laat het BAG UI element zien
                objectMapping.highlightIDs.Clear();
                objectMapping.highlightIDs.Add(id);
                objectMapping.ids = data.ids;
                objectMapping.uvs = data.uvs;
                objectMapping.vectorMap = data.vectorMap;
                objectMapping.mappedUVs = data.mappedUVs;

                newAssetBundle.Unload(true);
            }
        }

        yield return null;
        tileHandler.pauseLoading = false;
        isBusy = false;
        callback(id);
    }

}
