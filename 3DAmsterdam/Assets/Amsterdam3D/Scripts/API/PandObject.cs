using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PandObject : MonoBehaviour
{
    //[SerializeField] private Text indexBAGText = default;
    [SerializeField] private Text nummerAanduidingText = default;
    [SerializeField] private Text adresText = default;
    [SerializeField] private Text postcodeText = default;
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
        nummerAanduidingText.text = pandData.results[adresIndex].adresGegevens.nummeraanduidingidentificatie;
        adresText.text = pandData.results[adresIndex].adresGegevens.adres;
        postcodeText.text = pandData.results[adresIndex].adresGegevens.postcode;
        woningTypeText.text = pandData.results[adresIndex].adresGegevens.type_adres;
        bouwJaarText.text = pandData.oorspronkelijk_bouwjaar;
    }

    private void CloseObject()
    {
        Destroy(this.gameObject); // later kan je dit object poolen als optimalisatie maar als nog één malig instantieren ipv alles tegelijkertijd, scheelt mogenlijk optimalisatie
    }
}
