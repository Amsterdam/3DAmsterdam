using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using Amsterdam3D.SelectionTools;

namespace Assets.Amsterdam3D.Scripts
{
    public class HideBagID : MonoBehaviour
    {

        // Use this for initialization

        [SerializeField]
        SelectionToolBehaviour boxSelect;

        [SerializeField]
        LayerSystem.Layer buildingLayer;


        private Bounds selectedBounds;
        void Start()
        {
           boxSelect =  FindObjectOfType<SelectionToolBehaviour>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.H)) 
            {
                OnBoxSelect();
            }
        }

        public void OnBoxSelect() 
        {
            if (boxSelect.inSelection)
            {
                buildingLayer.LoadMeshColliders(callback => { selectedBounds = boxSelect.GetBounds(); });
                var vertices = boxSelect.GetVertices();
                StartCoroutine(GetAllBagIDsInRange(vertices[0], vertices[2], HideIDs));
                
            }
           
        }

        private void HideIDs(List<string> ids) 
        {
            buildingLayer.Hide(ids);
        }


        IEnumerator GetAllBagIDsInRange(Vector3 min, Vector3 max, System.Action<List<string>> callback)
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

                var csv = splitCSV(dataString);
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

            callback(ids);
            yield return null;
        }


        public List<string> splitCSV(string csv)
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
}