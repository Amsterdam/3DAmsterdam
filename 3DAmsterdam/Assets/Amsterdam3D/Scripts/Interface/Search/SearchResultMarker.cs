using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public void Show(Vector3 locationInWorld, string searchResultText)
    {
        gameObject.SetActive(true);
        WorldPosition = locationInWorld + Vector3.up * Constants.ZERO_GROUND_LEVEL_Y * 2.0f;
        text.text = searchResultText;
    }
}
