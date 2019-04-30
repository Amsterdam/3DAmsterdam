using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScaleObject : MonoBehaviour
{

    public PlaatsBlokje plaatsBlokje;
    public Slider hoogte, breedte, lengte, rotatie;

    public void Start()
    {
        hoogte.value = 0.5f;
        breedte.value = 0.5f;
        lengte.value = 0.5f;
        rotatie.value = 0f;
    }

    public void ScaleHoogte()
    {
        plaatsBlokje.selectedObject.transform.localScale = new Vector3(plaatsBlokje.selectedObject.transform.localScale.x, hoogte.value * 100, plaatsBlokje.selectedObject.transform.localScale.z);
    }

    public void ScaleBreedte()
    {
        plaatsBlokje.selectedObject.transform.localScale = new Vector3(breedte.value * 100, plaatsBlokje.selectedObject.transform.localScale.y, plaatsBlokje.selectedObject.transform.localScale.z);
    }

    public void ScaleLengte()
    {
        plaatsBlokje.selectedObject.transform.localScale = new Vector3(plaatsBlokje.selectedObject.transform.localScale.x, plaatsBlokje.selectedObject.transform.localScale.y, lengte.value * 100);
    }

    public void RotateObject()
    {
        plaatsBlokje.selectedObject.transform.rotation = Quaternion.Euler(0, rotatie.value * 360, 0);
    }
}
