using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PandObject : MonoBehaviour
{
    [SerializeField] private Text indexBAGText = default;
    [SerializeField] private Text adresText = default;
    [SerializeField] private Text woningTypeText = default;
    [SerializeField] private Text bouwJaarText = default;

    public void SetText(Pand.Rootobject pandData)
    {
        //Zet alle pand data en displayed het in de UI.
        indexBAGText.text = pandData.results[0].landelijk_id;
        adresText.text = pandData.results[0]._display;
        woningTypeText.text = pandData.type_woonobject;
        bouwJaarText.text = pandData.oorspronkelijk_bouwjaar;
    }
}
