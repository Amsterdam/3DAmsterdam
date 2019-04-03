using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InsTijd : MonoBehaviour
{
    public GameObject TijdObject;

    private int Uren, Minuten;
    private int Jaren, Maanden, Dagen;


    IEnumerator CheckVerschillen()
    {
        int Nr = 0;

        for (; ; )
        {

            yield return new WaitForSeconds(0.1f);
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        Uren = 14;

    }

        // Update is called once per frame
    void Update()
    {
        Jaren = TijdObject.GetComponent<MenuFunctions>()._currentYear;
        Maanden = TijdObject.GetComponent<MenuFunctions>()._currentMonth;
        Dagen = TijdObject.GetComponent<MenuFunctions>()._currentDay;
        Uren = TijdObject.GetComponent<MenuFunctions>()._hours;
        Minuten = TijdObject.GetComponent<MenuFunctions>()._minutes;
    }




    public void updateUren()
    {
        EnviroSkyMgr.instance.SetTime(2018, 140, Uren, Minuten, 0);
    }


    public void updateDatum()
    {
        string datumtekst = (Jaren + 2018) + "-" + (Maanden + 1) + "-" + (Dagen + 1);
        DateTime tijdstip;
        tijdstip = DateTime.ParseExact(datumtekst, "yyyy-M-d", null);

        EnviroSkyMgr.instance.SetTime(tijdstip);
    }

    public void NuTijd(bool Nu)
    {
        if (Nu)
        {
            // zet het weer naar nu
            EnviroSkyMgr.instance.SetTime(System.DateTime.Now);

            // zet de tekst naar nu
            Jaren = System.DateTime.Now.Year;
            Maanden = System.DateTime.Now.Month;
            Dagen = System.DateTime.Now.Day;
            Uren = System.DateTime.Now.Hour;
            Minuten = System.DateTime.Now.Minute;
        }
    }
}


