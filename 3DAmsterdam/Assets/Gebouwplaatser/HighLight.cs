using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighLight : MonoBehaviour
{
    public Color originalColour;

    void Start()
    {
        originalColour = gameObject.GetComponent<Renderer>().material.color;
    }

    void OnMouseOver()
    {
        gameObject.GetComponent<Renderer>().material.color = Color.red;            
    }

    void OnMouseExit()
    {
        gameObject.GetComponent<Renderer>().material.color = originalColour;  
    }
}
