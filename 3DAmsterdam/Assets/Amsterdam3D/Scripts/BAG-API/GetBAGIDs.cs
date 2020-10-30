using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LayerSystem;
using Amsterdam3D.CameraMotion;
using UnityEngine.EventSystems;

public class GetBAGIDs : MonoBehaviour
{
	public TileHandler tileHandler;
    public GameObject BuildingContainer;
    public bool isBusy = false;
    private Ray ray;
    private string id = "";
    private GameObject selectedTile;
    private bool meshCollidersAttached = false;
    private string selectedID = "";
    private const string ApiUrl = "https://api.data.amsterdam.nl/bag/v1.1/pand/";
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

        if (Input.GetKey(KeyCode.LeftControl))
        {

            if (Input.GetKeyDown(KeyCode.H))
            {

                BuildingContainer.GetComponent<Layer>().UnhideAll();
            }
        }

       else if (Input.GetKeyDown(KeyCode.H))
        {

            BuildingContainer.GetComponent<Layer>().Hide(selectedID);
        }


        if (Input.GetMouseButtonDown(0))
        {
            mouseClickTime = Time.time;
            mousePosition = Input.mousePosition;
        }
        else if ((Time.time - mouseClickTime) < clickTimer && Vector3.Distance(mousePosition, Input.mousePosition) < mouseDragDistance && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0) && CameraModeChanger.Instance.CameraMode == CameraMode.GodView)
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
        StartCoroutine(ImportBAG.Instance.CallAPI(ApiUrl, id, RetrieveType.Pand)); // laat het BAG UI element zien
        if (id == "null")
        {
            BuildingContainer.GetComponent<LayerSystem.Layer>().UnHighlightAll();
        }
        else
        {
            BuildingContainer.GetComponent<LayerSystem.Layer>().Highlight(id);
            selectedID = id;
        }
        isBusy = false;
    }

    private void OnMeshColliderAttached(bool value) 
    {
        meshCollidersAttached = value;
    }
    
    IEnumerator GetIDData(Ray ray, System.Action<string> callback)
    {
        tileHandler.pauseLoading = true;
        meshCollidersAttached = false;
        BuildingContainer.GetComponent<LayerSystem.Layer>().LoadMeshColliders(OnMeshColliderAttached);
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


        Mesh mesh = hit.collider.gameObject.GetComponent<MeshFilter>().mesh;
        Vector2 uv = mesh.uv2[hit.triangleIndex];

        DisplayBAGData.Instance.PrepareUI();

        selectedTile = hit.collider.gameObject;
        tileHandler.GetIDData(hit.collider.gameObject, hit.triangleIndex * 3, UseObjectID);
       
    }

   

}
