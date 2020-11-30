using Amsterdam3D.CameraMotion;
using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Amsterdam3D.Sewerage
{
    public partial class SewerageGenerator : MonoBehaviour
    {
        private const string sewerPipesWfsUrl = "https://api.data.amsterdam.nl/v1/wfs/rioolnetwerk/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&outputFormat=geojson&typeName=rioolleidingen&bbox=";
        private const string sewerManholesWfsUrl = "https://api.data.amsterdam.nl/v1/wfs/rioolnetwerk/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&outputFormat=geojson&typeName=rioolknopen&bbox=";

        [SerializeField]
        private bool drawEditorGizmos = false;

        private SewerLines sewerLines;
        private SewerManholes sewerManholes;

        [SerializeField]
        private SewerLineSpawner sewerPipeSpawner;
        [SerializeField]
        private SewerManholeSpawner sewerManholeSpawner;

        private const int maxSpawnsPerFrame = 100;

        private Vector3RD boundingBoxMinimum  = default;
        private Vector3RD boundingBoxMaximum = default;

        private double boundingBoxMargin = 2.0f;

        private void Start()
		{
			//For testing purposes, just load a set area.
			//We want this to come from the tile/layer system
			GetBoundingBoxCameraIsIn();
		}

		private void GetBoundingBoxCameraIsIn()
		{
            Vector3RD cameraRD = CoordConvert.UnitytoRD(CameraModeChanger.Instance.ActiveCamera.transform.position);
            cameraRD.x = Mathf.Round((float)cameraRD.x);
            cameraRD.y = Mathf.Round((float)cameraRD.y);

            //Outside our bounds? Load a new area (always load a bigger area)
            if(cameraRD.x < boundingBoxMinimum.x || cameraRD.y > boundingBoxMinimum.y || cameraRD.x > boundingBoxMaximum.x || cameraRD.y < boundingBoxMaximum.y)
            {
                //TODO. clean up and load up new area.

                Generate(boundingBoxMinimum, boundingBoxMaximum);
            }

            /*boundingBoxMinimum = new Vector3RD(122000, 484000, 0);
			boundingBoxMaximum = new Vector3RD(123000, 483000, 0);*/
		}

		/// <summary>
		/// Starts genering the sewage network based on the geometry data and points in a WFS service
		/// </summary>
		/// <param name="boxMinimum">The RD coordinates min point of a bounding box area</param>
		/// <param name="boxMaximum">The RD coordinates maximum point of a bounding box area</param>
		public void Generate(Vector3RD boxMinimum = default, Vector3RD boxMaximum  = default)
        {
            boundingBoxMinimum = boxMinimum;
            boundingBoxMaximum = boxMaximum;

            StartCoroutine(GetSewerLinesInBoundingBox());
        }

        private IEnumerator SpawnLineObjects()
        {
            SewerLines.Feature sewerLineFeature;
			for (int i = 0; i < sewerLines.features.Length; i++)
			{
                if ((i % maxSpawnsPerFrame) == 0) yield return new WaitForEndOfFrame();

                sewerLineFeature = sewerLines.features[i];
                sewerPipeSpawner.CreateSewerLine(
                    sewerLineFeature.geometry.unity_coordinates[0],
                    sewerLineFeature.geometry.unity_coordinates[1],
                    float.Parse(sewerLineFeature.properties.diameter)
                );
            }

            //Lines are done spawing. Start loading and spawing the manholes.
            StartCoroutine(GetSewerManholesInBoundingBox());

            yield return null;
		}
        private IEnumerator SpawnManholeObjects()
        {
            SewerManholes.Feature sewerManholeFeature;
            for (int i = 0; i < sewerManholes.features.Length; i++)
            {
                //Speedy way to check if the string is not a 'Knikpunt' but a 'Regenwaterrioolput'
                if (sewerManholes.features[i].properties.objectsoort.Length != 18) continue;

                if ((i % maxSpawnsPerFrame) == 0) yield return new WaitForEndOfFrame();

                sewerManholeFeature = sewerManholes.features[i];
                sewerManholeSpawner.CreateManhole(
                    CoordConvert.RDtoUnity(new Vector3RD(
                        sewerManholeFeature.geometry.coordinates[0],
                        sewerManholeFeature.geometry.coordinates[1],
                        float.Parse(sewerManholeFeature.properties.putdekselhoogte)
                        )
                    )
                );
            }
            yield return null;
        }

        IEnumerator GetSewerManholesInBoundingBox()
        {
            string escapedUrl = sewerManholesWfsUrl;
            escapedUrl += UnityWebRequest.EscapeURL(boundingBoxMinimum.x.ToString(CultureInfo.InvariantCulture) + "," + boundingBoxMinimum.y.ToString(CultureInfo.InvariantCulture) + "," + boundingBoxMaximum.x.ToString(CultureInfo.InvariantCulture) + "," + boundingBoxMaximum.y.ToString(CultureInfo.InvariantCulture));
            var sewerageRequest = UnityWebRequest.Get(escapedUrl);

            Debug.Log(escapedUrl);

            yield return sewerageRequest.SendWebRequest();
            if (!sewerageRequest.isNetworkError && !sewerageRequest.isHttpError)
            {
                string dataString = sewerageRequest.downloadHandler.text;
                Debug.Log(dataString);
                sewerManholes = JsonUtility.FromJson<SewerManholes>(dataString);

                yield return new WaitForEndOfFrame();
                StartCoroutine(SpawnManholeObjects());
            }
            yield return null;
        }

        IEnumerator GetSewerLinesInBoundingBox()
        {
            string escapedUrl = sewerPipesWfsUrl;
            escapedUrl += UnityWebRequest.EscapeURL(boundingBoxMinimum.x.ToString(CultureInfo.InvariantCulture) + "," + boundingBoxMinimum.y.ToString(CultureInfo.InvariantCulture) + "," + boundingBoxMaximum.x.ToString(CultureInfo.InvariantCulture) + "," + boundingBoxMaximum.y.ToString(CultureInfo.InvariantCulture));
            var sewerageRequest = UnityWebRequest.Get(escapedUrl);

            yield return sewerageRequest.SendWebRequest();
            if (!sewerageRequest.isNetworkError && !sewerageRequest.isHttpError)
            {
                //Replace multidimensional arrays with strings. JsonUtility doesnt support it (yet)   
                string dataString = sewerageRequest.downloadHandler.text.Replace("[[", "\"").Replace("]]", "\"");
                sewerLines = JsonUtility.FromJson<SewerLines>(dataString);
                foreach (var feature in sewerLines.features)
                {
                    Vector3[] pointCoordinate = SplitToCoordinatesArray(feature.geometry.coordinates, feature.properties.bob_beginpunt, feature.properties.bob_eindpunt);
                    feature.geometry.unity_coordinates = pointCoordinate;
                }

                yield return new WaitForEndOfFrame();
                StartCoroutine(SpawnLineObjects());
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
                        double.Parse(vector2String[0],CultureInfo.InvariantCulture),
                        double.Parse(vector2String[1],CultureInfo.InvariantCulture),
                        (i == 0) ? double.Parse(startHeight, CultureInfo.InvariantCulture) : double.Parse(endHeight, CultureInfo.InvariantCulture)
                );

                Vector3 unityCoordinate = CoordConvert.RDtoUnity(newRDVector3);
                newVector2Array.Add(unityCoordinate);

            }

            return newVector2Array.ToArray();
        }

        
        private void OnDrawGizmos()
        {
            if (!drawEditorGizmos) return;    

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
}