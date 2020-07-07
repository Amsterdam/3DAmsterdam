using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenLink : MonoBehaviour
{
    public GameObject building;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckBuilding();
    }
    void CheckBuilding()

    {

        RaycastHit hit;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);



        if (Physics.Raycast(ray, out hit))

        {

            if (building == hit.transform.gameObject)

            {

                if (Input.GetMouseButtonDown(0))

                {

                    Application.ExternalEval("window.open(\"https://bldng360.nl/gebouwen/fgYLCkrU6Ee6TUnfTOsCjw\")");

                    //Application.OpenURL("https://bldng360.nl/gebouwen/fgYLCkrU6Ee6TUnfTOsCjw");

                }

            }

        }

    }
}
