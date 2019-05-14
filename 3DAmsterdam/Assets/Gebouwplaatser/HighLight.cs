using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighLight : MonoBehaviour
{
    public Color originalColour;

    void Start()
    {
        foreach (Transform child in this.transform)
        {
            foreach(Material mat in child.transform)
            {
                originalColour = mat.color;
            }     
        }
        originalColour = gameObject.GetComponent<Renderer>().material.color;
    }

    void OnMouseOver()
    {
        foreach (Transform child in this.transform)
        {
            foreach(Material mat in child.transform)
            {
                mat.color =  Color.red;
            }
        }

        gameObject.GetComponent<Renderer>().material.color = Color.red;
    }

    void OnMouseExit()
    {
        foreach (Transform child in this.transform)
        {
            foreach(Material mat in child.transform)
            {
                mat.color = originalColour;
            }
        }
        gameObject.GetComponent<Renderer>().material.color = originalColour;  
    }
}
