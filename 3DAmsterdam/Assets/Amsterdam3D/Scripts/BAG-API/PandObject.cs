using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PandObject : MonoBehaviour
{
    //[SerializeField] private Text indexBAGText = default;
    [Header("Main Properties")]
    [SerializeField] private Text nummerAanduidingText = default;
    [SerializeField] private Text adresText = default;
    [SerializeField] private Text postcodeText = default;
    [SerializeField] private Text woningTypeText = default;
    [SerializeField] private Text bouwJaarText = default;
    [SerializeField] private Text buurt = default;
    [SerializeField] private Text buurtCombinatie = default;
    [SerializeField] private Text stadsdeel = default;
    [SerializeField] private Text huur = default;
    [SerializeField] private Text oppervlakte = default;
    [SerializeField] private Text aantalKamers = default;
    [SerializeField] private Text aantalBouwlagen = default;
    [SerializeField] private Text hoogsteBouwlaag = default;
    [SerializeField] private Text laagsteBouwlaag = default;
    [SerializeField] private Text verdiepingToegang = default;
    [SerializeField] private Text bestemmingsPlan = default;
    [SerializeField] private Text functie = default;
    //[SerializeField] private Text gebruiksOppervlakte = default;
    [SerializeField] private Text categorieVergunning = default;
    [SerializeField] private Text categorieOnderwerp = default;
    [SerializeField] private Text categorieTitel = default;
    [SerializeField] private Text categorieURL = default;
    [SerializeField] private Text monument = default;
    [SerializeField] private Button beperking = default;
    [SerializeField] private Text woningcorperatieNaam = default;

    [Header("Buttons")]
    [SerializeField] private Button closeButton = default;
    [SerializeField] private Scrollbar scroll = default;
    [SerializeField] private Toggle straatnaam = default;
    [SerializeField] private Text straatText = default;

    private int adresIndex = 0;
    private Pand.Rootobject thisPand = new Pand.Rootobject();


    private void Start()
    {
        closeButton.gameObject.SetActive(true);
        closeButton.onClick.AddListener(CloseObject);
        beperking.onClick.AddListener(LoadWKBP);
    }


    public void SetText(Pand.Rootobject pandData, int Index)
    {
        // zet de pand data
        scroll.value = 1f;
        thisPand = pandData;
        adresIndex = Index;
        straatnaam.gameObject.SetActive(true);
        straatnaam.isOn = true;
        straatText.text = pandData.results[adresIndex].nummeraanduiding.adres;
        // zet de terug knop uit als er maar één pand is
        if (pandData.results.Length <= 1)
        {
            if (closeButton.gameObject.activeSelf)
                closeButton.gameObject.SetActive(false);
        }
        else
        {
            if(!closeButton.gameObject.activeSelf)
                closeButton.gameObject.SetActive(true);
        }
        //Zet alle pand data en displayed het in de UI.
        nummerAanduidingText.text = pandData.results[adresIndex].nummeraanduiding.nummeraanduidingidentificatie;
        adresText.text = pandData.results[adresIndex].nummeraanduiding.adres + " " + pandData.results[adresIndex].nummeraanduiding.postcode + " " + "Amsterdam";
        ///postcodeText.text = pandData.results[adresIndex].nummeraanduiding.postcode;
        woningTypeText.text = pandData.results[adresIndex].nummeraanduiding.type_adres;
        bouwJaarText.text = pandData.oorspronkelijk_bouwjaar;
        buurt.text = pandData._buurt.naam;
        buurtCombinatie.text = pandData._buurtcombinatie.naam;
        stadsdeel.text = pandData._stadsdeel.naam;
        huur.text = pandData.results[adresIndex].nummeraanduiding.type_adres;
        oppervlakte.text = pandData.results[adresIndex].verblijfsobject.oppervlakte + " M²";
        aantalKamers.text = string.IsNullOrEmpty(pandData.results[adresIndex].verblijfsobject.aantal_kamers) ? "Onbekend" : pandData.results[adresIndex].verblijfsobject.aantal_kamers;
        aantalBouwlagen.text = string.IsNullOrEmpty(pandData.bouwlagen) ? "Onbekend" : pandData.bouwlagen;
        hoogsteBouwlaag.text = string.IsNullOrEmpty(pandData.hoogste_bouwlaag) ? "Onbekend" : pandData.hoogste_bouwlaag;
        laagsteBouwlaag.text = string.IsNullOrEmpty(pandData.laagste_bouwlaag) ? "Onbekend" : pandData.laagste_bouwlaag;
        verdiepingToegang.text = string.IsNullOrEmpty(pandData.results[adresIndex].verblijfsobject.verdieping_toegang) ? "Onbekend" : pandData.results[adresIndex].verblijfsobject.verdieping_toegang;
        bestemmingsPlan.text = "Onbekend";
        functie.text = "Onbekend";
        //gebruiksOppervlakte.text = pandData.results[adresIndex].verblijfsobject.oppervlakte + " M²";
        categorieVergunning.text = "Onbekend";
        categorieOnderwerp.text = "Onbekend";
        categorieTitel.text = "Onbekend";
        categorieURL.text = "Onbekend";
        // kijkt of er wel monumenten zijn
        /*
        if (pandData.monumenten.results.Length > 0)
        {
            monument.text = "Ja, " + pandData.monumenten.results[0].monumentnummer;
        }
        else 
        {
            monument.text = "Nee";
        }
        */
        woningcorperatieNaam.text = pandData.results[adresIndex].verblijfsobject.eigendomsverhouding;
        DisplayBAGData.Instance.loadingCirle.SetActive(false); // loading bar
    }

    public void LoadWKBP()
    {
        DisplayBAGData.Instance.loadingCirle.SetActive(true); // loading bar
        StartCoroutine(DisplayBAGData.Instance.wkbp.LoadWKBP(thisPand, adresIndex));
    }

    private void CloseObject()
    {
        this.gameObject.SetActive(false); // later kan je dit object poolen als optimalisatie maar als nog één malig instantieren ipv alles tegelijkertijd, scheelt mogenlijk optimalisatie
        straatnaam.gameObject.SetActive(false);
    }
}
