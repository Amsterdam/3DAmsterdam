using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandleMetaDataUpdates : MonoBehaviour
{    
    public Text AdresText;
    public Text PostcodePlaatsText;
    public Text PerceelGrootteText;

    public GameObject IsMonument;
    public GameObject IsBeschermd;


    void Start()
    {
        MetadataLoader.Instance.BuildingMetaDataLoaded += BuildingMetaDataLoaded;
        MetadataLoader.Instance.AddressLoaded += AddressLoaded;
        MetadataLoader.Instance.PerceelDataLoaded += PerceelDataLoaded;
        MetadataLoader.Instance.IsMonumentEvent += IsMonumentEvent;
        MetadataLoader.Instance.IsBeschermdEvent += IsBeschermdEvent;
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
        PerceelGrootteText.text = "Perceeloppervlakte: " + args.Area.ToString("F2") + "m²";
    }

    private void AddressLoaded(object source, AdressDataEventArgs args)
    {
        AdresText.text = args.StraatEnNummer;
        PostcodePlaatsText.text = args.PostcodeEnPlaats;
    }

    private void BuildingMetaDataLoaded(object source, Netherlands3D.T3D.Uitbouw.ObjectDataEventArgs args)
    {        
    }

    
}
