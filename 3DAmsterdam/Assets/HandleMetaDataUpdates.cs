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


    void Start()
    {
        MetadataLoader.Instance.BuildingMetaDataLoaded += BuildingMetaDataLoaded;
        MetadataLoader.Instance.AddressLoaded += AddressLoaded;
        MetadataLoader.Instance.PerceelDataLoaded += PerceelDataLoaded;
    }

    private void PerceelDataLoaded(object source, PerceelDataEventArgs args)
    {
        PerceelGrootteText.text = args.PerceelGrootte;
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
