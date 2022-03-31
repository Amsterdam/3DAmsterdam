using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdresDisplay : MonoBehaviour
{
    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    private void OnEnable()
    {
        LoadAddress();
    }

    private void LoadAddress()
    {
        var straat = T3DInit.HTMLData.Street;// new SaveableString(HTMLKeys.STREET_KEY);
        var huisnummer = T3DInit.HTMLData.HouseNumber;
        var huisnummerToevoeging = T3DInit.HTMLData.HouseNumberAddition;
        var postcode = T3DInit.HTMLData.ZipCode;
        var plaats = T3DInit.HTMLData.City;

        text.text = $"{straat} {huisnummer}{huisnummerToevoeging}\n{postcode} {plaats}";
    }
}