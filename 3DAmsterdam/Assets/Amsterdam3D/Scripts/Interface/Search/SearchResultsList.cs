using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Interface.Search
{
    public class SearchResultsList : MonoBehaviour
    {
        [SerializeField]
        private SearchResult searchResultPrefab;

        [SerializeField]
        private int maxSearchResults = 8;

        public void DrawResults(SearchData.Response.Docs[] results, string searchTerm) {
            int itemsFound = results.Length;
            if (itemsFound == 0)
            {
                NoSearchResults();
                return;
            }
            int resultsToShow = Mathf.Min(itemsFound, maxSearchResults);
            for (int i = 0; i < resultsToShow; i++)
            {
                CreateNewResult(results[i], searchTerm);
            }
        }
        private void NoSearchResults()
        {
            Debug.Log("No search results");
        }

        public void CreateNewResult(SearchData.Response.Docs result, string searchTerm)
        {
            var resultName = result.weergavenaam.Replace("\"", "");
            var resultID = result.id;

            int searchStart = resultName.ToLower().IndexOf(searchTerm.ToLower());
            string boldMarkupPart = "<b>" + resultName.Substring(searchStart, searchTerm.Length) + "</b>";
            string restOfName = resultName.Substring(searchStart + searchTerm.Length);

            SearchResult newSearchResult = Instantiate(searchResultPrefab, transform).GetComponent<SearchResult>();
        }
    }
}
