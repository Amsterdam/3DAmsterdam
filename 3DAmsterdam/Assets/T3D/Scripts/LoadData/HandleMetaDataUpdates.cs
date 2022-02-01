using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HandleMetaDataUpdates : MonoBehaviour
{
    public Text[] AdresText;
    public Text[] PostcodePlaatsText;
    public Text PerceelGrootteText;
    public Text BimModelStatus;

    public Text IsMonumentText;
    private const string isMonumentString = "Dit gebouw is een rijksmonument.";
    private const string notMonumentString = "Dit gebouw is geen rijksmonument.";

    public Text IsBeschermdText;
    private const string isBeschermdtString = "Dit gebouw ligt in een rijks-\nbeschermd stads- of dorpsgezicht";
    private const string notBeschermdString = "Dit gebouw ligt niet in een rijks-\nbeschermd stads- of dorpsgezicht.";

    private bool perceelIsLoaded = false;
    private bool buildingOutlineIsLoaded = false;

    private float perceelArea;
    private float builtArea;

    void Start()
    {
        MetadataLoader.Instance.BuildingMetaDataLoaded += BuildingMetaDataLoaded;
        MetadataLoader.Instance.AddressLoaded += AddressLoaded;
        MetadataLoader.Instance.PerceelDataLoaded += PerceelDataLoaded;
        MetadataLoader.Instance.IsMonumentEvent += IsMonumentEvent;
        MetadataLoader.Instance.IsBeschermdEvent += IsBeschermdEvent;
        MetadataLoader.Instance.BuildingOutlineLoaded += Instance_BuildingOutlineLoaded;        

        //StartCoroutine(SetSidebarAreaText());
    }

    private void IsMonumentEvent(bool isMonument)
    {
        if (isMonument)
            IsMonumentText.text = isMonumentString;
        else
            IsMonumentText.text = notMonumentString;
    }

    private void IsBeschermdEvent(bool isBeschermd)
    {
        if (isBeschermd)
            IsBeschermdText.text = isBeschermdtString;
        else
            IsBeschermdText.text = notBeschermdString;
    }

    private void PerceelDataLoaded(object source, PerceelDataEventArgs args)
    {
        perceelIsLoaded = true;
        perceelArea = args.Area;        
    }

    private void AddressLoaded(object source, AdressDataEventArgs args)
    {
        foreach(var adrestext in AdresText)
        {
            adrestext.text = $"{args.Straat} {args.Huisnummer}";
        }

        foreach (var postcodeplaats in PostcodePlaatsText)
        {
            postcodeplaats.text = $"{args.Postcode} {args.Plaats}";
        }
        
    }

    private void BuildingMetaDataLoaded(object source, Netherlands3D.T3D.Uitbouw.ObjectDataEventArgs args)
    {
    }

    private void Instance_BuildingOutlineLoaded(object source, BuildingOutlineEventArgs args)
    {
        buildingOutlineIsLoaded = true;
        builtArea = args.TotalArea;                
    }

    private IEnumerator SetSidebarAreaText()
    {
        PerceelGrootteText.text = string.Empty;
        yield return new WaitUntil(() => perceelIsLoaded && buildingOutlineIsLoaded);

        var unbuiltArea = (perceelArea - builtArea);
        //var maxAllowedAreaRestriction = RestrictionChecker.ActiveRestrictions.FirstOrDefault(restriction => restriction is PerceelAreaRestriction) as PerceelAreaRestriction;
        //var maxAllowedFraction = maxAllowedAreaRestriction.MaxAreaPercentage;

        PerceelGrootteText.text += "Totaal perceeloppervlakte: " + perceelArea.ToString("F2") + "m²\n" +
                                    "Bebouwd perceeloppervlakte: " + builtArea.ToString("F2") + "m²\n" +
                                    "Totaal beschikbaar oppervlakte: " + unbuiltArea.ToString("F2") + "m²\n" +
                                    "<b>Toegestaan bouwoppervlakte: " + (unbuiltArea * PerceelAreaRestriction.MaxAreaFraction).ToString("F2") + "m²</b>";
    }
}
