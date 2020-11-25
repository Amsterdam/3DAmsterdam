using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public partial class SewerageGeneration : MonoBehaviour
{
    private const string sewerageWfsUrl = "https://api.data.amsterdam.nl/v1/wfs/rioolnetwerk/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&outputFormat=geojson&typeName=rioolleidingen&bbox=";
    
    [SerializeField]
    public SeweragePipes sewerage;

	private void Start()
	{
        //For testing purposes, just load a set area.
        //We want this to come from the tile/layer system
        Generate(new Vector3RD(123000,483000, 0), new Vector3RD(124000,484000, 0));
    }

	public void Generate(Vector3RD rdMin, Vector3RD rdMax)
    {
        StartCoroutine(GetSeweragePipesInBoundingBox(rdMin, rdMax));
    }

    IEnumerator GetSeweragePipesInBoundingBox(Vector3RD rdMin, Vector3RD rdMax)
    {
        List<string> ids = new List<string>();
        string escapedUrl = sewerageWfsUrl;
        escapedUrl += UnityWebRequest.EscapeURL(rdMin.x + "," + rdMin.y + "," + rdMax.x + "," + rdMax.y);
        var sewerageRequest = UnityWebRequest.Get(escapedUrl);
        print(escapedUrl);

        yield return sewerageRequest.SendWebRequest();
        if (sewerageRequest.isNetworkError || sewerageRequest.isHttpError)
        {
            WarningDialogs.Instance.ShowNewDialog("Sorry, door een probleem met de BAG id server is een selectie maken tijdelijk niet mogelijk.");
        }
        else
        {
            //Replace multidimensional arrays with strings. JsonUtility doesnt support it (yet)   
            string dataString = sewerageRequest.downloadHandler.text.Replace("[[","\"").Replace("]]","\"");
            sewerage = JsonUtility.FromJson<SeweragePipes>(dataString);
            foreach (var feature in sewerage.features)
            {
                feature.geometry.unity_coordinates = SplitToCoordinatesArray(feature.geometry.coordinates);
            }
        }
        yield return null;
    }

    private Vector3[] SplitToCoordinatesArray(string coordinates)
    {
        string[] splitArray = coordinates.Split(new string[] { "],[" }, StringSplitOptions.None);
        List<Vector3> newVector2Array = new List<Vector3>();

        foreach(string splitString in splitArray)
        {
            string[] vector2String = splitString.Split(',');
            Vector2 newVector2 = new Vector2(
                    float.Parse(vector2String[0]),
                    float.Parse(vector2String[1])
            );

            Vector3 unityCoordinate = CoordConvert.RDtoUnity(newVector2);
            newVector2Array.Add(unityCoordinate);
        }

        return newVector2Array.ToArray();
    }

	private void OnDrawGizmos()
	{
        Gizmos.color = Color.green;
        foreach(var feature in sewerage.features)
        {
            Gizmos.DrawSphere(feature.geometry.unity_coordinates[0], 3);
            Gizmos.DrawLine(feature.geometry.unity_coordinates[0], feature.geometry.unity_coordinates[1]);
            Gizmos.DrawSphere(feature.geometry.unity_coordinates[1], 3);
        }
    }
}
