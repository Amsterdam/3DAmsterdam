using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class WKBPObject : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint = default;
    [SerializeField] private GameObject wkbpParent = default;
    [SerializeField] private GameObject wkbpPrefab = default;
    [SerializeField] private Toggle wkbpToggle = default;
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
            WKBPList.Clear();
        }
        yield return StartCoroutine(ImportWKBP.Instance.CallWKBP(thisPand.results[adresIndex]));
        foreach (WKBP.Result result in thisPand.results[adresIndex].verblijfsobject.wkbpBeperkingen.results)
        {
            GameObject tempBeperkingObj = Instantiate(wkbpPrefab, spawnPoint.position, spawnPoint.rotation);
            tempBeperkingObj.transform.SetParent(spawnPoint);
            WKBPInstance tempWKBP = tempBeperkingObj.GetComponent<WKBPInstance>();
            WKBPList.Add(tempWKBP);
            tempWKBP.Initialize(result);
        }
        wkbpParent.SetActive(true);
        wkbpToggle.gameObject.SetActive(true);
        wkbpToggle.isOn = true;
    }
}
