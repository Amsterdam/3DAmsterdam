using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public partial class SewerageGenerator : MonoBehaviour
{
    private const string sewerPipesWfsUrl = "https://api.data.amsterdam.nl/v1/wfs/rioolnetwerk/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&outputFormat=geojson&typeName=rioolleidingen&bbox=";
    private const string sewerManholesWfsUrl = "https://api.data.amsterdam.nl/v1/wfs/rioolnetwerk/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&outputFormat=geojson&typeName=rioolknopen&bbox=";

    [SerializeField]
    private SewerLines sewerLines;

    [SerializeField]
    private SewerManholes sewerManholes;

    [SerializeField]
    private UnityEvent doneLoadingSewerLines;
    [SerializeField]
    private UnityEvent doneLoadingSewerManholes;

    private void Start()
	{
        //For testing purposes, just load a set area.
        //We want this to come from the tile/layer system
        Generate(new Vector3RD(123000,483000, 0), new Vector3RD(124000,484000, 0));
    }

    /// <summary>
    /// Starts genering the sewage network based on the geometry data and points in a WFS service
    /// </summary>
    /// <param name="rdMinimum">The RD coordinates min point of a bounding box area</param>
    /// <param name="rdMaximum">The RD coordinates maximum point of a bounding box area</param>
	public void Generate(Vector3RD rdMinimum, Vector3RD rdMaximum)
    {
        StartCoroutine(GetSewerPipesInBoundingBox(rdMinimum, rdMaximum));
        StartCoroutine(GetSewerManholesInBoundingBox(rdMinimum, rdMaximum));
    }

    IEnumerator GetSewerManholesInBoundingBox(Vector3RD rdMinimum, Vector3RD rdMaximum)
    {
        string escapedUrl = sewerManholesWfsUrl;
        escapedUrl += UnityWebRequest.EscapeURL(rdMinimum.x.ToString(CultureInfo.InvariantCulture) + "," + rdMinimum.y.ToString(CultureInfo.InvariantCulture) + "," + rdMaximum.x.ToString(CultureInfo.InvariantCulture) + "," + rdMaximum.y.ToString(CultureInfo.InvariantCulture));
        var sewerageRequest = UnityWebRequest.Get(escapedUrl);

        yield return sewerageRequest.SendWebRequest();
        if (!sewerageRequest.isNetworkError && !sewerageRequest.isHttpError)
        {
            string dataString = sewerageRequest.downloadHandler.text;
            Debug.Log(dataString);
            sewerManholes = JsonUtility.FromJson<SewerManholes>(dataString);
            doneLoadingSewerManholes.Invoke();
        }
        yield return null;
    }

    IEnumerator GetSewerPipesInBoundingBox(Vector3RD rdMinimum, Vector3RD rdMaximum)
    {
        string escapedUrl = sewerPipesWfsUrl;
        escapedUrl += UnityWebRequest.EscapeURL(rdMinimum.x.ToString(CultureInfo.InvariantCulture) + "," + rdMinimum.y.ToString(CultureInfo.InvariantCulture) + "," + rdMaximum.x.ToString(CultureInfo.InvariantCulture) + "," + rdMaximum.y.ToString(CultureInfo.InvariantCulture));
        var sewerageRequest = UnityWebRequest.Get(escapedUrl);

        yield return sewerageRequest.SendWebRequest();
        if (!sewerageRequest.isNetworkError && !sewerageRequest.isHttpError)
        {
            //Replace multidimensional arrays with strings. JsonUtility doesnt support it (yet)   
            string dataString = sewerageRequest.downloadHandler.text.Replace("[[","\"").Replace("]]","\"");
            sewerLines = JsonUtility.FromJson<SewerLines>(dataString);
            foreach (var feature in sewerLines.features)
            {
                Vector3[] pointCoordinate = SplitToCoordinatesArray(feature.geometry.coordinates, feature.properties.bob_beginpunt, feature.properties.bob_eindpunt);
                feature.geometry.unity_coordinates = pointCoordinate;
            }
            doneLoadingSewerLines.Invoke();
        }
        yield return null;
    }


    /// <summary>
    /// Splits an unsupported multidimensional array into a Vector3 array.
    /// </summary>
    /// <param name="coordinates">The string containing the multidimensional array</param>
    /// <returns>An array of unity coordinates</returns>
    private Vector3[] SplitToCoordinatesArray(string coordinates, string startHeight, string endHeight)
    {
        string[] splitArray = coordinates.Split(new string[] { "],[" }, StringSplitOptions.None);
        List<Vector3> newVector2Array = new List<Vector3>();

        //Convert string with RD coordinates into unity coordinates
		for (int i = 0; i < splitArray.Length; i++)
		{
            string[] vector2String = splitArray[i].Split(',');
            Vector3RD newRDVector3 = new Vector3RD(
                    double.Parse(vector2String[0]),
                    double.Parse(vector2String[1]),
                    (i==0) ? double.Parse(startHeight) : double.Parse(endHeight)
            );

            Vector3 unityCoordinate = CoordConvert.RDtoUnity(newRDVector3);
            newVector2Array.Add(unityCoordinate);

        }

        return newVector2Array.ToArray();
    }

	private void OnDrawGizmos()
	{
        if (sewerLines != null)
        {
            Gizmos.color = Color.green;
            foreach (var feature in sewerLines.features)
            {
                Gizmos.DrawSphere(feature.geometry.unity_coordinates[0], 3);
                Gizmos.DrawLine(feature.geometry.unity_coordinates[0], feature.geometry.unity_coordinates[1]);
                Gizmos.DrawSphere(feature.geometry.unity_coordinates[1], 3);
            }
        }

        if (sewerManholes != null)
        {
            Gizmos.color = Color.blue;
            foreach (var feature in sewerManholes.features)
            {
                Vector3 manHoleCoordinate = new Vector3(feature.geometry.coordinates[0], feature.geometry.coordinates[1]);
                Gizmos.DrawSphere(manHoleCoordinate, 3);
            }
        }
    }
}
