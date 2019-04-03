using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grounddetection : MonoBehaviour
{
    bool floating = false;
    Vector3 positie = new Vector3();
    Vector3 rayvanaf;
    private void Start()
    {
        rayvanaf = transform.position;
    }


    // Update is called once per frame
    void Update()
    {
        Debug.Log(transform.name);

        RaycastHit hit;
        
        rayvanaf.y -= 50;
        Debug.Log(transform.position.y);
        Debug.Log(rayvanaf.y);
        Physics.Raycast(rayvanaf, transform.TransformDirection(Vector3.up), out hit, Mathf.Infinity);
        Debug.Log(hit.transform.name);
        if (hit.transform.name !="FPSController")
            
        {

            if (floating)
            {
                GetComponent<Rigidbody>().useGravity = true;
            }
            floating = false;
        }
        else
        {
            Debug.Log("wel floating");
            if (floating == false)
            {
                GetComponent<Rigidbody>().useGravity = false;
                positie = transform.position;
                positie.y += 3;
                floating = true;
                
            }
            transform.position = positie;

        }
    }
}
