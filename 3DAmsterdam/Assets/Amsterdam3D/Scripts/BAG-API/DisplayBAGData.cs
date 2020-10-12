using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayBAGData : MonoBehaviour
{
    public GameObject premisesInterfacePanel;
    public GameObject loadingCirle;

    [SerializeField] private Text indexBAGText = default;
    
    [SerializeField] public Transform buttonObjectTargetSpawn = default;
    public GameObject premisesUIButton;
    private List<PremisesButton> premisesButtons = new List<PremisesButton>();

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
        premisesInterfacePanel.SetActive(false);
        premises.premisesGameObject.SetActive(false);
        streetToggle.gameObject.SetActive(false);
        loadingCirle.SetActive(false);
    }
    /// <summary>
    /// cleans up the ui and disables all elements except for the main premises list UI screen
    /// </summary>
    public void PrepareUI()
    {
        wkbp.wkbpToggle.gameObject.SetActive(false);
        wkbp.wkbpParent.SetActive(false);
        premises.CloseObject();
        premisesInterfacePanel.SetActive(true);
        loadingCirle.SetActive(true);
        RemoveButtons();
        buttonObjectTargetSpawn.gameObject.SetActive(true);
    }
    /// <summary>
    /// Generates all premises buttons. If there is only 1 premises it will show just that premises
    /// </summary>
    /// <param name="pandData"></param>
    public void ShowData(Pand.Rootobject pandData)
    {
        RemoveButtons();
        scroll.value = 1f;
        if (pandData.results.Length > 0)
        {
            // starts ui en cleans up all previous buttons
            indexBAGText.text = pandData._display;
            // check if there's more than one adress

            if (pandData.results.Length > 1)
            {
                // creates a button for each adress
                for (int i = 0; i < pandData.results.Length; i++)
                {
                    GameObject temp = Instantiate(premisesUIButton, buttonObjectTargetSpawn.position, buttonObjectTargetSpawn.rotation);
                    temp.transform.SetParent(buttonObjectTargetSpawn);
                    PremisesButton tempButton = temp.GetComponent<PremisesButton>();
                    // puts the buttons in a list
                    premisesButtons.Add(tempButton);
                    // adds the data to the button
                    tempButton.Initiate(pandData, i);
                }
                loadingCirle.SetActive(false);
            }
            else
            {
                // if there is only one adress, then show the only adress available
                StartCoroutine(PlaceCoroutine(pandData, 0));
            }
        }
    }
    /// <summary>
    /// Places premises but doesn't wait for it to be completed due to the data loading fast
    /// </summary>
    /// <param name="pandData"></param>
    /// <param name="index"></param>
    public void PlacePremises(Pand.Rootobject pandData, int index)
    {
        StartCoroutine(ImportBAG.Instance.CallAPI("https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/", ImportBAG.Instance.hoofdData.results[index].landelijk_id, RetrieveType.NummeraanduidingInstance));

        // stuurt de pand data door
        premises.premisesGameObject.SetActive(true);
        premises.SetText(pandData, index);

        if(loadingCirle.activeSelf)
            loadingCirle.SetActive(false);
    }
    /// <summary>
    /// waits for the premises data to be loaded and then loads it in
    /// </summary>
    /// <param name="pandData"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public IEnumerator PlaceCoroutine(Pand.Rootobject pandData, int index)
    {
        // waits till all the data is ready
        yield return StartCoroutine(ImportBAG.Instance.CallAPI("https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/", ImportBAG.Instance.hoofdData.results[index].landelijk_id, RetrieveType.NummeraanduidingInstance));

        // stuurt de pand data door
        premises.premisesGameObject.SetActive(true);
        premises.SetText(pandData, index);

        if (loadingCirle.activeSelf)
            loadingCirle.SetActive(false);
    }
    
    /// <summary>
    /// removes all the old premises buttons from the main ui hub
    /// </summary>
    public void RemoveButtons()
    {
        foreach(PremisesButton btn in premisesButtons)
        {
            Destroy(btn.gameObject);
           
        }
        premisesButtons.Clear();
    }
}

