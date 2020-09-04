using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PandObject : MonoBehaviour
{
    //[SerializeField] private Text indexBAGText = default;
    [SerializeField] private Text adresText = default;
    [SerializeField] private Text woningTypeText = default;
    [SerializeField] private Text bouwJaarText = default;
    [SerializeField] private Button closeButton = default;

    private void Start()
    {
        closeButton.onClick.AddListener(CloseObject);
    }

    public void SetText(Pand.Rootobject pandData, int adresIndex)
    {
        //Zet alle pand data en displayed het in de UI.
        //indexBAGText.text = pandData.results[0].landelijk_id;
        adresText.text = pandData.results[adresIndex]._display;
        woningTypeText.text = pandData.type_woonobject;
        bouwJaarText.text = pandData.oorspronkelijk_bouwjaar;
    }

    private void CloseObject()
    {
        Destroy(this.gameObject); // later kan je dit object poolen als optimalisatie maar als nog één malig instantieren ipv alles tegelijkertijd, scheelt optimalisatie
    }
}
