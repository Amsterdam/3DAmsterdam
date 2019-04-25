using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaatsBlokje : MonoBehaviour
{
    public GameObject blokje;
    GameObject tempGebouw;
    GameObject gebouw;
    public GameObject selectedObject;

    bool placingObject;
    bool instantiate;
    bool highlight;
    bool getColour;

    public LayerMask rayLayer;

    Color originalColour;

    int clickCount;

    // Start is called before the first frame update
    void Start()
    {
        placingObject = false;
        instantiate = false;
        getColour = false;
        rayLayer = 1 << 9;
        clickCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        PlaceObject();
        SelectAndTransform();
    }

    public void PlaceActivation()
    {
        placingObject = true;
        instantiate = true;
        highlight = false;
    }

    public void PlaceObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (placingObject)
        {
            if(Physics.Raycast(ray, out hit))
            {
                if (instantiate)
                {
                    tempGebouw = (GameObject)Instantiate(blokje, hit.point, Quaternion.identity);
                    instantiate = false;
                }

                tempGebouw.transform.position = hit.point;

                if (Input.GetMouseButtonDown(0))
                {
                    tempGebouw.transform.position = hit.point;
                    tempGebouw.gameObject.layer = 11;
                    placingObject = false;
                }
            }
        }
    }

    public void SelectAndTransform()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 1000))
        {
            if(hit.transform.gameObject.tag == "Sizeable")
            {
                if (Input.GetMouseButtonDown(0) && placingObject == false)
                {
                    originalColour = hit.transform.gameObject.GetComponent<Renderer>().material.color;

                    HighLight highlight = hit.transform.gameObject.GetComponent<HighLight>();

                    hit.transform.gameObject.GetComponent<Renderer>().material.color = Color.red;         

                    Destroy(highlight);

                    selectedObject = hit.transform.gameObject;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (hit.transform.gameObject != selectedObject && !(EventSystem.current.IsPointerOverGameObject()))
                {
                    selectedObject.transform.gameObject.GetComponent<Renderer>().material.color = originalColour;

                    selectedObject.transform.gameObject.AddComponent<HighLight>();

                    selectedObject = null;
                }
            }
        }
    }
}
