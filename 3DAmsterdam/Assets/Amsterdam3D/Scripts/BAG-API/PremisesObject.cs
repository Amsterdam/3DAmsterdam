using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PremisesObject : MonoBehaviour
{
    //[SerializeField] private Text indexBAGText = default;
    [Header("Main Properties")]
    [SerializeField] private Text numberIndexText = default;
    [SerializeField] private Text adressText = default;
    [SerializeField] private Text zipText = default;
    [SerializeField] private Text houseTypeText = default;
    [SerializeField] private Text constructionYearText = default;
    [SerializeField] private Text neighbourhood = default;
    [SerializeField] private Text neighbourhoodCombination = default;
    [SerializeField] private Text district = default;
    [SerializeField] private Text rent = default;
    [SerializeField] private Text surface = default;
    [SerializeField] private Text roomCount = default;
    [SerializeField] private Text buildingLayerCount = default;
    [SerializeField] private Text highestBuildingLayer = default;
    [SerializeField] private Text lowestBuildingLayer = default;
    [SerializeField] private Text floorAccess = default;
    [SerializeField] private Text destinationPlan = default;
    [SerializeField] private Text function = default;
    //[SerializeField] private Text gebruiksOppervlakte = default;
    [SerializeField] private Text categoryPermits = default;
    [SerializeField] private Text categoryTopic = default;
    [SerializeField] private Text categoryTitle = default;
    [SerializeField] private Text categoryURL = default;
    [SerializeField] private Text monument = default;
    [SerializeField] private Button restrictions = default;
    [SerializeField] private Text housingCorporationName = default;

    [Header("Buttons")]
    [SerializeField] private Button closeButton = default;
    [SerializeField] private Scrollbar scroll = default;
    [SerializeField] private Toggle streetName = default;
    [SerializeField] private Text streetText = default;

    private int adressIndex = 0;
    private Pand.Rootobject thisPremises = new Pand.Rootobject();

    public GameObject premisesGameObject = default;


    private void Start()
    {
        closeButton.onClick.AddListener(CloseObject);
        restrictions.onClick.AddListener(LoadWKBP);
    }

    /// <summary>
    /// Sets text for the premis
    /// </summary>
    /// <param name="premisesData"></param>
    /// <param name="Index"></param>
    public void SetText(Pand.Rootobject premisesData, int Index)
    {
        // zet de pand data
        scroll.value = 1f;
        thisPremises = premisesData;
        adressIndex = Index;
        streetName.gameObject.SetActive(true);
        streetName.isOn = true;
        streetText.text = premisesData.results[adressIndex].nummeraanduiding.adres;
        // zet de terug knop uit als er maar één pand is
        if (premisesData.results.Length <= 1)
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
        numberIndexText.text = premisesData.results[adressIndex].nummeraanduiding.nummeraanduidingidentificatie;
        adressText.text = premisesData.results[adressIndex].nummeraanduiding.adres + " " + premisesData.results[adressIndex].nummeraanduiding.postcode + " " + "Amsterdam";
        zipText.text = premisesData.results[adressIndex].nummeraanduiding.postcode;
        houseTypeText.text = premisesData.results[adressIndex].nummeraanduiding.type_adres;
        constructionYearText.text = premisesData.oorspronkelijk_bouwjaar;
        neighbourhood.text = premisesData._buurt.naam;
        neighbourhoodCombination.text = premisesData._buurtcombinatie.naam;
        district.text = premisesData._stadsdeel.naam;
        rent.text = premisesData.results[adressIndex].nummeraanduiding.type_adres;
        surface.text = premisesData.results[adressIndex].verblijfsobject.oppervlakte + " M²";
        roomCount.text = string.IsNullOrEmpty(premisesData.results[adressIndex].verblijfsobject.aantal_kamers) ? "Onbekend" : premisesData.results[adressIndex].verblijfsobject.aantal_kamers;
        buildingLayerCount.text = string.IsNullOrEmpty(premisesData.bouwlagen) ? "Onbekend" : premisesData.bouwlagen;
        highestBuildingLayer.text = string.IsNullOrEmpty(premisesData.hoogste_bouwlaag) ? "Onbekend" : premisesData.hoogste_bouwlaag;
        lowestBuildingLayer.text = string.IsNullOrEmpty(premisesData.laagste_bouwlaag) ? "Onbekend" : premisesData.laagste_bouwlaag;
        floorAccess.text = string.IsNullOrEmpty(premisesData.results[adressIndex].verblijfsobject.verdieping_toegang) ? "Onbekend" : premisesData.results[adressIndex].verblijfsobject.verdieping_toegang;
        destinationPlan.text = "Onbekend";
        function.text = "Onbekend";
        //gebruiksOppervlakte.text = premisesData.results[adresIndex].verblijfsobject.oppervlakte + " M²";
        categoryPermits.text = "Onbekend";
        categoryTopic.text = "Onbekend";
        categoryTitle.text = "Onbekend";
        categoryURL.text = "Onbekend";
        // kijkt of er wel monumenten zijn
        /*
        if (premisesData.monumenten.results.Length > 0)
        {
            monument.text = "Ja, " + premisesData.monumenten.results[0].monumentnummer;
        }
        else 
        {
            monument.text = "Nee";
        }
        */
        monument.text = "Onbekend";
        housingCorporationName.text = premisesData.results[adressIndex].verblijfsobject.eigendomsverhouding;
        DisplayBAGData.Instance.loadingCirle.SetActive(false); // loading bar
    }
    /// <summary>
    /// loads WKBP Objects
    /// </summary>
    public void LoadWKBP()
    {
        DisplayBAGData.Instance.loadingCirle.SetActive(true); // loading bar
        StartCoroutine(DisplayBAGData.Instance.wkbp.LoadWKBP(thisPremises, adressIndex));
        premisesGameObject.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        streetName.gameObject.SetActive(false);
    }
    /// <summary>
    /// Closes the premises object
    /// </summary>
    public void CloseObject()
    {
        premisesGameObject.SetActive(false); // later kan je dit object poolen als optimalisatie maar als nog één malig instantieren ipv alles tegelijkertijd, scheelt mogenlijk optimalisatie
        streetName.gameObject.SetActive(false);
    }
}
