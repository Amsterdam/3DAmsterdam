using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ColliderCheck : MonoBehaviour
{
    private List<Transform> rendererOptions;
    private Vector3 largestSize = Vector3.zero;
    private Transform rendChild;

    void Start()
    {
        rendererOptions = new List<Transform>();

        // als er al een collider op het object zit wordt die verplaatst met een box collider
        if (gameObject.GetComponent<Collider>() != null && gameObject.GetComponent<BoxCollider>() == null)
        {
            Destroy(gameObject.GetComponent<Collider>());
            gameObject.AddComponent<BoxCollider>();
        }

        // box collider wordt op object geplaatst
        if (gameObject.GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }

        ScaleCollider();
    }

    private void ScaleCollider()
    {
        // het object zelf wordt toegevoegd als optie voor grootste object
        rendererOptions.Add(this.gameObject.transform);

        if (transform.childCount != 0)
        {
            // alle children worden ook aan de list toegevoegd
            foreach (Transform child in transform)
            {
                rendererOptions.Add(child);
            }
        }

        foreach (Transform option in rendererOptions)
        {
            if (option.GetComponent<Renderer>() != null)
            {
                // de grootste renderer wordt uitgekozen
                if (option.GetComponent<Renderer>().bounds.size.magnitude > largestSize.magnitude)
                {
                    largestSize = option.GetComponent<Renderer>().bounds.size;
                    rendChild = option;
                }
            }
        }

        rendererOptions.Clear();

        // box collider wordt gebouwd gebaseerd op grootste renderer
        var boxCollider = rendChild.gameObject.AddComponent<BoxCollider>();
        Destroy(rendChild.gameObject.GetComponent<BoxCollider>());

        // box collider van object zelf wordt gelijk gezet aan collider van grootste renderer
        gameObject.GetComponent<BoxCollider>().size = boxCollider.size;
        gameObject.GetComponent<BoxCollider>().center = new Vector3(0f, boxCollider.center.y, 0f);
    }
}
