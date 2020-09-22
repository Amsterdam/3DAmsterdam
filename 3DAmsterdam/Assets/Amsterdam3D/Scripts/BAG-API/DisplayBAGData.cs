using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayBAGData : MonoBehaviour
{
    public GameObject ui;

    [SerializeField] private Text indexBAGText = default;
    
    [SerializeField] private Transform pandObjectTargetSpawn = default;
    public GameObject pandUIPrefab;
    [SerializeField] private Transform buttonObjectTargetSpawn = default;
    public GameObject pandUIButton;
    [SerializeField] private List<PandButton> pandButtons = new List<PandButton>();

    public static DisplayBAGData Instance = null;

    private void Awake()
    {
        // maak een singleton zodat je deze class contant kan aanroepen vanuit elke hoek
        if(Instance == null)
        {
            Instance = this;
        }
        ui.SetActive(false);    
    }

    public void ShowData(Pand.Rootobject pandData)
    {
        ui.SetActive(true);
        RemoveButtons();
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
        StartCoroutine(ImportBAG.Instance.CallAPI(ImportBAG.Instance.hoofdData.results[index].nummeraanduiding.verblijfsobject, "", RetrieveType.VerblijfsobjectInstance));
        GameObject temp = Instantiate(pandUIPrefab, pandObjectTargetSpawn.position, pandObjectTargetSpawn.rotation);
        temp.transform.SetParent(pandObjectTargetSpawn);
        // reset de transform naar het midden
        temp.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
        PandObject tempPand = temp.GetComponent<PandObject>();
        // stuurt de pand data door
        tempPand.SetText(pandData, index);
    }
    
    public IEnumerator PlaceCoroutine(Pand.Rootobject pandData, int index)
    {
        // wacht tot alle data binnen is
        yield return StartCoroutine(ImportBAG.Instance.CallAPI(ImportBAG.Instance.hoofdData.results[index].nummeraanduiding.verblijfsobject, "", RetrieveType.VerblijfsobjectInstance));
        GameObject temp = Instantiate(pandUIPrefab, pandObjectTargetSpawn.position, pandObjectTargetSpawn.rotation);
        temp.transform.SetParent(pandObjectTargetSpawn);
        // reset de transform naar het midden
        temp.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
        PandObject tempPand = temp.GetComponent<PandObject>();
        // stuurt de pand data door
        tempPand.SetText(pandData, index);
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

