using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InsDatumTijd : MonoBehaviour
{

    public GameObject TijdObject;

    private int Uren, Minuten;
    private int Jaren, Maanden, Dagen;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CheckVerschilTijd());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator CheckVerschilTijd()
    {
        for (; ; )
        {
            int DagenCheck = Dagen;
            int UrenCheck = Uren;
            int MinutenCheck = Minuten;

            Minuten = TijdObject.GetComponent<MenuFunctions>()._minutes;
            Uren = TijdObject.GetComponent<MenuFunctions>()._hours;
            Dagen = TijdObject.GetComponent<MenuFunctions>()._currentDay;
            Maanden = TijdObject.GetComponent<MenuFunctions>()._currentMonth;
            Jaren = TijdObject.GetComponent<MenuFunctions>()._currentYear;

            if (MinutenCheck != Minuten)
            {
                EnviroSkyMgr.instance.SetTime(EnviroSkyMgr.instance.Time.Years, EnviroSkyMgr.instance.Time.Days, EnviroSkyMgr.instance.Time.Hours, Minuten, EnviroSkyMgr.instance.Time.Seconds);
            }

            if (UrenCheck != Uren)
            {
                EnviroSkyMgr.instance.SetTime(EnviroSkyMgr.instance.Time.Years, EnviroSkyMgr.instance.Time.Days, Uren, EnviroSkyMgr.instance.Time.Minutes, EnviroSkyMgr.instance.Time.Seconds);
            }

            if (DagenCheck != Dagen)
            {
                string datumtekst = (Jaren) + "-" + (Maanden) + "-" + (Dagen);
                string urenminutenseconden = Uren + " " + Minuten + " " + 00;

                DateTime tijdstip;
                tijdstip = DateTime.ParseExact(datumtekst, "yyyy-M-d", null);

                EnviroSkyMgr.instance.SetTime(tijdstip);

                EnviroSkyMgr.instance.SetTime(EnviroSkyMgr.instance.Time.Years, EnviroSkyMgr.instance.Time.Days, Uren, Minuten, EnviroSkyMgr.instance.Time.Seconds);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }
}