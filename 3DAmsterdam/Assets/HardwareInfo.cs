using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HardwareInfo : MonoBehaviour
{
    private string browserInfoKey;
    private SaveableString browserInfo;

    private void Awake()
    {
        browserInfoKey = GetType() + ".browserInfo";
        browserInfo = new SaveableString(browserInfoKey);
    }

    void Start()
    {
        GetComponent<InputField>().text = GetHardwareInfo();
    }

    string GetHardwareInfo()
    {
        return SystemInfo.operatingSystem
            + "\n" + browserInfo.Value;
    }
}
