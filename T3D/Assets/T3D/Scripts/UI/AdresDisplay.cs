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
        var straat = ServiceLocator.GetService<T3DInit>().HTMLData.Street;// new SaveableString(HTMLKeys.STREET_KEY);
        var huisnummer = ServiceLocator.GetService<T3DInit>().HTMLData.HouseNumber;
        var huisnummerToevoeging = ServiceLocator.GetService<T3DInit>().HTMLData.HouseNumberAddition;
        var postcode = ServiceLocator.GetService<T3DInit>().HTMLData.ZipCode;
        var plaats = ServiceLocator.GetService<T3DInit>().HTMLData.City;

        text.text = $"{straat} {huisnummer}{huisnummerToevoeging}\n{postcode} {plaats}";
    }
}
