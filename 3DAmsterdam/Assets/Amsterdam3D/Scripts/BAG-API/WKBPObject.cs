using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class WKBPObject : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint = default;
    [SerializeField] public GameObject wkbpParent = default;
    [SerializeField] private GameObject wkbpPrefab = default;
    [SerializeField] public Toggle wkbpToggle = default;
    [SerializeField] private Scrollbar slider = default;
    private List<WKBPInstance> WKBPList = new List<WKBPInstance>();

    private void Start()
    {
        wkbpParent.SetActive(false);
        wkbpToggle.gameObject.SetActive(false);
    }
    public IEnumerator LoadWKBP(Pand.Rootobject thisPand, int adresIndex)
    {
        if(WKBPList.Count > 0)
        {
            foreach(WKBPInstance inst in WKBPList)
            {
                Destroy(inst.gameObject);
            }
            WKBPList.Clear(); // clears the old wkbp list
        }
        yield return StartCoroutine(ImportWKBP.Instance.CallWKBP(thisPand.results[adresIndex]));
        foreach (WKBP.Result result in thisPand.results[adresIndex].verblijfsobject.wkbpBeperkingen.results)
        {
            // adds new wkbp elements
            GameObject tempBeperkingObj = Instantiate(wkbpPrefab, spawnPoint.position, spawnPoint.rotation);
            tempBeperkingObj.transform.SetParent(spawnPoint);
            WKBPInstance tempWKBP = tempBeperkingObj.GetComponent<WKBPInstance>();
            WKBPList.Add(tempWKBP);
            tempWKBP.Initialize(result);
        }
        wkbpParent.SetActive(true);
        wkbpToggle.gameObject.SetActive(true);
        slider.value = 1f; // reset slider
        wkbpToggle.isOn = true;
        DisplayBAGData.Instance.loadingCirle.SetActive(false); // loading bar
    }
}
