using UnityEngine;
using UnityEngine.UI;
using BruTile;
using System.Collections.Generic;

[System.Serializable]
public class DropDownOptions
{
    public string naam;
    public string url;
}

public class SetKaartlaag : MonoBehaviour
    

{
    public List<DropDownOptions> keuzes = new List<DropDownOptions>();
    Dropdown m_Dropdown;


    public GameObject TileObject;

    private CameraView CV;



    void Start()
    {
        m_Dropdown = GetComponent<Dropdown>();
        if (keuzes.Count==0)
        {
            setDefault();
        }
        SetActive();
        //m_Dropdown.AddOptions();
        m_Dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(m_Dropdown);
        });

        //Initialise the Text to say the first value of the Dropdown
        //m_Text.text = "Kaartlaag";
    }

    private void setDefault()
    {
        m_Dropdown.ClearOptions();

        DropDownOptions optie = new DropDownOptions();
        optie.naam = "Kaart";
        optie.url = "https://saturnus.geodan.nl/mapproxy/bgt/service?crs=EPSG%3A3857&service=WMS&version=1.1.1&request=GetMap&styles=&format=image%2Fjpeg&layers=bgt&bbox={xMin}%2C{yMin}%2C{xMax}%2C{yMax}&width=256&height=256&srs=EPSG%3A4326";
        keuzes.Add(optie);

        optie = new DropDownOptions();
        optie.naam = "Luchtfoto Actueel";
        optie.url = "https://geodata.nationaalgeoregister.nl/luchtfoto/rgb/wms?styles=&layers=Actueel_ortho25&service=WMS&request=GetMap&format=image%2Fpng&version=1.1.0&bbox={xMin}%2C{yMin}%2C{xMax}%2C{yMax}&width=256&height=512&crs=EPSG%3A4326&srs=EPSG%3A4326";
        keuzes.Add(optie);

        optie = new DropDownOptions();
        optie.naam = "Luchtfoto 2016";
        optie.url = "https://geodata.nationaalgeoregister.nl/luchtfoto/rgb/wms?styles=&layers=2016_ortho25&service=WMS&request=GetMap&format=image%2Fpng&version=1.1.0&bbox={xMin}%2C{yMin}%2C{xMax}%2C{yMax}&width=256&height=512&crs=EPSG%3A4326&srs=EPSG%3A4326";
        keuzes.Add(optie);

        List<string> opties = new List<string>();

        for (int i = 0; i < keuzes.Count; i++)
        {
            opties.Add(keuzes[i].naam);
        }
        m_Dropdown.AddOptions(opties);
    }
    private void SetActive()
    {
        Debug.Log(keuzes.Count);
        for (int i = 0; i < keuzes.Count; i++)
        {

            if (keuzes[i].url== TileObject.GetComponent<TileLoader>().textureUrl)
            {
                m_Dropdown.SetValueWithoutNotify(i);
            }
        }

    }

    //Ouput the new value of the Dropdown into Text
    void DropdownValueChanged(Dropdown change)
    {
        TileObject.GetComponent<TileLoader>().textureUrl = keuzes[change.value].url;
        TileObject.GetComponent<TileLoader>().UpdateTerrainTextures();


    }
}