using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacePrefabs : MonoBehaviour
{
    private GameObject building, buildingIns;

    private bool placingObject = false, instantiate = false, highlight;

    private int clickCount = 0;

    public void PlaceActivation(GameObject _building)
    {
        building = _building;

        placingObject = true;
        instantiate = true;
        highlight = false;
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (placingObject)
        {
            if (Physics.Raycast(ray, out hit))
            {
                if (instantiate)
                {
                    buildingIns = (GameObject)Instantiate(building, hit.point, Quaternion.identity);
                    buildingIns.tag = "CustomPlaced";
                    instantiate = false;
                }

                buildingIns.transform.position = hit.point;

                if (Input.GetMouseButtonDown(0))
                {
                    buildingIns.gameObject.layer = 11;
                    placingObject = false;
                }
            }
        }
    }
}
