using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PijlenPrefab : MonoBehaviour
{
    private GameObject pijlenPrefab;

    private GameObject pijlenPrefabMesh;
    private bool spawned = false, nextSpawn;

    private string hitObject;
    private RaycastHit hit;
    private Ray ray;

    private List<Transform> rendererOptions;
    private Vector3 largestSize = Vector3.zero;
    private Transform rendChild;

    private float scalingXZ, scaleFactor = 2f;

    private void Start()
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

        FindRenderer();
        ChangeCollider();

        // de prefab wordt vanuit de "Resources" folder ingeladen
        pijlenPrefabMesh = (GameObject) Resources.Load("PijlenPrefab");
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0) && spawned == false)
        {
            FindRenderer();
            ChangeCollider();

            // de prefab wordt geinstantieerd op het moment dat er op het object geklikt wordt
            pijlenPrefab = Instantiate(pijlenPrefabMesh, transform.position, Quaternion.identity);
            pijlenPrefab.transform.parent = gameObject.transform;

            // berekent afstand tussen de grond en object
            var groundToObject = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), transform.position);

            // prefab wordt op positie geplaatst gebaseerd op grootte/positie van object
            pijlenPrefab.transform.position = new Vector3(pijlenPrefab.transform.position.x, groundToObject + (largestSize.y / 3),
                                                          pijlenPrefab.transform.position.z);

            nextSpawn = true;
        }

        if (Input.GetMouseButtonUp(0) && nextSpawn)
        {
            spawned = true;
            nextSpawn = false;
        }
    }

    private void Update()
    {
        DefineCollider();
        UpdateScale();

        // als er niet op de prefab wordt geklikt wordt die verwijdert
        if (Input.GetMouseButtonDown(0) && spawned)
        {
            if (hitObject != "Pijlenprefab")
            {
                Destroy(pijlenPrefab);

                spawned = false;
            }
        }

        // alleen het object zelf wordt geroteerd
        if (pijlenPrefab != null)
        {
            pijlenPrefab.transform.eulerAngles = Vector3.zero;
        }
    }

    // er wordt bepaald wat de hitbox van de prefab zelf is, zodat bepaald kan worden wanneer er wel/niet op wordt geklikt
    private void DefineCollider()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            hitObject = hit.collider.name;
        }

        if (hitObject == "Afvalbak" || hitObject == "X" || hitObject == "Z" || hitObject == "Ry" || hitObject == "XZ")
        {
            hitObject = "Pijlenprefab";
        }
    }

    // de prefab wordt mee verschaald met de grootte van het object waar het omheen zit
    private void UpdateScale()
    {
        if (transform.hasChanged && pijlenPrefab != null)
        {
            scalingXZ = Mathf.Max(largestSize.x, largestSize.z);

            pijlenPrefab.transform.parent = null;
            pijlenPrefab.transform.localScale = new Vector3(scalingXZ * scaleFactor, 1f, scalingXZ * scaleFactor);
            pijlenPrefab.transform.parent = gameObject.transform;
        }
    }

    // de grootste renderer van een gameobject wordt uitgezocht omdat objecten veel children kunnen bevatten
    private void FindRenderer()
    {
        rendererOptions.Add(this.gameObject.transform);

        if (transform.childCount != 0)
        {
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
    }

    // gebaseerd op de grootste renderer wordt er een boxcollider toegevoegd aan het hoofdobject
    private void ChangeCollider()
    {
        var boxCollider = rendChild.gameObject.AddComponent<BoxCollider>();
        Destroy(rendChild.gameObject.GetComponent<BoxCollider>());

        gameObject.GetComponent<BoxCollider>().size = boxCollider.size;
        gameObject.GetComponent<BoxCollider>().center = new Vector3(0f, boxCollider.center.y, 0f);      
    }
}
