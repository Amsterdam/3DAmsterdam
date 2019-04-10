using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyToDemos : MonoBehaviour
{
    public Camera cam;
    public GameObject RickGameOBject;
    public GameObject Bimlink;
    public GameObject bomen;
    public GameObject DirectionalLight;
    public GameObject Enviro;

    private bool bomenactief = false;
    private bool RickActief = false;
    private bool EnviroActief = false;
    private Vector3 positionRickDemo = new Vector3(561.8f, 100f, -80f);

    public void RickDemo()
    {
        Bimlink.SetActive(false);
        if (RickActief == false)
        {
            cam.transform.position = positionRickDemo;
            RickGameOBject.SetActive(true);
            RickActief = true;
        }
        else
        {
            RickGameOBject.SetActive(false);
            RickActief = false;
        }
        
    }

    public void DeKomPlein()
    {
        Vector3 Campositie = new Vector3(4335f, 225f, -6297f);
        cam.transform.position = Campositie;
        RickGameOBject.SetActive(false);
        Bimlink.SetActive(true);
    }

    public void Gevelfotos()
    {
        Vector3 Campositie = new Vector3(1788f, 536f, 509f);
        cam.transform.position = Campositie;
        RickGameOBject.SetActive(false);
        Bimlink.SetActive(false);
    }

    public void BomenToggle()
    {
        if (bomenactief == false)
        {
            bomen.SetActive(true);
            bomenactief = true;
        }
        else
        {
            bomen.SetActive(false);
            bomenactief = false;
        }
    }

    public void EnviroToggle()
    {
        if (EnviroActief == false)
        {
            Enviro.SetActive(true);
            DirectionalLight.SetActive(false);
            EnviroActief = true;
        }
        else
        {
            EnviroActief = false;
            Enviro.SetActive(false);
            DirectionalLight.SetActive(true);
        }
    }
}
