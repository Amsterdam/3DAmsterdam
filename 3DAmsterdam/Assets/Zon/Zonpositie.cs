using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zonpositie : MonoBehaviour
{
    public int dagnr = 180;
    public float tijd = 12f;
    private float lat = 5;
    private float lon = 52;

    private float Declinatie;
    private float hoek;
    public GameObject Lightsource;
    // Start is called before the first frame update

    private List<int> dagenpermaand = new List<int>() { 31,29,31,30,31,30,31,31,30,31,30,31 };
    

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       


    }
}
