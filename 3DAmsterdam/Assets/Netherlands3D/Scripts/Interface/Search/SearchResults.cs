using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.Interface.Search
{
	public class SearchResults : MonoBehaviour
	{
		[SerializeField]
		private RectTransform searchResultsContainer;

		[SerializeField]
		private GameObject noResultsWarning;

		[SerializeField]
		private SearchResult searchResultPrefab;

		private SearchResult selectedResultItem;

		[SerializeField]
		private int maxSearchResults = 8;

		private SearchField searchField;

		/// <summary>
		/// Draws all the search result suggestions in the list.
		/// </summary>
		/// <param name="results">The serialized results list from the search API</param>
		/// <param name="usedInputField">The search field we used to find these results</param>
		public void DrawResults(SearchData.Response.Docs[] results, SearchField usedInputField)
		{
			searchField = usedInputField;

			ClearOldResults();

			int itemsFound = results.Length;
			if (itemsFound == 0)
			{
				NoSearchResults();
				return;
			}
			int resultsToShow = Mathf.Min(itemsFound, maxSearchResults);
			for (int i = 0; i < resultsToShow; i++)
			{
				CreateNewResult(results[i], searchField.SearchInput, (i == 0));
			}
		}

		/// <summary>
		/// Draw the list with all the search results/suggestions
		/// </summary>
		/// <param name="show">Showing enables the list GameObject</param>
		public void ShowResultsList(bool show)
		{
			searchResultsContainer.gameObject.SetActive(show);
		}

		/// <summary>
		/// Fill the searchtext with the contents of the suggestion
		/// </summary>
		/// <param name="inputText">The new input text value</param>
		public void AutocompleteSearchText(string inputText)
		{
			searchField.SearchInput = inputText;
		}

		/// <summary>
		/// Destroys all the results in the list
		/// </summary>
		public void ClearOldResults()
		{
			foreach (Transform child in searchResultsContainer)
				Destroy(child.gameObject);
		}

		/// <summary>
		/// Searches the active suggestion/search result, or shows a warning when its empty
		/// </summary>
		public void ApplySearch()
		{
			//Check if the current EventSystem selection is one of the search results.
			//Otherwise, leave it as it is.
			var currentFocusObject = EventSystem.current.currentSelectedGameObject;
			if (currentFocusObject)
			{
				var currentResult = currentFocusObject.GetComponent<SearchResult>();
				if (currentResult) selectedResultItem = currentResult;
			}

			if (selectedResultItem)
			{
				ShowResultsList(true); //The results contain the coroutines, make sure they are active
				selectedResultItem.ClickedResult();
				selectedResultItem.GetComponent<Button>().Select();
			}
			else
			{
				//Show a warning that now results have been found.
				NoResultsWarning();
			}
		}

		/// <summary>
		/// Draws a warning for 5 seconds showing no search results were found
		/// </summary>
		private void NoResultsWarning()
		{
			noResultsWarning.SetActive(true);
			StopAllCoroutines();
			StartCoroutine(HideWarningAfterTimer());
		}

		IEnumerator HideWarningAfterTimer()
		{
			yield return new WaitForSeconds(5.0f);
			HideWarning();
		}

		public void HideWarning()
		{
			noResultsWarning.SetActive(false);
		}

		private void NoSearchResults()
		{
			Debug.Log("No search results");
		}

		/// <summary>
		/// Spawns a new search item in the suggestions/results list, and sets focus on the selected one.
		/// </summary>
		/// <param name="result">The serialized results list from the search API</param>
		/// <param name="searchTerm">The text used to get there results</param>
		/// <param name="select">Focus on this result with the EventSystem</param>
		public void CreateNewResult(SearchData.Response.Docs result, string searchTerm, bool select)
		{
			var resultName = result.weergavenaam.Replace("\"", "");
			var resultID = result.id;

			int searchStart = resultName.ToLower().IndexOf(searchTerm.ToLower());
			if (searchStart < 0)
				return;

			string boldMarkupPart = "<b>" + resultName.Substring(searchStart, searchTerm.Length) + "</b>";
			string restOfName = resultName.Substring(searchStart + searchTerm.Length);

			SearchResult newSearchResult = Instantiate(searchResultPrefab, searchResultsContainer).GetComponent<SearchResult>();
			newSearchResult.ResultText = boldMarkupPart + restOfName;
			newSearchResult.ID = resultID;
			newSearchResult.ParentList = this;

			if (select)
			{
				selectedResultItem = newSearchResult;
			}
		}
	}
}
