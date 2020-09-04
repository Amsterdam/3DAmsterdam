using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayBAGData : MonoBehaviour
{
    [SerializeField] private Text indexBAGText = default;
    
    [SerializeField] private Transform pandObjectTargetSpawn = default;
    public GameObject pandUIPrefab;
    [SerializeField] private Transform buttonObjectTargetSpawn = default;
    public GameObject pandUIButton;
    private List<PandButton> pandButtons = new List<PandButton>();

    public static DisplayBAGData Instance = null;

    private void Awake()
    {
        // maak een singleton zodat je deze class contant kan aanroepen vanuit elke hoek
        if(Instance == null)
        {
            Instance = this;
        }   
    }

    public void ShowData(Pand.Rootobject pandData)
    {
        if (pandData.results.Length > 0)
        {
            indexBAGText.text = pandData.results[0].landelijk_id;
            // zet de data om in een text object dat wordt geïnstantieerd
            /*GameObject temp = Instantiate(pandUIPrefab, transform.position, transform.rotation);
            temp.transform.SetParent(transform);
            // reset de transform naar het midden
            //temp.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
            PandObject tempPand = temp.GetComponent<PandObject>();
            // stuurt de pand data door
            tempPand.SetText(pandData);*/
            if (pandData.results.Length > 1)
            {
                // maakt een knop aan voor elk adres
                for (int i = 0; i < pandData.results.Length; i++)
                {
                    GameObject temp = Instantiate(pandUIButton, buttonObjectTargetSpawn.position, buttonObjectTargetSpawn.rotation);
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
                PlacePand(pandData, 0);
            }
        }
    }

    public void PlacePand(Pand.Rootobject pandData, int index)
    {
        GameObject temp = Instantiate(pandUIPrefab, pandObjectTargetSpawn.position, pandObjectTargetSpawn.rotation);
        temp.transform.SetParent(pandObjectTargetSpawn);
        // reset de transform naar het midden
        temp.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
        PandObject tempPand = temp.GetComponent<PandObject>();
        // stuurt de pand data door
        tempPand.SetText(pandData, index);
    }
}
