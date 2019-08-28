using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlaatsBlokje : MonoBehaviour
{
    public GameObject blokje;
    private GameObject tempGebouw;

    private bool placingObject, instantiate;

    private int cubeNaming = 1;

    void Start()
    {
        placingObject = false;
        instantiate = false;
    }

    void Update()
    {
        PlaceObject();
    }

    private void PlaceObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (placingObject)
        {
            if (Physics.Raycast(ray, out hit))
            {
                if (instantiate)
                {
                    // houdt het blokje vast aan de muis
                    tempGebouw = (GameObject)Instantiate(blokje, hit.point, Quaternion.identity);
                    instantiate = false;
                }

                // positie van blokje gaat mee met die van de muis
                tempGebouw.transform.position = new Vector3(0, tempGebouw.transform.localScale.y / 2, 0) + hit.point;

                // blokje krijgt schaal en rotatie die gegeven worden in het menu
                tempGebouw.transform.localScale = new Vector3(50f, 50f, 50f);
                //tempGebouw.transform.rotation = Quaternion.Euler(0, rotatie.value * 360, 0);

                // als er geklikt wordt, dan wordt het blokje geplaatst
                if (Input.GetMouseButtonDown(0))
                {
                    tempGebouw.AddComponent<ColliderCheck>();
                    tempGebouw.AddComponent<PositioningBlokje>();
                    tempGebouw.gameObject.layer = 11;
                    tempGebouw.gameObject.name = "Cube" + cubeNaming;

                    cubeNaming++;

                    placingObject = false;
                }
            }
        }
    }

    public void PlaceActivation()
    {
        placingObject = true;
        instantiate = true;
    }     
}
