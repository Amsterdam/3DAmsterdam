using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.Search
{
    public class SearchResultMarker : WorldPointFollower
    {
        public static SearchResultMarker Instance;

        [SerializeField]
        private Text text;

        private void Start()
        {
            Instance = this;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Show the marker at this world location, and set the text
        /// </summary>
        /// <param name="locationInWorld">Location in the world to follow</param>
        /// <param name="searchResultText">The new body text for the marker</param>
        public void Show(Vector3 locationInWorld, string searchResultText)
        {
            gameObject.SetActive(true);
            WorldPosition = locationInWorld + Vector3.up * Config.activeConfiguration.zeroGroundLevelY * 2.0f;

            //Set text from search result (without formatting)
            text.text = Regex.Replace(searchResultText, "<.*?>", string.Empty);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}