using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ApplicationVersion : MonoBehaviour
{
    void Start()
    {
        GetComponent<TextMeshProUGUI>().text = Application.version;
    }
}
