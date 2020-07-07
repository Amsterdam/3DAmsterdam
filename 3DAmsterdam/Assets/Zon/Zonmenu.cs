using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Zonmenu : MonoBehaviour
{
    public Dropdown MaandDropdown;
    public Dropdown DagDropdown;
    public GameObject Lightsource;
    public Text tijdtekst;
    public Slider TijdSlider;

    private int dagnummer;
    private int maandnummer;
    private float tijd = 12;

    private List<int> dagenpermaand = new List<int>() { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
    // Start is called before the first frame update
    void Start()
    {
        UpdateMaand();
    }
    public void UpdateTijd()
    {
        float urenfloat = TijdSlider.value;
        int uren = Mathf.FloorToInt(urenfloat);
        int minuten = (int)((urenfloat - (float)uren) *60);
        tijdtekst.text = uren.ToString() + ":" + minuten.ToString();
        tijd = urenfloat;
        UpdateBelichting();
    }

    public void UpdateDag()
    {
        dagnummer = DagDropdown.value+1;
        SetDatum();
    }

    public void UpdateMaand()
    {
        int ingesteldeDag = DagDropdown.value;

        DagDropdown.ClearOptions();
        maandnummer = MaandDropdown.value;
        List<string> dagopties = new List<string>();
        for (int i = 0; i < dagenpermaand[maandnummer]; i++)
        {
            dagopties.Add((i + 1).ToString());
        }
        DagDropdown.AddOptions(dagopties);

        if (ingesteldeDag > dagopties.Count-1)
        {
            DagDropdown.value = dagopties.Count - 1;
        }
        else
        {
            DagDropdown.value = ingesteldeDag;
        }
        dagnummer = DagDropdown.value+1;
        SetDatum();
    }

    private void SetDatum()
    {
        int maandstartdag = 0;
        for (int i = 0; i < maandnummer ; i++)
        {
            maandstartdag += dagenpermaand[i];
        }
        dagnummer = maandstartdag + dagnummer;
        UpdateBelichting();

    }

    private void UpdateBelichting()
    {
        float Declinatie = Mathf.Sin(2 * Mathf.PI * (((float)dagnummer - 80) / 365.25f)) * 23.5f;
        //Debug.Log(Declinatie);

        float hoek = -1 * Mathf.Cos(2 * Mathf.PI * (tijd / 24)) * (90 - 52.373f) + Declinatie;
        Debug.Log(hoek);

        float richting = ((tijd / 24f) * 360f) + 180;
        Quaternion rot = Quaternion.Euler(hoek, richting, 0);
        Lightsource.transform.rotation = rot;

        if (hoek<0)
        {
            Lightsource.GetComponent<Light>().intensity = 0.2f;
        }
        else
        {
            Lightsource.GetComponent<Light>().intensity = 1f;
        }
    }
}
