using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LayerSystem;
using Amsterdam3D.CameraMotion;
using UnityEngine.EventSystems;
using System.Linq;
using Amsterdam3D.Interface;

public class SelectByID : MonoBehaviour
{
    public TileHandler tileHandler;

    private bool isWorkingOnSelection = false;

    private Ray ray;

    private string lastSelectedID = "";
    private List<string> selectedIDs;

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

    private const string emptyID = "null";

    private void Awake()
	{
        selectedIDs = new List<string>();
        containerLayer = gameObject.GetComponent<Layer>();
    }

	void Update()
    {
        multiSelection = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
 
        if (Input.GetMouseButtonDown(0))
        {
            mouseClickTime = Time.time;
            mousePosition = Input.mousePosition;
        }
        else if ((Input.GetMouseButtonUp(1) && (selectedIDs.Count == 0)) || (Input.GetMouseButtonUp(0) && (Time.time - mouseClickTime) < clickTimer && Vector3.Distance(mousePosition, Input.mousePosition) < mouseDragDistance && !EventSystem.current.IsPointerOverGameObject() && CameraModeChanger.Instance.CameraMode == CameraMode.GodView))
        {
            //If we did a left mouse click, or a right mouse click with no existing selection, find the selected ID under our pointer
            FindSelectedID();
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                HideSelectedIDs();
            }
        }
    }

    /// <summary>
    /// Select a mesh ID underneath the pointer
    /// </summary>
    private void FindSelectedID()
    {
        ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
        //Try to find a selected mesh ID and highlight it
        StartCoroutine(GetSelectedMeshIDData(ray, (value) => { HighlightSelectedID(value); }));
    }

    /// <summary>
    /// Find selected ID's based on a area selection done by our selectiontools.
    /// We find BAG id's within an area using a WebRequest and an API.
    /// </summary>
    public void FindSelectedIDsInArea()
    {
        SelectionTools selectionTools = FindObjectOfType<SelectionTools>();
        var vertices = selectionTools.GetVertices();
        var bounds = selectionTools.GetBounds();
        containerLayer.AddMeshColliders();
        StartCoroutine(GetAllIDsInRange(vertices[0], vertices[2], HighlightSelectionIDs));
    }

    private void HighlightSelectedID(string id)
    {
        if (!multiSelection)
		{
            //Not multiselecting? Always clear our previous selection list.
            selectedIDs.Clear();
        }
        if (id != emptyID)
        {
            selectedIDs.Add(id);
            lastSelectedID = id;

            HighlightSelectionIDs(selectedIDs);
            isWorkingOnSelection = false;
        }
        else{
            ClearSelection();
        }
    }

    private void HighlightSelectionIDs(List<string> ids)
    {
        selectedIDs.AddRange(ids);
        lastSelectedID = (selectedIDs.Count > 0) ? selectedIDs.Last() : emptyID;
        containerLayer.Highlight(selectedIDs);
    }

    public void ClearSelection()
	{
		lastSelectedID = emptyID;
		selectedIDs.Clear();
        HighlightSelectionIDs(selectedIDs);
    }

    public void HideSelectedIDs()
    {
        if (selectedIDs.Count > 0)
        {
            containerLayer.Hide(selectedIDs);
        }
    }
    public void UnhideAll()
    {
        lastSelectedID = emptyID;
        selectedIDs.Clear();
        containerLayer.Hide(selectedIDs);
    }


    public void ShowBAGDataForSelectedID()
    {
        DisplayBAGData.Instance.PrepareUI();
        StartCoroutine(ImportBAG.Instance.CallAPI(ApiUrl, lastSelectedID, RetrieveType.Pand)); // laat het BAG UI element zien
    }

    IEnumerator GetSelectedMeshIDData(Ray ray, System.Action<string> callback)
    {
        isWorkingOnSelection = true;
        tileHandler.pauseLoading = true;
        containerLayer.AddMeshColliders();

        //Didn't hit anything
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10000, clickCheckLayerMask.value) == false)
        {
            isWorkingOnSelection = false;
            tileHandler.pauseLoading = false;
            callback(emptyID);
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

    IEnumerator GetAllIDsInRange(Vector3 min, Vector3 max, System.Action<List<string>> callback = null)
    {
        var wgsMin = ConvertCoordinates.CoordConvert.UnitytoRD(min);
        var wgsMax = ConvertCoordinates.CoordConvert.UnitytoRD(max);

        List<string> ids = new List<string>();
        string url = "https://map.data.amsterdam.nl/maps/bag?REQUEST=GetFeature&SERVICE=wfs&version=2.0.0&typeName=bag:pand&propertyName=bag:id&outputFormat=csv&bbox=";
        // construct url string
        url += wgsMin.x + "," + wgsMin.y + "," + wgsMax.x + "," + wgsMax.y;
        var hideRequest = UnityWebRequest.Get(url);

        yield return hideRequest.SendWebRequest();
        if (hideRequest.isNetworkError || hideRequest.isHttpError)
        {
            WarningDialogs.Instance.ShowNewDialog("Sorry, door een probleem met de BAG id server is een selectie maken tijdelijk niet mogelijk.");
        }
        else
        {
            string dataString = hideRequest.downloadHandler.text;

            var csv = SplitCSV(dataString);
            int returnCounter = 0;
            // hard coded for this api request
            for (int i = 3; i < csv.Count; i += 2)
            {
                var numberOnlyString = GetNumbers(csv[i]);
                ids.Add(numberOnlyString);
                returnCounter++;
                if (returnCounter > 100)
                {
                    yield return null;
                    returnCounter = 0;
                }
            }
        }

        callback?.Invoke(ids);
        yield return null;
    }


    public List<string> SplitCSV(string csv)
    {
        List<string> splitString = new List<string>();
        bool inBracket = false;
        int startIndex = 0;

        for (int i = 0; i < csv.Length; i++)
        {
            if (csv[i] == '"')
            {
                inBracket = !inBracket;
            }

            else if (!inBracket)
            {
                if (csv[i] == ',' || csv[i] == '\n')
                {
                    splitString.Add(csv.Substring(startIndex, i - startIndex));
                    startIndex = i + 1;
                }
            }
        }

        return splitString;
    }

    private static string GetNumbers(string input)
    {
        return new string(input.Where(c => char.IsDigit(c)).ToArray());
    }
}
