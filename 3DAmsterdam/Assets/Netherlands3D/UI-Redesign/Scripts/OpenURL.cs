using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OpenURL : MonoBehaviour
{
    [SerializeField] private string URL;
    public void Open()
    {
        Debug.Log("Trying to open URL: " + URL);
        Application.OpenURL(URL);
    }
}
