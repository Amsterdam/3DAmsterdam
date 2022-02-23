using Netherlands3D.Cameras;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Netherlands3D;
using Netherlands3D.Core;

namespace Amsterdam3D.Sewerage
{
    public partial class SewerageGenerator : MonoBehaviour
    {
        [SerializeField]
        private bool drawEditorGizmos = false;

        private SewerLines sewerLines;
        private SewerManholes sewerManholes;

        [SerializeField]
        private Material sharedMaterial;

        [SerializeField]
        private Transform networkContainer;

        [SerializeField]
        private Transform combinesMeshTileContainer;

        [SerializeField]
        private SewerLineSpawner sewerPipeSpawner;
        [SerializeField]
        private SewerManholeSpawner sewerManholeSpawner;

        private const int maxSpawnsPerFrame = 100;
        private const int maxParsesPerFrame = 500;

        private Vector3RD boundingBoxMinimum  = default;
        private Vector3RD boundingBoxMaximum = default;

        private double boundingBoxMargin = 500.0f;

        private string[] splitArray;
        private List<Vector3> newVector2Array;
        private string[] vector2String;
        private float napOffset;



        private void Start()
		{
            newVector2Array = new List<Vector3>();
        }

		private void Update()
		{
			GetBoundingBoxCameraIsIn();
		}

		private void GetBoundingBoxCameraIsIn()
		{
            var cameraRD = CoordConvert.UnitytoRD(CameraModeChanger.Instance.ActiveCamera.transform.position);
            cameraRD.x = Mathf.Round((float)cameraRD.x);
            cameraRD.y = Mathf.Round((float)cameraRD.y);

            //Outside our bounds? Load a new area (always load a bigger area)
            if(cameraRD.x < boundingBoxMinimum.x || cameraRD.y > boundingBoxMinimum.y || cameraRD.x > boundingBoxMaximum.x || cameraRD.y < boundingBoxMaximum.y)
            {
                //Make sure to stop all ongoing requests
                StopAllCoroutines();

                //Set new area based on rounded camera position with a margin
                boundingBoxMinimum.x = cameraRD.x - boundingBoxMargin;
                boundingBoxMinimum.y = cameraRD.y + boundingBoxMargin;

                boundingBoxMaximum.x = cameraRD.x + boundingBoxMargin;
                boundingBoxMaximum.y = cameraRD.y - boundingBoxMargin;

                Generate(boundingBoxMinimum, boundingBoxMaximum);
            }
		}

        private void ClearNetwork()
        {   
            //Clears the network of spawned prefabs, so we can load a new area
            foreach(Transform child in networkContainer)
            {
                Destroy(child.gameObject);
			}
            foreach (Transform child in combinesMeshTileContainer)
            {
                //Make sure our combined meshes are destroyed
                Destroy(child.GetComponentInChildren<MeshFilter>().sharedMesh);
                Destroy(child.gameObject);
            }
        }

		/// <summary>
		/// Starts genering the sewage network based on the geometry data and points in a WFS service
		/// </summary>
		/// <param name="boxMinimum">The RD coordinates min point of a bounding box area</param>
		/// <param name="boxMaximum">The RD coordinates maximum point of a bounding box area</param>
		public void Generate(Vector3RD boxMinimum = default, Vector3RD boxMaximum  = default)
        {
            napOffset = Config.activeConfiguration.zeroGroundLevelY;
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
                //Speedy way to check if the string is not a 'Knikpunt'
                if (sewerManholes.features[i].properties.objectsoort.Length == 8) continue;

                if ((i % maxSpawnsPerFrame) == 0) yield return new WaitForEndOfFrame();

                sewerManholeFeature = sewerManholes.features[i];
                sewerManholeSpawner.CreateManhole(
                    CoordConvert.WGS84toUnity(new Vector3WGS(
                        sewerManholeFeature.geometry.coordinates[0],
                        sewerManholeFeature.geometry.coordinates[1],
                        (float.Parse(sewerManholeFeature.properties.putdekselhoogte, CultureInfo.InvariantCulture) +napOffset)
                        )
                    )
                );
            }
            CombineSewerage();
            yield return null;
        }

        IEnumerator GetSewerManholesInBoundingBox()
        {
            string escapedUrl = Config.activeConfiguration.sewerManholesWfsUrl;
            escapedUrl += UnityWebRequest.EscapeURL((boundingBoxMinimum.x - boundingBoxMargin).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMinimum.y + boundingBoxMargin).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMaximum.x + boundingBoxMargin).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMaximum.y - boundingBoxMargin).ToString(CultureInfo.InvariantCulture));
            var sewerageRequest = UnityWebRequest.Get(escapedUrl);

            yield return sewerageRequest.SendWebRequest();
            if (sewerageRequest.result == UnityWebRequest.Result.Success)
            {
                string dataString = sewerageRequest.downloadHandler.text;
                sewerManholes = JsonUtility.FromJson<SewerManholes>(dataString);

                yield return new WaitForEndOfFrame();
                StartCoroutine(SpawnManholeObjects());
            }
            yield return null;
        }

        IEnumerator GetSewerLinesInBoundingBox()
        {
            string escapedUrl = Config.activeConfiguration.sewerPipesWfsUrl;
            escapedUrl += UnityWebRequest.EscapeURL((boundingBoxMinimum.x - boundingBoxMargin).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMinimum.y + boundingBoxMargin).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMaximum.x + boundingBoxMargin).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMaximum.y - boundingBoxMargin).ToString(CultureInfo.InvariantCulture));
            var sewerageRequest = UnityWebRequest.Get(escapedUrl);

            yield return sewerageRequest.SendWebRequest();
            if (sewerageRequest.result == UnityWebRequest.Result.Success)
            {
                //Replace multidimensional arrays with strings. JsonUtility doesnt support it (yet)   
                string dataString = sewerageRequest.downloadHandler.text.Replace("[[", "\"").Replace("]]", "\"");
                sewerLines = JsonUtility.FromJson<SewerLines>(dataString);

				for (int i = 0; i < sewerLines.features.Length; i++)
				{
                    //Smear out the heavy parsing over a few frames, to avoid spikes and memory issues in WebGL
                    if ((i % maxParsesPerFrame) == 0) yield return new WaitForEndOfFrame();

                    var feature = sewerLines.features[i];
                    Vector3[] pointCoordinate = SplitToCoordinatesArray(feature.geometry.coordinates, feature.properties.bob_beginpunt, feature.properties.bob_eindpunt);
                    feature.geometry.unity_coordinates = pointCoordinate;
                }

                yield return new WaitForEndOfFrame();

                StartCoroutine(SpawnLineObjects());
            }
            //We have a new network now that can start to spawn. Clear the old objects.
            ClearNetwork();

            yield return null;
        }


        /// <summary>
        /// Splits an unsupported multidimensional array into a Vector3 array.
        /// </summary>
        /// <param name="coordinates">The string containing the multidimensional array</param>
        /// <returns>An array of unity coordinates</returns>
        private Vector3[] SplitToCoordinatesArray(string coordinates, string startHeight, string endHeight)
        {
            splitArray = coordinates.Split(new string[] { "],[" }, StringSplitOptions.None);
            newVector2Array.Clear();

            //Convert string with RD coordinates into unity coordinates
            for (int i = 0; i < splitArray.Length; i++)
            {
                vector2String = splitArray[i].Split(',');
                Vector3WGS newWGSVector3 = new Vector3WGS(
                        double.Parse(vector2String[0],CultureInfo.InvariantCulture),
                        double.Parse(vector2String[1],CultureInfo.InvariantCulture),
                        (i == 0) ? double.Parse(startHeight, CultureInfo.InvariantCulture)+napOffset : double.Parse(endHeight, CultureInfo.InvariantCulture)+napOffset
                );

                Vector3 unityCoordinate = CoordConvert.WGS84toUnity(newWGSVector3);
                newVector2Array.Add(unityCoordinate);

            }
            return newVector2Array.ToArray();
        }

        public GameObject CombineSewerage()
        {
            //Determine meshes to combine
            MeshFilter[] meshFilters = networkContainer.GetComponentsInChildren<MeshFilter>(true);
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].gameObject.SetActive(false);

                i++;
            }

            GameObject[] allChildren = new GameObject[networkContainer.childCount];
            int j = 0;
            //Find all child obj and store to that array
            foreach (Transform child in networkContainer)
            {
                allChildren[j] = child.gameObject;
                j++;
            }

            //Own combined mesh
            GameObject newCombinedTile = new GameObject();
            newCombinedTile.name = "CombinedTile";
            newCombinedTile.transform.SetParent(combinesMeshTileContainer);
            newCombinedTile.AddComponent<MeshRenderer>().material = sharedMaterial;

            Mesh newMesh = new Mesh
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };			
            newMesh.CombineMeshes(combine);
            newCombinedTile.AddComponent<MeshFilter>().sharedMesh = newMesh;

            //Now destroy our large amount of network children.
            foreach (GameObject child in allChildren)
            {
                Destroy(child.gameObject);
            }
            return newCombinedTile;
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