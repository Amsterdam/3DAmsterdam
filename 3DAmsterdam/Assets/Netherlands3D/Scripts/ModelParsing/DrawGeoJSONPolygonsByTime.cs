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
    [SerializeField]
    private string description = "";
    [SerializeField]
    private string urlWithGeoJsonData = "https://";

    [SerializeField]
    private string objectNameProperty = "Projectnaam";
    [SerializeField]
    private string startYearProperty = "Start_bouw";
    private float startBuildYear = 0;

    [SerializeField]
    private string colorIntValueProperty = "Fase_ID";

    private string objectName = "";
    private int colorIndex = 0;
    [SerializeField]
    private int indexOffset = -2;

    [SerializeField]
    private float defaultOpacity = 1.0f;

    [SerializeField]
    private float beforeTimeOpacity = 0.1f;
    [SerializeField]
    private float afterTimeOpacity = 0.1f;

    [Header("Listen to")]
    [SerializeField]
    private DateTimeEvent onCurrentDateChange;

    [Header("Invoke events")]
    [SerializeField]
    ColorPaletteEvent openLegendWithColorPalette;
    [SerializeField]
    private ColorPalette colorPalette;
    private Coroutine runningDataload;

    private PolygonVisualiser polyonVisualiser;

    private float dataMinYear = float.MaxValue;
    private float dataMaxYear = float.MinValue;



    private void Awake()
    {
        polyonVisualiser = GetComponent<PolygonVisualiser>();
    }

    private void OnDisable()
    {
        Clear();
    }

    private void Clear()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    private void OnEnable()
    {
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
                var type = geoJSON.GetGeometryType();
                startBuildYear = geoJSON.GetPropertyFloatValue(startYearProperty);

                //Track range of data in year
                if (dataMinYear > startBuildYear) dataMinYear = startBuildYear;
                if (dataMaxYear < startBuildYear) dataMaxYear = startBuildYear;

                colorIndex = (int)geoJSON.GetPropertyFloatValue(colorIntValueProperty) + indexOffset;
                objectName = geoJSON.GetPropertyStringValue(objectNameProperty);

                switch (type)
                {
                    case GeoJSON.GeoJSONGeometryType.Polygon:
                        List<List<GeoJSONPoint>> polygon = geoJSON.GetPolygon();
                        DrawPolygon(polygon);
                        yield return new WaitForEndOfFrame();
                        break;

                    case GeoJSON.GeoJSONGeometryType.MultiPolygon:
                        List<List<List<GeoJSONPoint>>> multiPolygons = geoJSON.GetMultiPolygon();
                        for (int i = 0; i < multiPolygons.Count; i++)
                        {
                            var multiPolygon = multiPolygons[i];
                            DrawPolygon(multiPolygon);
                        }
                        yield return new WaitForEndOfFrame();
                        break;
                }
            }
        }
        else
        {
            print($"Could not retrieve GeoJSON from {url}");
        }
    }

    private void PolygonDrawn(GameObject newPolygon)
    {
        //Add event to gameobject listening to date changes from timeline
        var changeOpacityByDate = newPolygon.AddComponent<ChangeOpacityByDate>();
        changeOpacityByDate.SetOpacityRange(defaultOpacity, beforeTimeOpacity, afterTimeOpacity);
        changeOpacityByDate.ApplyBaseColor(colorPalette.colors[colorIndex].color);
        changeOpacityByDate.ObjectDateTime = new DateTime(Mathf.RoundToInt(startBuildYear), 1, 1);
#if UNITY_EDITOR
        newPolygon.name = objectName;
        Debug.Log(newPolygon.name, newPolygon);
#endif
        onCurrentDateChange.started.AddListener(changeOpacityByDate.TimeChanged);
    }

    private void DrawPolygon(List<List<GeoJSONPoint>> polygon)
    {
        List<IList<Vector3>> unityPolygon = new List<IList<Vector3>>();

        //Grouped polys
        for (int i = 0; i < polygon.Count; i++)
        {
            var contour = polygon[i];

            IList<Vector3> polyList = new List<Vector3>();
            for (int j = 0; j < contour.Count; j++)
            {
                polyList.Add(CoordConvert.WGS84toUnity(contour[j].x, contour[j].y));
            }
            unityPolygon.Add(polyList);
        }

        GameObject newPolygonGameObject = polyonVisualiser.CreateAndReturnPolygon(unityPolygon);
        if(newPolygonGameObject)
            PolygonDrawn(newPolygonGameObject);
    }
}
