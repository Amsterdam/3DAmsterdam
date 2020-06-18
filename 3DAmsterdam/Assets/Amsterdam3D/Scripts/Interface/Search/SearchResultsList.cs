using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface.Search
{
	public class SearchResultsList : MonoBehaviour
	{
		[SerializeField]
		private SearchResult searchResultPrefab;

		[SerializeField]
		private int maxSearchResults = 8;

		private SearchResult firstResultItem;

		public void DrawResults(SearchData.Response.Docs[] results, string searchTerm)
		{
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
				CreateNewResult(results[i], searchTerm, (i == 0));
			}
		}

		public void ClearOldResults()
		{
			firstResultItem = null;
			foreach (Transform child in transform)
				Destroy(child.gameObject);
		}

		public void SelectFirstItem()
		{
			if (firstResultItem)
			{
				firstResultItem.ClickedResult();
				firstResultItem.GetComponent<Button>().Select();
			}
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

			SearchResult newSearchResult = Instantiate(searchResultPrefab, transform).GetComponent<SearchResult>();
			newSearchResult.ResultText = boldMarkupPart + restOfName;
			newSearchResult.ID = resultID;

			if (select) firstResultItem = newSearchResult;
		}
	}
}
