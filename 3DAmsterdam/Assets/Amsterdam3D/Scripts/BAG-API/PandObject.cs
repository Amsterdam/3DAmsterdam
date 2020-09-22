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
    [SerializeField] private Text typeBeperking = default;
    [SerializeField] private Text beperkingID = default;
    [SerializeField] private Text woningcorperatieNaam = default;

    [Header("Beperkingen")]
    [SerializeField] private GameObject wkbpInstancePlacer = default;
    [SerializeField] private GameObject wkbpObject = default;

    [Header("Button")]
    [SerializeField] private Button closeButton = default;


    private void Start()
    {
        closeButton.onClick.AddListener(CloseObject);
        
    }

    private void OnDisable()
    {
        Destroy(this.gameObject); // mogenlijk kan dit ook worden geobject pooled voor latere optimalisatie
    }

    public void SetText(Pand.Rootobject pandData, int adresIndex)
    {
        if(pandData.results.Length == 1)
        {
            closeButton.gameObject.SetActive(false);
        }
        //Zet alle pand data en displayed het in de UI.
        nummerAanduidingText.text = pandData.results[adresIndex].nummeraanduiding.nummeraanduidingidentificatie;
        adresText.text = pandData.results[adresIndex].nummeraanduiding.adres;
        postcodeText.text = pandData.results[adresIndex].nummeraanduiding.postcode;
        woningTypeText.text = pandData.results[adresIndex].nummeraanduiding.type_adres;
        bouwJaarText.text = pandData.oorspronkelijk_bouwjaar;
        buurt.text = pandData._buurt.naam;
        buurtCombinatie.text = pandData._buurtcombinatie.naam;
        stadsdeel.text = pandData._stadsdeel.naam;
        huur.text = pandData.results[adresIndex].nummeraanduiding.type_adres;
        oppervlakte.text = pandData.results[adresIndex].verblijfsobject.oppervlakte + " M²";
        aantalKamers.text = pandData.results[adresIndex].verblijfsobject.aantal_kamers;
        aantalBouwlagen.text = pandData.bouwlagen;
        hoogsteBouwlaag.text = pandData.hoogste_bouwlaag;
        laagsteBouwlaag.text = pandData.laagste_bouwlaag;
        verdiepingToegang.text = pandData.results[adresIndex].verblijfsobject.verdieping_toegang;
        bestemmingsPlan.text = "ONTBREEKT";
        functie.text = "ONTBREEKT";
        //gebruiksOppervlakte.text = pandData.results[adresIndex].verblijfsobject.oppervlakte + " M²";
        categorieVergunning.text = "ONTBREEKT";
        categorieOnderwerp.text = "ONTBREEKT";
        categorieTitel.text = "ONTBREEKT";
        categorieURL.text = "ONTBREEKT";
        if (pandData.monumenten.results.Length > 0)
        {
            monument.gameObject.transform.parent.gameObject.SetActive(true);
            monument.text = pandData.monumenten.results[0].monumentnummer;
        }
        else 
        { 
            monument.gameObject.transform.parent.gameObject.SetActive(false);
        }
        typeBeperking.text = pandData.status;
        
        foreach(WKBP.Result result in pandData.results[adresIndex].verblijfsobject.wkbpBeperkingen.results)
        {
            Debug.Log("Poep");
            GameObject tempBeperkingObj = Instantiate(wkbpObject, wkbpInstancePlacer.transform.position, wkbpInstancePlacer.transform.rotation);
            tempBeperkingObj.transform.SetParent(wkbpInstancePlacer.transform);
            tempBeperkingObj.GetComponent<WKBPObject>().Initialize(result);
        }
        beperkingID.text = "GEBRUIKS OPPERVLAKTE";
        woningcorperatieNaam.text = pandData.results[adresIndex].verblijfsobject.eigendomsverhouding;
    }

    private void CloseObject()
    {
        Destroy(this.gameObject); // later kan je dit object poolen als optimalisatie maar als nog één malig instantieren ipv alles tegelijkertijd, scheelt mogenlijk optimalisatie
    }
}
