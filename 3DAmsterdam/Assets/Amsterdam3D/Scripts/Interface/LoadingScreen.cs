using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private Text textMessage;
   
    public void ShowMessage(string text)
    {
        gameObject.SetActive(true);
        textMessage.text = text;
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
