using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropUpTemplate : MonoBehaviour
{
    public Text Text;
    Image image;

    private void Awake()
    {
        image = GetComponent<Image>(); 
    }

    public void Initialize(string text)
    {
        Text.text = text;        
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }

}
