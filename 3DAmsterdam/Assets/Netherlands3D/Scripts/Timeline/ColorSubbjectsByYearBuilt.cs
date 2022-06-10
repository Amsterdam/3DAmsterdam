using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSubbjectsByYearBuilt : MonoBehaviour
{
    [SerializeField]
    private ObjectEvent gotIdsAndYears;

    [SerializeField]
    private ObjectEvent applyColorsToIds;

    [SerializeField]
    private Color buildYearBeforeColor;

    [SerializeField]
    private Color buildYearAfterColor;

    Dictionary<string, float> keyValuePairs;
    Dictionary<string, Color> bagIdAndColors = new Dictionary<string, Color>();

    [SerializeField]
    private BoolEvent workingOnColoring;

    private void OnEnable()
    {
        gotIdsAndYears.started.AddListener(GotListIdsAndYears);
    }

    private void OnDisable()
    {
        gotIdsAndYears.started.RemoveListener(GotListIdsAndYears);
        workingOnColoring.started.Invoke(false);
    }

    private void GotListIdsAndYears(object idsAndYears)
    {
        keyValuePairs = (Dictionary<string, float>)idsAndYears;
        ApplyYearFromDateTime(DateTime.Now);
    }

    public void ApplyYearFromDateTime(DateTime dateTime)
    {
        if (keyValuePairs != null)
        {
            workingOnColoring.started.Invoke(true);

            Debug.Log("Coloring by year built");

            bagIdAndColors.Clear();

            foreach(KeyValuePair<string, float> pair in keyValuePairs) { 
                bagIdAndColors.Add(pair.Key,(pair.Value < dateTime.Year) ? buildYearBeforeColor : buildYearAfterColor);
            }

            applyColorsToIds.started.Invoke(bagIdAndColors);

            workingOnColoring.started.Invoke(false);
        }
    }
}
