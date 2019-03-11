using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuExtender : MonoBehaviour
{
    [HideInInspector]
    public bool extend = false;
    [HideInInspector]
    public bool goBack = false;

    public float extensionDistance = 300f;
    private float extensionSpeed = 5f;

    void Update()
    {
        if (extend)
        {
            // menu beweegt vooruit
            transform.position += new Vector3(extensionSpeed, 0, 0);

            // menu stopt met bewegen op bepaald punt
            if (transform.position.x == extensionDistance)
            {
                extend = false;
            }
        }

        if (goBack)
        {
            // menu beweegt achteruit (terug)
            transform.position -= new Vector3(extensionSpeed, 0, 0);

            // menu stopt met bewegen wanneer die niet meer zichtbaar is
            if (transform.position.x == 0)
            {
                goBack = false;
            }
        }
    }
}
