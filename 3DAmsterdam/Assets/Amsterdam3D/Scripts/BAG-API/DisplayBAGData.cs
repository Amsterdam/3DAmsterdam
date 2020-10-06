using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayBAGData : MonoBehaviour
{
    public GameObject ui;
    public GameObject loadingCirle;

    [SerializeField] private Text indexBAGText = default;
    
    [SerializeField] private Transform buttonObjectTargetSpawn = default;
    public GameObject pandUIButton;
    [SerializeField] private List<PandButton> pandButtons = new List<PandButton>();

    [SerializeField] private Toggle straatToggle = default;
    [SerializeField] private Scrollbar scroll = default;

    public PandObject pand;
    public WKBPObject wkbp;
    public static DisplayBAGData Instance = null;


    private void Awake()
    {
        // maak een singleton zodat je deze class contant kan aanroepen vanuit elke hoek
        if(Instance == null)
        {
            Instance = this;
        }
        ui.SetActive(false);
        pand.gameObject.SetActive(false);
        straatToggle.gameObject.SetActive(false);
        loadingCirle.SetActive(false);
    }

    public void ShowData(Pand.Rootobject pandData)
    {
        ui.SetActive(true);
        RemoveButtons();
        scroll.value = 1f;
        if (pandData.results.Length > 0)
        {
            // starts ui en cleans up all previous buttons

            indexBAGText.text = pandData._display;
            // is er meer dan één pand, dan laat hij alle adressen zien
            if (pandData.results.Length > 1)
            {
                // maakt een knop aan voor elk adres
                for (int i = 0; i < pandData.results.Length; i++)
                {
                    GameObject temp = Instantiate(pandUIButton, buttonObjectTargetSpawn.position, buttonObjectTargetSpawn.rotation);
                    //temp.name = pandData.results[i]._display;
                    temp.transform.SetParent(buttonObjectTargetSpawn);
                    PandButton tempButton = temp.GetComponent<PandButton>();
                    // zet de knop in de lijst met buttons
                    pandButtons.Add(tempButton);
                    // voegt de data aan de knop
                    tempButton.Initiate(pandData, i);
                }
                loadingCirle.SetActive(false);
            }
            else
            {
                // als er maar één adres is dan laat hij er maar één zien ipv alle buttons
                //PlacePand(pandData, 0);
                StartCoroutine(PlaceCoroutine(pandData, 0));
            }
        }
    }

    public void PlacePand(Pand.Rootobject pandData, int index)
    {
        StartCoroutine(ImportBAG.Instance.CallAPI("https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/", ImportBAG.Instance.hoofdData.results[index].landelijk_id, RetrieveType.NummeraanduidingInstance));

        // stuurt de pand data door

        pand.gameObject.SetActive(true);
        //straatToggle.isOn = true;
        pand.SetText(pandData, index);

        if(loadingCirle.activeSelf)
            loadingCirle.SetActive(false);
    }
    
    public IEnumerator PlaceCoroutine(Pand.Rootobject pandData, int index)
    {
        // wacht tot alle data binnen is
        yield return StartCoroutine(ImportBAG.Instance.CallAPI("https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/", ImportBAG.Instance.hoofdData.results[index].landelijk_id, RetrieveType.NummeraanduidingInstance));

        // stuurt de pand data door
        //straatToggle.gameObject.SetActive(true);
        pand.gameObject.SetActive(true);
        //straatToggle.isOn = true;
        pand.SetText(pandData, index);

        if (loadingCirle.activeSelf)
            loadingCirle.SetActive(false);
    }
    

    public void RemoveButtons()
    {
        foreach(PandButton btn in pandButtons)
        {
            Destroy(btn.gameObject);
           
        }
        pandButtons.Clear();
    }
}

