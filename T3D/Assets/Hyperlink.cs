using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class Hyperlink : MonoBehaviour
{
    [SerializeField]
    private string url;

    private void OnEnable()
    {
        GetComponent<Button>().onClick.AddListener(OpenURL);
    }

    private void OnDisable()
    {
        GetComponent<Button>().onClick.RemoveAllListeners();
    }

    void OpenURL()
    {
        Application.OpenURL(url);
    }
}
