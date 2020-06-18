using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Globalization;
using ConvertCoordinates;
using System;

namespace Amsterdam3D.Interface.Search
{
    public class SearchField : MonoBehaviour
    {
        [SerializeField]
        private int charactersNeededBeforeSearch = 2;

        [SerializeField]
        private InputField searchInputField;

        [SerializeField]
        private SearchResultsList searchResultsList;

        public Button[] ResultButtons = new Button[5];
        private string[] ResultIDs = new string[5];

        private const string REPLACEMENT_STRING = "{SEARCHTERM}";
        private const string locationSuggestionUrl = "https://geodata.nationaalgeoregister.nl/locatieserver/v3/suggest?q={SEARCHTERM}%20and%20Amsterdam%20&rows=5";

        private SearchData searchData;

        public void GetSuggestions(string textInput)
        {
            StopAllCoroutines();
            if (textInput.Length > charactersNeededBeforeSearch)
            {
                StartCoroutine(FindSearchSuggestions(textInput));
            }
        }

        IEnumerator FindSearchSuggestions(string searchTerm)
        {
            string urlEncodedSearchTerm = UnityWebRequest.EscapeURL(searchTerm);
            string url = locationSuggestionUrl.Replace(REPLACEMENT_STRING, urlEncodedSearchTerm);

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                {
                    Debug.Log("Error: " + webRequest.error);
                }
                else
                {
                    string jsonStringResult = webRequest.downloadHandler.text;
                    searchData = JsonUtility.FromJson<SearchData>(jsonStringResult);

                    searchResultsList.DrawResults(searchData.response.docs, searchTerm);
                }
            }
        }
    }
}