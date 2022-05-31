using Netherlands3D.Core;
using Netherlands3D.Events;
using Netherlands3D.Timeline;
using Netherlands3D.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DrawGeoJSONPolygonsByTime : MonoBehaviour
{
    [SerializeField]
    private string urlWithGeoJsonData = "https://";

    [Header("Invoke")]
    [SerializeField]
    private Vector3ListsEvent drawPolygonEvent;

    [Header("Listen to")]
    [SerializeField]
    GameObjectEvent createdPolygonGameObject;

    [SerializeField]
    private string startYearProperty = "Start_bouw";
    private float startBuildYear = 0;

    [SerializeField]
    private TimelineUI timeline;

    [SerializeField]
    private string colorIntValueProperty = "Fase_ID";
    [SerializeField]
    private Color[] colorByIntIndex;

    private int colorIndex = 0;

    private void Awake()
    {
        if (createdPolygonGameObject) createdPolygonGameObject.started.AddListener(PolygonDrawn);
    }

    private void Start()
    {
        LoadData();
    }

    public void LoadData(string geoJsonURL = "")
    {
        var url = (geoJsonURL == "") ? urlWithGeoJsonData : geoJsonURL;
        StartCoroutine(LoadGeoJSON(url));
    }

    private IEnumerator LoadGeoJSON(string url)
    {
        var geoJsonDataRequest = UnityWebRequest.Get(url);
        yield return geoJsonDataRequest.SendWebRequest();

        if (geoJsonDataRequest.result == UnityWebRequest.Result.Success)
        {
            GeoJSON geoJSON = new GeoJSON(geoJsonDataRequest.downloadHandler.text);
            yield return null;

            //We already filtered the request, so we can draw all features
            while (geoJSON.GotoNextFeature())
            {
                startBuildYear = geoJSON.GetPropertyFloatValue(startYearProperty);
                colorIndex = (int)geoJSON.GetPropertyFloatValue(colorIntValueProperty);

                List<List<GeoJSONPoint>> polygon = geoJSON.GetPolygon();
                yield return DrawPolygon(polygon);
            }
        }
    }

    private void PolygonDrawn(GameObject newPolygon)
    {
        //add event to gameobject listening to date from timeline
        //newPolygon.AddComponent<ActiveFromTime>();
        newPolygon.name = $"{startBuildYear} phase:{colorIndex}";
        var changeOpacityByDate = newPolygon.AddComponent<ChangeOpacityByDate>();

        changeOpacityByDate.ApplyBaseColor(colorByIntIndex[colorIndex]);
        changeOpacityByDate.ObjectDateTime = new DateTime(Mathf.RoundToInt(startBuildYear), 1, 1);

        //TODO: Remove dependency to timeline by adding DateTime scriptable object events to timeline
        timeline.onCurrentDateChange.AddListener(changeOpacityByDate.TimeChanged);
    }

    private IEnumerator DrawPolygon(List<List<GeoJSONPoint>> polygon)
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

        drawPolygonEvent.started?.Invoke(unityPolygon);

        yield return null;
    }
}
