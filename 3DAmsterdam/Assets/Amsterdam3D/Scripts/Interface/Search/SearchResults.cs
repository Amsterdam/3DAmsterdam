using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface.Search
{
	public class SearchResults : MonoBehaviour
	{
		[SerializeField]
		private RectTransform searchResultsContainer;

		[SerializeField]
		private GameObject noResultsWarning;

		[SerializeField]
		private SearchResult searchResultPrefab;

		[SerializeField]
		private int maxSearchResults = 8;

		private SearchResult firstResultItem;
		private SearchField searchField;

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

		private void Update()
		{
			if(Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
			{
				SelectFirstItem();
			}
		}

		public void ShowResultsList(bool show)
		{
			searchResultsContainer.gameObject.SetActive(show);
		}

		public void AutocompleteSearchText(string inputText)
		{
			searchField.SearchInput = inputText;
		}

		public void ClearOldResults()
		{
			firstResultItem = null;
			foreach (Transform child in searchResultsContainer)
				Destroy(child.gameObject);
		}

		public void SelectFirstItem()
		{
			if (firstResultItem)
			{
				ShowResultsList(true); //The results contain the coroutines, make sure they are active
				firstResultItem.ClickedResult();
				firstResultItem.GetComponent<Button>().Select();
			}
			else
			{
				//Show a warning that now results have been found.
				NoResultsWarning();
			}
		}

		private void NoResultsWarning()
		{
			noResultsWarning.SetActive(true);
			StopAllCoroutines();
			StartCoroutine(HideWarningAfterTimer());
		}

		IEnumerator HideWarningAfterTimer()
		{
			yield return new WaitForSeconds(5.0f);
			noResultsWarning.SetActive(false);
		}


		private void NoSearchResults()
		{
			Debug.Log("No search results");
		}

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

			if (select) firstResultItem = newSearchResult;
		}
	}
}
