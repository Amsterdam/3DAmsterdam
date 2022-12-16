using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Globalization;
using Netherlands3D.Core;
using System;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;
using TMPro;

namespace Netherlands3D.Interface.Search
{
	public class SearchField : MonoBehaviour
	{
		[SerializeField]
		private GameObject clearButton;

		[SerializeField]
		private int charactersNeededBeforeSearch = 2;

		[SerializeField]
		private TMP_InputField searchInputField;
		/// <summary>
		/// Searchinput makes sure any rich markup (bold, italic etc.) is stripped from the input, when we set the value.
		/// </summary>
		public string SearchInput { get => searchInputField.text; set => searchInputField.text = Regex.Replace(value, "<.*?>", string.Empty); }

		private SearchResults searchResultsList;

		private const string REPLACEMENT_STRING = "{SEARCHTERM}";

		private SearchData searchData;

		public bool IsFocused => searchInputField.isFocused;

		private void Start()
		{
			searchResultsList = GetComponent<SearchResults>();
			searchResultsList.ShowResultsList(false);
		}

		public void ClearInput()
		{
			searchInputField.text = "";
			GetSuggestions();

			SearchResultMarker.Instance.Hide();
		}

		IEnumerator CatchEnter(){
			while (IsFocused && !(Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Break)))
			{
				yield return null;
			}
			searchResultsList.ApplySearch();
		}

		public void GetSuggestions(string textInput = "")
		{
			searchResultsList.HideWarning();

			var inputNotEmpty = (textInput != "");

			clearButton.SetActive(inputNotEmpty);
			searchResultsList.ShowResultsList(inputNotEmpty);

			StopAllCoroutines();

			if (inputNotEmpty)
			{
				StartCoroutine(CatchEnter());
				if (textInput.Length > charactersNeededBeforeSearch)
				{
					StartCoroutine(FindSearchSuggestions(textInput));
				}
			}
			else{
				searchResultsList.ClearOldResults();
			}
		}

		public void EndEdit()
		{
			//No need to catch
		}

		IEnumerator FindSearchSuggestions(string searchTerm)
		{
			string urlEncodedSearchTerm = UnityWebRequest.EscapeURL(searchTerm);
            string url = Config.activeConfiguration.LocationSuggestionUrl.Replace(REPLACEMENT_STRING, urlEncodedSearchTerm);

			using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
			{
				// Request and wait for the desired page.
				yield return webRequest.SendWebRequest();

				if (webRequest.result != UnityWebRequest.Result.Success)
				{
					WarningDialogs.Instance.ShowNewDialog("Sorry, door een probleem met de server is zoeken tijdelijk niet mogelijk.");
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