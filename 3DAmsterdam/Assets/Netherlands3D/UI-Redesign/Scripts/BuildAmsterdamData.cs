using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Netherlands3D.Interface.Redesign;

public class BuildAmsterdamData : MonoBehaviour
{
    [Header("Fields")]
    [SerializeField] private TMP_Text bagId;
    [SerializeField] private TMP_Text city;
    [SerializeField] private TMP_Text district;
    [SerializeField] private TMP_Text neighbourhood;
    [SerializeField] private TMP_Text buildYear;
    [SerializeField] private TMP_Text buildLayer;
    [SerializeField] private TMP_Text residenceObjects;
    [SerializeField] private OpenURL url;
    [SerializeField] private Transform addressGroup;

    [Header("Prefabs")]
    [SerializeField] private GameObject textPrefab;


    [Header("Listening event")]
    [SerializeField] private UnityEvent<AmsterdamData> setDataEvent;

    void Awake()
    {
        setDataEvent.AddListener(SetData);
    }

    void Start()
    {
        //Example
        SetData(new AmsterdamData(
            "bag id", 
            "amsterdam", 
            "my district", 
            "my neighbourhood", 
            "2018", 
            "7", 
            "3", 
            "www.google.com", 
            new List<string>() { "address 1", "address 2" }));

    }

    private void SetData(AmsterdamData data)
    {
        bagId.text = data.BagId;
        city.text = data.City;
        district.text = data.District;
        neighbourhood.text = data.Neighbourhood;
        buildYear.text = data.BuildYear;
        buildLayer.text = data.BuildLayer;
        residenceObjects.text = data.ResidenceObjects;
        url.GetComponent<OpenURL>().SetUrl(data.Url);

        foreach(var address in data.Addresses)
        {
            var addressText = Instantiate(textPrefab, addressGroup).GetComponentInChildren<TMP_Text>();
            addressText.text = address;
        }
    }
}

public class AmsterdamData {
    private string bagId;
    private string city;
    private string district;
    private string neighbourhood;
    private string buildYear;
    private string buildLayer;
    private string residenceObjects;
    private string url;
    private List<string> addresses;

    public string BagId => bagId;
    public string City => city;
    public string District => district;
    public string Neighbourhood => neighbourhood;
    public string BuildYear => buildYear;
    public string BuildLayer => buildLayer;
    public string ResidenceObjects => residenceObjects;
    public string Url => url;
    public List<string> Addresses => addresses;

    public AmsterdamData(
        string bagId,
        string city,
        string district,
        string neighbourhood,
        string buildYear,
        string buildLayer,
        string residenceObjects,
        string url,
        List<string> addresses)
    {
        this.bagId = bagId;
        this.city = city;
        this.district = district;
        this.neighbourhood = neighbourhood;
        this.buildYear = buildYear;
        this.buildLayer = buildLayer;
        this.residenceObjects = residenceObjects;
        this.url = url;
        this.addresses = addresses;
    }

}

