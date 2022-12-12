using Netherlands3D.Core;
using Netherlands3D.Core.Colors;
using Netherlands3D.Events;
using Netherlands3D.Timeline;
using Netherlands3D.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PolygonVisualiser))] 
public class DrawGeoJSONPolygonsByTime : MonoBehaviour
{
    [SerializeField] private string description = "";
    [SerializeField] private string urlWithGeoJsonData = "https://";

    [SerializeField] private string objectNameProperty = "Projectnaam";
    [SerializeField] private string startTimeProperty = "Start_bouw";
    [SerializeField] private string endTimeProperty = "";

    [SerializeField] private string colorProperty = "Fase_ID";
    [SerializeField] private CoordinateType coordinateType = CoordinateType.WGS84;

    private int colorIndex = 0;
    private string objectName = "";

    [SerializeField] private ColorPalette colorPalette;
    [SerializeField] private ColorPropertyValue[] colorPropertyValues;

    [SerializeField] private float defaultOpacity = 1.0f;
    [SerializeField] private float beforeTimeOpacity = 0.1f;
    [SerializeField] private float afterTimeOpacity = 0.1f;
    [SerializeField] private int featuresBeforeDraw = 50;

    [Header("Listen to")]
    [SerializeField] private DateTimeEvent onCurrentDateChange;

    [Header("Invoke events")]
    [SerializeField] ColorPaletteEvent openLegendWithColorPalette;
    private Coroutine runningDataload;

    private PolygonVisualiser polyonVisualiser;

    private bool init = false;

    Dictionary<string, string> types = new Dictionary<string, string>();

    [System.Serializable]
    public struct ColorPropertyValue
    {
        public string propertyValue;
        public int colorPaletteIndex;
    }
    
    public enum CoordinateType
    {
        RD,
        WGS84
    }

    private void Awake()
    { 
        polyonVisualiser = GetComponent<PolygonVisualiser>();
    }

    private void OnDisable()
    {
        init = true;

        Clear();
    }

    private void Clear()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    private void OnEnable()
    {
        if (!init) return;

        if (colorPalette && openLegendWithColorPalette) openLegendWithColorPalette.started.Invoke(colorPalette);

        LoadData();
    }

    public void LoadData(string geoJsonURL = "")
    {
        Clear();

        var url = (geoJsonURL == "") ? urlWithGeoJsonData : geoJsonURL;
        print($"Starting visualising GeoJSON {description} from {url}");

        if(runningDataload!=null)
        {
            StopCoroutine(runningDataload);
        }

        runningDataload = StartCoroutine(LoadGeoJSON(url));
    }

    private IEnumerator LoadGeoJSON(string url)
    {
        var geoJsonDataRequest = UnityWebRequest.Get(url);
        yield return geoJsonDataRequest.SendWebRequest();
        int feature = 0;

        if (geoJsonDataRequest.result == UnityWebRequest.Result.Success)
        {
            GeoJSON geoJSON = new GeoJSON(geoJsonDataRequest.downloadHandler.text);
            yield return null;

            //We already filtered the request, so we can draw all features
            while (geoJSON.GotoNextFeature())
            {
                feature++;
                colorIndex = 0;

                var type = geoJSON.GetGeometryType();
                var startTimeString = geoJSON.GetPropertyStringValue(startTimeProperty);
                DateTime startDateTime = new DateTime();
                DateTime endDateTime = DateTime.MaxValue;

                //Start time is mandatory
                DateTime.TryParse(startTimeString, out startDateTime);
                //Optional retrieval of endtime 
                if (endTimeProperty.Length > 0)
                {
                    var endTimeString = geoJSON.GetPropertyStringValue(endTimeProperty);
                    DateTime.TryParse(endTimeString, out endDateTime);
                }

                var colorPropertyValue = geoJSON.GetPropertyStringValue(colorProperty);
                types[colorPropertyValue] = colorPropertyValue;

                foreach (var targetColorProperty in colorPropertyValues)
                {
                    if (targetColorProperty.propertyValue == colorPropertyValue)
                        colorIndex = targetColorProperty.colorPaletteIndex;
                }

                objectName = geoJSON.GetPropertyStringValue(objectNameProperty);

                switch (type)
                {
                    case GeoJSON.GeoJSONGeometryType.Polygon:
                        List<List<GeoJSONPoint>> polygon = geoJSON.GetPolygon();
                        DrawPolygon(polygon, startDateTime, endDateTime);
                        break;

                    case GeoJSON.GeoJSONGeometryType.MultiPolygon:
                        List<List<List<GeoJSONPoint>>> multiPolygons = geoJSON.GetMultiPolygon();
                        for (int i = 0; i < multiPolygons.Count; i++)
                        {
                            var multiPolygon = multiPolygons[i];
                            DrawPolygon(multiPolygon, startDateTime, endDateTime);
                        }
                        break;
                }

                if((feature % featuresBeforeDraw) == 0)
                    yield return new WaitForEndOfFrame();
            }

            Debug.Log(types.Count + " found");
            foreach (var typeWork in types)
            {
                Debug.Log(typeWork.Value);
            }
        }
        else
        {
            print($"Could not retrieve GeoJSON from {url}");
        }
    }

    private void PolygonDrawn(GameObject newPolygon, DateTime startDateTime, DateTime endDateTime)
    {
        //Add event to gameobject listening to date changes from timeline
        var changeOpacityByDate = newPolygon.AddComponent<ChangeOpacityByDate>();
        changeOpacityByDate.SetOpacityRange(defaultOpacity, beforeTimeOpacity, afterTimeOpacity);
        changeOpacityByDate.ApplyBaseColor(colorPalette.colors[colorIndex].color);
        changeOpacityByDate.AppearDateTime = startDateTime;
        changeOpacityByDate.DisappearDateTime = endDateTime;
#if UNITY_EDITOR
        newPolygon.name = objectName;
        Debug.Log(newPolygon.name, newPolygon);
#endif
        onCurrentDateChange.started.AddListener(changeOpacityByDate.TimeChanged);
    }

    private void DrawPolygon(List<List<GeoJSONPoint>> polygon, DateTime startDateTime, DateTime endDateTime)
    {
        List<IList<Vector3>> unityPolygon = new List<IList<Vector3>>();

        //Grouped polys
        for (int i = 0; i < polygon.Count; i++)
        {
            var contour = polygon[i];

            IList<Vector3> polyList = new List<Vector3>();

            if (coordinateType == CoordinateType.RD)
            {
                for (int j = 0; j < contour.Count; j++)
                {
                    polyList.Add(CoordConvert.RDtoUnity(new Vector2RD(contour[j].x, contour[j].y)));
                }
            }
            else
            {
                for (int j = 0; j < contour.Count; j++)
                {
                    polyList.Add(CoordConvert.WGS84toUnity(contour[j].x, contour[j].y));
                }
            }
            unityPolygon.Add(polyList);
        }

        GameObject newPolygonGameObject = polyonVisualiser.CreateAndReturnPolygon(unityPolygon);
        if(newPolygonGameObject)
            PolygonDrawn(newPolygonGameObject, startDateTime, endDateTime);
    }
}
