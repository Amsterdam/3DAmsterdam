using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HandleMetaDataUpdates : MonoBehaviour
{    
    public Text AdresText;
    public Text PostcodePlaatsText;
    public Text PerceelGrootteText;

    public GameObject IsMonument;
    public GameObject IsBeschermd;

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

        StartCoroutine(SetSidebarAreaText());
    }

    private void IsMonumentEvent()
    {
        IsMonument.SetActive(true);
    }

    private void IsBeschermdEvent()
    {
        IsBeschermd.SetActive(true);
    }

    private void PerceelDataLoaded(object source, PerceelDataEventArgs args)
    {
        perceelIsLoaded = true;
        perceelArea = args.Area;
        print("perceel outline laoded");
        //PerceelGrootteText.text += "Totaal Perceeloppervlakte: " + args.Area.ToString("F2") + "m²";
    }

    private void AddressLoaded(object source, AdressDataEventArgs args)
    {
        AdresText.text = args.StraatEnNummer;
        PostcodePlaatsText.text = args.PostcodeEnPlaats;
    }

    private void BuildingMetaDataLoaded(object source, Netherlands3D.T3D.Uitbouw.ObjectDataEventArgs args)
    {
    }

    private void Instance_BuildingOutlineLoaded(object source, BuildingOutlineEventArgs args)
    {
        buildingOutlineIsLoaded = true;
        builtArea = args.TotalArea;
        print("building outline laoded");
        //PerceelGrootteText.text += "Bebouwd Perceeloppervlakte: " + args.TotalArea.ToString("F2") + "m²";
    }

    private IEnumerator SetSidebarAreaText()
    {
        PerceelGrootteText.text = string.Empty;
        yield return new WaitUntil(()=> perceelIsLoaded && buildingOutlineIsLoaded);

        var unbuiltArea = (perceelArea - builtArea);
        //var maxAllowedAreaRestriction = RestrictionChecker.ActiveRestrictions.FirstOrDefault(restriction => restriction is PerceelAreaRestriction) as PerceelAreaRestriction;
        //var maxAllowedFraction = maxAllowedAreaRestriction.MaxAreaPercentage;

        PerceelGrootteText.text += "Totaal perceeloppervlakte: " + perceelArea.ToString("F2") + "m²\n" +
                                    "Bebouwd perceeloppervlakte: " + builtArea.ToString("F2") + "m²\n" +
                                    "Totaal beschikbaar oppervlakte: " + unbuiltArea.ToString("F2") + "m²\n" +
                                    "<b>Toegestaan bouwoppervlakte: " + (unbuiltArea * PerceelAreaRestriction.MaxAreaFraction).ToString("F2") + "m²</b>";
    }
}
