using Netherlands3D.CameraMotion;
using Netherlands3D.JavascriptConnection;
using ConvertCoordinates;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Netherlands3D.Interface.Search
{
    public class SearchResult : ChangePointerStyleHandler
    {
        private const string REPLACEMENT_STRING = "{ID}";
        private const string lookupUrl = "https://geodata.nationaalgeoregister.nl/locatieserver/v3/lookup?id={ID}";

        private SearchResults parentList;
        public SearchResults ParentList { get => parentList; set => parentList = value; }

        [SerializeField]
        private Text textField;

        private string resultText;
        private string id;

        private LookupData lookupData;

        private SelectByID selectByID;

        public string ResultText { 
            get => resultText;
            set
            {
                resultText = value;
                textField.text = value;
            }
        }
        public string ID
        {
            get => id;
            set
            {
                id = value;
                gameObject.name = id;
            }
        }

		private void Start()
		{
            selectByID = FindObjectOfType<SelectByID>();
        }

		public void ClickedResult(){
            ParentList.AutocompleteSearchText(ResultText);
            StartCoroutine(FindLocationByIDLookup());
        }

        IEnumerator FindLocationByIDLookup()
        {
            string uri = lookupUrl.Replace(REPLACEMENT_STRING, id);
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.isNetworkError)
                {
                    Debug.Log("Error: " + webRequest.error);
                }
                else
                {
                    string jsonStringResult = webRequest.downloadHandler.text;
                    lookupData = JsonUtility.FromJson<LookupData>(jsonStringResult);
                    int itemsFound = lookupData.response.numFound;
                    if (itemsFound == 0)
                    {
                        IDNotFound();
                        yield break;
                    }

                    string locationData = lookupData.response.docs[0].centroide_ll;
                    Vector3 targetLocation = ExtractUnityLocation(ref locationData);

                    ParentList.ShowResultsList(false);
                    
                    CameraModeChanger.Instance.CurrentCameraControls.MoveAndFocusOnLocation(targetLocation, new Quaternion());
                    SearchResultMarker.Instance.Show(targetLocation, textField.text);
                }
            }
        }

        private void IDNotFound(){
            Debug.Log("ID not found. This might be a problem at the API side.");
        }

        private static Vector3 ExtractUnityLocation(ref string locationData)
        {
            locationData = locationData.Replace("POINT(", "").Replace(")", "").Replace("\"", "");
            string[] lonLat = locationData.Split(' ');

            double.TryParse(lonLat[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double lon);
            double.TryParse(lonLat[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double lat);

            Vector3 unityLocation = CoordConvert.WGS84toUnity(lon, lat);
            return unityLocation;
        }
    }
}