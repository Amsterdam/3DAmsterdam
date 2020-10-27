using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;

namespace Assets.Amsterdam3D.Scripts
{
    public class HideBagID : MonoBehaviour
    {

        // Use this for initialization

        [SerializeField]
        BoxSelect boxSelect;

        [SerializeField]
        LayerSystem.Layer buildingLayer;
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnBoxSelect() 
        {
            StartCoroutine(GetAllBagIDsInRange(boxSelect.GetCurrentSelection(), buildingLayer.Hide));
        }



        IEnumerator GetAllBagIDsInRange(Bounds bounds, System.Action<List<string>> callback)
        {

            var wgsMin = ConvertCoordinates.CoordConvert.UnitytoRD(bounds.min);
            var wgsMax = ConvertCoordinates.CoordConvert.UnitytoRD(bounds.max);

            List<string> ids = new List<string>();
            string url = "https://map.data.amsterdam.nl/maps/bag?REQUEST=GetFeature&SERVICE=wfs&version=2.0.0&typeName=bag:pand&propertyName=bag:id&outputFormat=csv&bbox=";
            // construct url string
            url += wgsMin.x + "," + wgsMin.y + "," + wgsMax.x + "," + wgsMax.y;

            var h = UnityWebRequest.Get(url);

            yield return h.SendWebRequest();

            if (h.isDone && !h.isHttpError)
            {

                string dataString = h.downloadHandler.text;
                Debug.Log(dataString);

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

        // source: 
        private static string GetNumbers(string input)
        {
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }
    }
}