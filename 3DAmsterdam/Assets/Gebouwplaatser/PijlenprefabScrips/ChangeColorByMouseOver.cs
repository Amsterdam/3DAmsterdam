using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColorByMouseOver : MonoBehaviour {

    Color myColor = new Color(0f, 0f, 1f, 1f);

    private Color32 originalColor;
    private byte orgR;
    private byte orgG;
    private byte orgB;
    private byte orgA;

    void Start()
    {
        originalColor = GetComponent<Renderer>().material.color;
        orgR = originalColor.r;
        orgG = originalColor.g;
        orgB = originalColor.b;
        orgA = originalColor.a;
    }

    private void OnMouseOver()
    {
        GetComponent<Renderer>().material.color = Color.yellow;
    }

    private void OnMouseExit()
    {
        GetComponent<Renderer>().material.color = new Color32(orgR, orgG, orgB, orgA);
    }
}
