using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LayerSystem;
using Amsterdam3D.CameraMotion;
using UnityEngine.EventSystems;

public class SelectByID : MonoBehaviour
{
    public TileHandler tileHandler;
    public bool isBusyGettingBagID = false;
    private Ray ray;

    private string selectedID = "";
    private List<int> selectedIDs;

    private const string ApiUrl = "https://api.data.amsterdam.nl/bag/v1.1/pand/";
    private float mouseClickTime;
    private const float mouseDragDistance = 10.0f; //10 pixels results in a drag
    private Vector2 mousePosition;
    [SerializeField]
    private float clickTimer = 0.3f;

    [SerializeField]
    private LayerMask clickCheckLayerMask;
    private Layer containerLayer;

    private bool multiSelection = false;

	private void Awake()
	{
        selectedIDs = new List<int>();
        containerLayer = gameObject.GetComponent<Layer>();
    }

	void Update()
    {
        if (isBusyGettingBagID)
            return;
 
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.G))
        {
            containerLayer.UnhideAll();
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            containerLayer.Hide(selectedID);
        }

        multiSelection = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetMouseButtonDown(0))
        {
            mouseClickTime = Time.time;
            mousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(1) || ((Time.time - mouseClickTime) < clickTimer && Vector3.Distance(mousePosition, Input.mousePosition) < mouseDragDistance && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0) && CameraModeChanger.Instance.CameraMode == CameraMode.GodView))
        {
            FindSelectedID();
        }
    }

    private void FindSelectedID()
    {
        selectedID = "";
        ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);

        //Try to find a selected mesh ID and highlight it
        StartCoroutine(GetSelectedMeshIDData(ray, (value) => { HighlightSelectedID(value); }));
    }

    private void HighlightSelectedID(string id)
    {
        if (!multiSelection && id == "null")
        {
            containerLayer.UnHighlightAll();
        }
        else
        {
            containerLayer.Highlight(id);
            selectedID = id;
        }
        isBusyGettingBagID = false;
    }

    public void HideSelectedID()
    {
        if (selectedID != "null")
        {
            containerLayer.Hide(selectedID);
        }
    }

    public void ClearSelection()
    {
        selectedIDs.Clear();
    }

    public void ShowBAGDataForSelectedID()
    {
        DisplayBAGData.Instance.PrepareUI();
        StartCoroutine(ImportBAG.Instance.CallAPI(ApiUrl, selectedID, RetrieveType.Pand)); // laat het BAG UI element zien
    }

    IEnumerator GetSelectedMeshIDData(Ray ray, System.Action<string> callback)
    {
        isBusyGettingBagID = true;
        tileHandler.pauseLoading = true;
        containerLayer.AddMeshColliders();

        //Didn't hit anything
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10000, clickCheckLayerMask.value) == false)
        {
            isBusyGettingBagID = false;
            tileHandler.pauseLoading = false;
            callback("null");
            yield break;
        }

        //Get the mesh we selected and check if it has an ID stored in the UV2 slot
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

        //Keep piercing forward with raycasts untill we find a visible UV
        if (uv.y == 0.2f)
        {
            hitVisibleObject = false;
            Vector3 lastPoint = hit.point;
            while (uv.y == 0.2f)
            {
                Vector3 hitPoint = newHit.point + (ray.direction * 0.01f);
                ray = new Ray(hitPoint, ray.direction);
                if (Physics.Raycast(ray, out newHit, 10000, clickCheckLayerMask.value))
                {
                    uv = mesh.uv2[newHit.triangleIndex * 3];
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
                yield return new WaitForEndOfFrame();
            }
        }
        hit = lastHit;

        //Not retrieve the selected BAG ID tied to the selected triangle
        tileHandler.GetIDData(gameObjectToHighlight, hit.triangleIndex * 3, HighlightSelectedID);
    }
}
