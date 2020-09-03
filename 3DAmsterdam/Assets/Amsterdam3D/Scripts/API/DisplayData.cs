using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayData : MonoBehaviour
{
    public GameObject pandUIPrefab;

    public static DisplayData Instance = null;
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
        // zet de data om in een text object dat wordt geïnstantieerd
        GameObject temp = Instantiate(pandUIPrefab, transform.position, transform.rotation);
        temp.transform.SetParent(transform);
        // reset de transform naar het midden
        temp.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
        PandObject tempPand = temp.GetComponent<PandObject>();
        // stuurt de pand data door
        tempPand.SetText(pandData);
        if (pandData.results.Length > 0) 
        {

            Debug.Log(pandData.count);
        }
        else
        {

        }
    }
}
