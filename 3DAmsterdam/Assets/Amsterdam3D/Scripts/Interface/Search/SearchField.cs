using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Globalization;
using ConvertCoordinates;
using System;
using UnityEngine.EventSystems;
using BruTile.Wms;
using System.Text.RegularExpressions;

namespace Amsterdam3D.Interface.Search
{
	public class SearchField : MonoBehaviour, ISelectHandler
	{
		[SerializeField]
		private GameObject clearButton;

		[SerializeField]
		private int charactersNeededBeforeSearch = 2;

		[SerializeField]
		private InputField searchInputField;
		/// <summary>
		/// Searchinput makes sure any rich markup (bold, italic etc.) is stripped from the input, when we set the value.
		/// </summary>
		public string SearchInput { get => searchInputField.text; set => searchInputField.text = Regex.Replace(value, "<.*?>", string.Empty); }

		private SearchResults searchResultsList;

		private const string REPLACEMENT_STRING = "{SEARCHTERM}";
		private const string locationSuggestionUrl = "https://geodata.nationaalgeoregister.nl/locatieserver/v3/suggest?q={SEARCHTERM}%20and%20Amsterdam%20&rows=5";

		private SearchData searchData;

		private void Start()
		{
			searchResultsList = GetComponent<SearchResults>();
			searchResultsList.ShowResultsList(false);
		}

		public void ClearInput()
		{
			searchInputField.text = "";
			GetSuggestions();
		}

		public void GetSuggestions(string textInput = "")
		{
			var inputNotEmpty = (textInput != "");

			clearButton.SetActive(inputNotEmpty);
			searchResultsList.ShowResultsList(inputNotEmpty);

			StopAllCoroutines();
			if (textInput.Length > charactersNeededBeforeSearch)
			{
				StartCoroutine(FindSearchSuggestions(textInput));
			}
		}

		public void EndEdit()
		{
			//searchResultsList.gameObject.SetActive(false);
		}
		public void OnSelect(BaseEventData data)
		{
			print("Selected search input field");
			searchResultsList.ShowResultsList(true);
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

					searchResultsList.DrawResults(searchData.response.docs, this);
				}
			}
		}
	}
}