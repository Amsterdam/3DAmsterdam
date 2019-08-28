using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TekstVeldScript : MonoBehaviour
{
    public API api;
    public TextMeshProUGUI naam;
    public TextMeshProUGUI bouwjaar;
    public TextMeshProUGUI BAGID;
    public TMP_Dropdown dropDown;
    public TextMeshProUGUI label;

    void Start()
    {
        api.naam = naam;
        api.bouwjaar = bouwjaar;
        api.BAGID = BAGID;
        api.dropDown = dropDown;
        api.verblijfsobjecten = label;
    }
}
