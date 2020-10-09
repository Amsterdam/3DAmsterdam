using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayBAGData : MonoBehaviour
{
    public GameObject ui;
    public GameObject loadingCirle;

    [SerializeField] private Text indexBAGText = default;
    
    [SerializeField] public Transform buttonObjectTargetSpawn = default;
    public GameObject premisesUIButton;
    [SerializeField] private List<PremisesButton> premisesButtons = new List<PremisesButton>();

    [SerializeField] private Toggle streetToggle = default;
    [SerializeField] private Scrollbar scroll = default;

    public PremisesObject premises;
    public WKBPObject wkbp;
    public static DisplayBAGData Instance = null;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        ui.SetActive(false);
        premises.premisesGameObject.SetActive(false);
        streetToggle.gameObject.SetActive(false);
        loadingCirle.SetActive(false);
    }

    public void PrepareUI()
    {
        wkbp.wkbpToggle.gameObject.SetActive(false);
        wkbp.wkbpParent.SetActive(false);
        premises.CloseObject();
        ui.SetActive(true);
        loadingCirle.SetActive(true);
        /// removes all the old buttons from the main Premises UI Screen
        RemoveButtons();
        buttonObjectTargetSpawn.gameObject.SetActive(true);
    }

    public void ShowData(Pand.Rootobject pandData)
    {
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
                    GameObject temp = Instantiate(premisesUIButton, buttonObjectTargetSpawn.position, buttonObjectTargetSpawn.rotation);
                    //temp.name = pandData.results[i]._display;
                    temp.transform.SetParent(buttonObjectTargetSpawn);
                    PremisesButton tempButton = temp.GetComponent<PremisesButton>();
                    // zet de knop in de lijst met buttons
                    premisesButtons.Add(tempButton);
                    // voegt de data aan de knop
                    tempButton.Initiate(pandData, i);
                }
                loadingCirle.SetActive(false);
            }
            else
            {
                // als er maar één adres is dan laat hij er maar één zien ipv alle buttons
                StartCoroutine(PlaceCoroutine(pandData, 0));
            }
        }
    }

    public void PlacePremises(Pand.Rootobject pandData, int index)
    {
        StartCoroutine(ImportBAG.Instance.CallAPI("https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/", ImportBAG.Instance.hoofdData.results[index].landelijk_id, RetrieveType.NummeraanduidingInstance));

        // stuurt de pand data door
        premises.premisesGameObject.SetActive(true);
        premises.SetText(pandData, index);

        if(loadingCirle.activeSelf)
            loadingCirle.SetActive(false);
    }
    
    public IEnumerator PlaceCoroutine(Pand.Rootobject pandData, int index)
    {
        // wacht tot alle data binnen is
        yield return StartCoroutine(ImportBAG.Instance.CallAPI("https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/", ImportBAG.Instance.hoofdData.results[index].landelijk_id, RetrieveType.NummeraanduidingInstance));

        // stuurt de pand data door
        premises.premisesGameObject.SetActive(true);
        premises.SetText(pandData, index);

        if (loadingCirle.activeSelf)
            loadingCirle.SetActive(false);
    }
    

    public void RemoveButtons()
    {
        foreach(PremisesButton btn in premisesButtons)
        {
            Destroy(btn.gameObject);
           
        }
        premisesButtons.Clear();
    }
}

