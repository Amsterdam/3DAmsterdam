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
    public bool isBusyGettingBagID = false;
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

    private Layer containerLayer;

	private void Start()
	{
        containerLayer = gameObject.GetComponent<Layer>();
    }

	void Update()
    {
        if (isBusyGettingBagID)
            return;
 
        if (Input.GetKey(KeyCode.LeftControl))
        {

            if (Input.GetKeyDown(KeyCode.G))
            {

                containerLayer.UnhideAll();
            }
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            containerLayer.Hide(selectedID);
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
        selectedID = "";
        ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
        isBusyGettingBagID = true;
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
            StartCoroutine(ImportBAG.Instance.CallAPI(ApiUrl, id, RetrieveType.Pand)); // laat het BAG UI element zien
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
        int vertexIndex = hit.triangleIndex * 3;
        if (vertexIndex > mesh.uv2.Length) 
        {
            Debug.LogWarning("UV index out of bounds");
            yield break;
        }
        var uv = mesh.uv2[vertexIndex];
        var gameObjectToHighlight = hit.collider.gameObject;
        var hitVisibleObject = true;

        var newHit = hit;
        var lastHit = hit;

        if (uv.y == 0.2f)
        {
            hitVisibleObject = false;
            Vector3 lastPoint = hit.point;
            while (uv.y == 0.2f)
            {
                Vector3 h = newHit.point + (ray.direction * 0.01f);
                
                Debug.Log("Ray point: " + h);
                ray = new Ray(h, ray.direction);
                if (Physics.Raycast(ray, out newHit, 10000, clickCheckLayerMask.value))
                {
                    uv = mesh.uv2[newHit.triangleIndex * 3];
                    //Debug.DrawRay(ray.origin, ray.direction * 1000, Color.blue, 1000);
                    Debug.DrawLine(lastPoint, newHit.point, Color.blue, 1000);
                    lastPoint = newHit.point;
                    lastHit = newHit;
                    if (uv.y != 0.2f) 
                    {
                        hitVisibleObject = true;
                        Debug.Log("Hit visible");
                    }
                }
                else 
                {
                    break;
                }
                yield return new WaitForSeconds(1f);
            }
        }

        hit = lastHit;
        if (hitVisibleObject) 
        {
            DisplayBAGData.Instance.PrepareUI();
        }
        tileHandler.GetIDData(gameObjectToHighlight, hit.triangleIndex * 3, UseObjectID);
    }
}
