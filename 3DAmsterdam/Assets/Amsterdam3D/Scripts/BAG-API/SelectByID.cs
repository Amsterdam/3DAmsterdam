using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LayerSystem;
using Amsterdam3D.CameraMotion;
using UnityEngine.EventSystems;
using System.Linq;
using Amsterdam3D.Interface;
using System;
using System.Globalization;

public class SelectByID : Interactable
{
    public TileHandler tileHandler;

    private Ray ray;
    private string lastSelectedID = "";

    public static List<string> selectedIDs;

    [SerializeField]
    private LayerMask clickCheckLayerMask;
    private AssetbundleMeshLayer containerLayer;

    private const string emptyID = "null";

	private string bagIdRequestServiceBoundingBoxUrl = "https://map.data.amsterdam.nl/maps/bag?REQUEST=GetFeature&SERVICE=wfs&version=2.0.0&typeName=bag:pand&propertyName=bag:id&outputFormat=csv&bbox=";

	private string bagIdRequestServicePolygonUrl = "https://map.data.amsterdam.nl/maps/bag?REQUEST=GetFeature&SERVICE=wfs&version=2.0.0&typeName=bag:pand&propertyName=bag:id&outputFormat=csv&Filter=";

    private const int maximumRayPiercingLoops = 20;

    private RaycastHit lastRaycastHit;

    private void Awake()
	{
        contextMenuState = ContextPointerMenu.ContextState.BUILDING_SELECTION;

        selectedIDs = new List<string>();
        containerLayer = gameObject.GetComponent<AssetbundleMeshLayer>();
    }

    public override void Select()
    {
        base.Select();
        FindSelectedID();
    }
    public override void SecondarySelect()
    {
        base.Select();
        //On a secondary click, only select if we did not make a multisselection yet.
        if (selectedIDs.Count < 2) Select();
    }

    public override void Deselect()
	{
		base.Deselect();
        ClearSelection();
    }

    /// <summary>
    /// Select a mesh ID underneath the pointer
    /// </summary>
    private void FindSelectedID()
    {
        //Clear selected ids if we are not adding to a multiselection
        if (!Selector.doingMultiselect) selectedIDs.Clear();

        //Try to find a selected mesh ID and highlight it
        StartCoroutine(GetSelectedMeshIDData(Selector.mainSelectorRay, (value) => { HighlightSelectedID(value); }));
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
       
        //Polygon selection
        StartCoroutine(GetAllIDsInPolygonRange(vertices.ToArray(), HighlightObjectsWithIDs));
    }

    /// <summary>
    /// Add a single object to highlight selection. If we clicked an empty ID, clear the selection if we are not in multiselect
    /// </summary>
    /// <param name="id">The object ID</param>
    public void HighlightSelectedID(string id)
    {
        if (id == emptyID && !Selector.doingMultiselect)
        {
            ClearSelection();
        }
        else{
            List<string> singleIdList = new List<string>();
            //Allow clicking a single object multiple times to move them in and out of our selection
            if (Selector.doingMultiselect && selectedIDs.Contains(id))
            {
                selectedIDs.Remove(id);
            }
            else{
                singleIdList.Add(id);
            }
            HighlightObjectsWithIDs(singleIdList);
        }
    }

    /// <summary>
    /// Removes an object with this specific ID from the selected list, and update the highlights
    /// </summary>
    /// <param name="id">The unique ID of this item</param>
    public void DeselectSpecificID(string id)
    {
        if (selectedIDs.Contains(id))
        {
            selectedIDs.Remove(id);
            HighlightObjectsWithIDs(selectedIDs);
 
        }
    }

    /// <summary>
    /// Add list of ID's to our selected objects list
    /// </summary>
    /// <param name="ids">List of IDs to add to our selection</param>
    private void HighlightObjectsWithIDs(List<string> ids = null)
    {
        if(ids != null) selectedIDs.AddRange(ids);
        selectedIDs = selectedIDs.Distinct().ToList(); //Filter out any possible duplicates

        lastSelectedID = (selectedIDs.Count > 0) ? selectedIDs.Last() : emptyID;
        containerLayer.Highlight(selectedIDs);

        //Specific context menu /sidepanel items per selection count
        if (selectedIDs.Count == 1)
		{
            ShowBAGDataForSelectedID(lastSelectedID);
            ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.BUILDING_SELECTION);
		}
		else if (selectedIDs.Count > 1)
		{
			ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.MULTI_BUILDING_SELECTION);
            //Update sidepanel outliner
            ObjectProperties.Instance.OpenPanel("Selectie", true);
            ObjectProperties.Instance.AddTitle("Geselecteerde panden");
            foreach (var id in selectedIDs)
            {
                ObjectProperties.Instance.AddSelectionOutliner(this.gameObject, "Pand " + id, id);
            }
            ObjectProperties.Instance.RenderThumbnail(true);
        }
		else
		{
            ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.DEFAULT);
		}
	}

    /// <summary>
    /// Clear our list of selected objects, and update the highlights
    /// </summary>
    public void ClearSelection()
	{
		if (selectedIDs.Count != 0)
		{
			lastSelectedID = emptyID;
			selectedIDs.Clear();
        }

        ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.DEFAULT);

        //Remove highlights by highlighting our empty list
        containerLayer.Highlight(selectedIDs);
	}

    /// <summary>
    /// Hides all objects that matches the list of ID's, and remove them from our selection list.
    /// </summary>
    public void HideSelectedIDs()
    {
        if (selectedIDs.Count > 0)
        {
            //Adds selected ID's to our hidding objects of our layer
            containerLayer.Hide(selectedIDs);
            selectedIDs.Clear();
        }

        //If we hide something, make sure our context menu is reset to default again.
        //ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.DEFAULT);
    }

    /// <summary>
    /// Shows all hidden objects by clearing our selection and hiding that empty list
    /// </summary>
    public void UnhideAll()
    {
        lastSelectedID = emptyID;
        selectedIDs.Clear();
        containerLayer.Hide(selectedIDs);
    }

    /// <summary>
    /// Method to allow other objects to display the information panel for the last ID we selected here.
    /// </summary>
    public void ShowBAGDataForSelectedID(string id = "")
    {
        var thumbnailFrom = lastRaycastHit.point + (Vector3.up*300) + (Vector3.back*300);
        var lookAtTarget = lastRaycastHit.point;

        if (id != emptyID)
        {
            ObjectProperties.Instance.OpenPanel("Pand",true);
            if (selectedIDs.Count > 1) ObjectProperties.Instance.AddActionButton("< Geselecteerde panden", (action) => {
                HighlightObjectsWithIDs();
			}
            );
            ObjectProperties.Instance.displayBagData.ShowBuildingData(id);
        }
        else if(lastSelectedID != emptyID)
        { 
            ObjectProperties.Instance.OpenPanel("Pand", true);
            ObjectProperties.Instance.displayBagData.ShowBuildingData(lastSelectedID);
        }
    }

    private void GetAllVertsInSelection(string id)
    {
        containerLayer.GetAllVerts(selectedIDs);
	}

    IEnumerator GetSelectedMeshIDData(Ray ray, System.Action<string> callback)
    {
        //Check area that we clicked, and add the (heavy) mesh collider there
        Vector3 planeHit = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
        containerLayer.AddMeshColliders(planeHit);

        //No fire a raycast towards our meshcolliders to see what face we hit 
        if (Physics.Raycast(ray, out lastRaycastHit, 10000, clickCheckLayerMask.value) == false)
        {
            
            callback(emptyID);
            yield break;
        }

        //Get the mesh we selected and check if it has an ID stored in the UV2 slot
        Mesh mesh = lastRaycastHit.collider.gameObject.GetComponent<MeshFilter>().mesh;
        int vertexIndex = lastRaycastHit.triangleIndex * 3;
        if (vertexIndex > mesh.uv2.Length) 
        {
            Debug.LogWarning("UV index out of bounds. This object/LOD level does not contain highlight/hidden uv2 slot");
            
            yield break;
        }

        var hitUvCoordinate = mesh.uv2[vertexIndex];
        var gameObjectToHighlight = lastRaycastHit.collider.gameObject;

        //Maybe we hit an object with objectdata, that has hidden selections, in that case, loop untill we find something
        ObjectData objectMapping = gameObjectToHighlight.GetComponent<ObjectData>();
        
        if (objectMapping && objectMapping.colorIDMap)
        {
            Color hitPixelColor = objectMapping.GetUVColorID(hitUvCoordinate);
            int raysLooped = 0;
            while (hitPixelColor == ObjectData.HIDDEN_COLOR && raysLooped < maximumRayPiercingLoops)
            {
                Vector3 deeperHitPoint = lastRaycastHit.point + (ray.direction * 0.01f);
                ray = new Ray(deeperHitPoint, ray.direction);
                if (Physics.Raycast(ray, out lastRaycastHit, 10000, clickCheckLayerMask.value))
                {
                    vertexIndex = lastRaycastHit.triangleIndex * 3;
                    hitUvCoordinate = mesh.uv2[vertexIndex];

                    hitPixelColor = objectMapping.GetUVColorID(hitUvCoordinate);
                }
                raysLooped++;
                yield return new WaitForEndOfFrame();
            }
        }
        
        //Not retrieve the selected BAG ID tied to the selected triangle
        containerLayer.GetIDData(gameObjectToHighlight, lastRaycastHit.triangleIndex * 3, HighlightSelectedID);
    }

    IEnumerator GetAllIDsInBoundingBoxRange(Vector3 min, Vector3 max, System.Action<List<string>> callback = null)
    {
        var wgsMin = ConvertCoordinates.CoordConvert.UnitytoRD(min);
        var wgsMax = ConvertCoordinates.CoordConvert.UnitytoRD(max);

        List<string> ids = new List<string>();
        string url = bagIdRequestServiceBoundingBoxUrl;
        // construct url string
        url += wgsMin.x + "," + wgsMin.y + "," + wgsMax.x + "," + wgsMax.y;
        var hideRequest = UnityWebRequest.Get(url);
        print(url);
        yield return hideRequest.SendWebRequest();
        if (hideRequest.isNetworkError || hideRequest.isHttpError)
        {
            WarningDialogs.Instance.ShowNewDialog("Sorry, door een probleem met de BAG id server is een selectie maken tijdelijk niet mogelijk.");
        }
        else
        {
            //Filter out the list of ID's from our returned CSV           
            string dataString = hideRequest.downloadHandler.text;
            var csv = SplitCSV(dataString);
            int returnCounter = 0;
            
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

    public BagDataSelection bagDataSelection;
    IEnumerator GetAllIDsInPolygonRange(Vector3[] points, System.Action<List<string>> callback = null)
    {
        List<string> ids = new List<string>();

        //Create a string array of coordinates for our filter XML
        string coordinates = "";
		for (int i = 0; i < points.Length; i++)
		{
            //convert Unity to WGS84
            var coordinate = ConvertCoordinates.CoordConvert.UnitytoRD(points[i]);
            if (i != 0) coordinates += ",";
            coordinates += coordinate.x.ToString(CultureInfo.InvariantCulture) + " " + coordinate.y.ToString(CultureInfo.InvariantCulture);
        }

        //Our filter according to https://www.mapserver.org/ogc/filter_encoding.html , the type of WFS server used by the API.
        var filter = $"<Filter><Intersects><PropertyName>Geometry</PropertyName><gml:Polygon><gml:outerBoundaryIs><gml:LinearRing><gml:coordinates>{coordinates}</gml:coordinates></gml:LinearRing></gml:outerBoundaryIs></gml:Polygon></Intersects></Filter>";

        var requestUrl = bagIdRequestServicePolygonUrl + UnityWebRequest.EscapeURL(filter);
        var hideRequest = UnityWebRequest.Get(requestUrl);

        yield return hideRequest.SendWebRequest();

        if (hideRequest.isNetworkError || hideRequest.isHttpError)
        {
            WarningDialogs.Instance.ShowNewDialog("Sorry, door een probleem met de BAG id server is een selectie maken tijdelijk niet mogelijk.");
        }
        else
        {
            //Filter out the list of ID's from our returned CSV
            string dataString = hideRequest.downloadHandler.text;
            var csv = SplitCSV(dataString);
            int returnCounter = 0;
            
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

        //Make sure all the metadata requests are done before we continue
        while(TileHandler.runningTileDataRequests > 0)
        {
            yield return new WaitForEndOfFrame();
		}

        callback?.Invoke(ids);
        yield return null;
    }

    //Api bag data selection return format (selective)
    [System.Serializable]
    public class BagDataSelection
    {
        public BagDataObjects[] object_list;
        public int object_count;
        public int page_count;

        [System.Serializable]
        public class BagDataObjects
        {
            public string _id = "";
        }
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
