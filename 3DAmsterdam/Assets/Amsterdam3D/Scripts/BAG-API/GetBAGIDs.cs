using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LayerSystem;
using Amsterdam3D.CameraMotion;
using UnityEngine.EventSystems;

public class GetBAGIDs : MonoBehaviour
{
	private const string ApiUrl = "https://api.data.amsterdam.nl/bag/v1.1/pand/";
	public TileHandler tileHandler;
    public GameObject BuildingContainer;
    public bool isBusy = false;
    private Ray ray;
    private string id = "";
    private GameObject selectedTile;
    private bool meshCollidersAttached = false;

    private float mouseClickTime;
    private const float mouseDragDistance = 10.0f; //10 pixels results in a drag
    private Vector2 mousePosition;
    [SerializeField]
    private float clickTimer = 0.3f;

    [SerializeField]
    private LayerMask clickCheckLayerMask;

    void Update()
    {
        if (isBusy)
        {
            return;
        }

        if(Input.GetMouseButtonDown(0))
        {
            mouseClickTime = Time.time;
            mousePosition = Input.mousePosition;
        }
        else if ((Time.time-mouseClickTime) < clickTimer && Vector3.Distance(mousePosition,Input.mousePosition) < mouseDragDistance && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0) && CameraModeChanger.Instance.CameraMode == CameraMode.GodView)
        {
            GetBagID();
        }
    }

    private void GetBagID()
    {
        ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
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
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10000, clickCheckLayerMask.value) == false)
        {
            id = "null";
            isBusy = false;
            tileHandler.pauseLoading = false;
            callback("null");
            yield break;
        }

        DisplayBAGData.Instance.PrepareUI();

        selectedTile = hit.collider.gameObject;
        string name = hit.collider.gameObject.GetComponent<MeshFilter>().mesh.name;
        Debug.Log(name);
        string dataName = name.Replace(" Instance", "");
        dataName = dataName.Replace("mesh", "building");
        dataName = dataName.Replace("-", "_") + "-data";
        string dataURL = Constants.TILE_METADATA_URL + dataName;
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
                ObjectData objectMapping = hit.collider.gameObject.GetComponent<ObjectData>();
                if (objectMapping is null)
                {
                    objectMapping = hit.collider.gameObject.AddComponent<ObjectData>();
                }

                AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];
                int vertexIndex = hit.triangleIndex * 3;
                int idIndex = data.vectorMap[vertexIndex];
                id = data.ids[idIndex];
                StartCoroutine(ImportBAG.Instance.CallAPI(ApiUrl, id, RetrieveType.Pand)); // laat het BAG UI element zien
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
