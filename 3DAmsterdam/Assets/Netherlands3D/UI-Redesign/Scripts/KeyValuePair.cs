using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class KeyValuePair : MonoBehaviour
{
    [SerializeField] private string key;
    [SerializeField] private string value;

    [SerializeField] private TMP_Text keyObject;
    [SerializeField] private TMP_Text valueObject;

    public void SetKey(string text)
    {
        key = text;
    }
    public void SetValue(string text)
    {
        value = text;
    }

    private void OnValidate()
    {
        keyObject.text = key;
        valueObject.text = value;
    }

}
