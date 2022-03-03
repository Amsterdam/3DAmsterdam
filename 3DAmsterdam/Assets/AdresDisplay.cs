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
        var straat = new SaveableString(HTMLKeys.STREET_KEY);
        var huisnummer = new SaveableString(HTMLKeys.HOUSE_NUMBER_KEY);
        //var huisnummertoevoeging = ;
        var postcode = new SaveableString(HTMLKeys.ZIP_CODE_KEY);
        var plaats = new SaveableString(HTMLKeys.CITY_KEY);

        text.text = $"{straat.Value} {huisnummer.Value}\n{postcode.Value} {plaats.Value}";
    }
}
