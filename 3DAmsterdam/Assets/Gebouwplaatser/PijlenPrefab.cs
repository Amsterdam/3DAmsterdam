using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PijlenPrefab : MonoBehaviour
{
    private ScaleUploads scaleScript;

    [HideInInspector]
    public bool activate = false;

    private List<Transform> rendererOptions;
    private Vector3 largestSize = Vector3.zero;
    private Transform rendChild;


    void Start()
    {
        //scaleScript = GameObject.Find("Manager").GetComponent<ScaleUploads>();
        //scaleScript.gameObjects.Add(this.gameObject);

        //rendererOptions = new List<Transform>();

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
    }

    //private void Update()
    //{
    //    // menus verdwijnen als er ergens anders geklikt wordt
    //    if (Input.GetMouseButtonDown(0) && activate && !EventSystem.current.IsPointerOverGameObject())
    //    {
    //        activate = false;
    //    }
    //}

    //private void OnMouseOver()
    //{
    //    // selecteert het juiste object
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        scaleScript.selectedObject = this.gameObject;
    //    }

    //    // navigatie/verschaal menu komt tevoorschijn
    //    if (Input.GetMouseButtonUp(0) && activate == false)
    //    {
    //        activate = true;
    //    }
    //}





    //private GameObject pijlenPrefab, pijlenPrefabMesh;
    //private ScaleUploads scaleScript;

    //private bool spawned = false, nextSpawn;
    //[HideInInspector]
    //public bool scaling, setScaleValues;

    //private string hitObject;
    //private RaycastHit hit;
    //private Ray ray;

    //private float scalingXZ, scaleFactor = 2f;

    //private void Start()
    //{
    //    rendererOptions = new List<Transform>();

    //    // als er al een collider op het object zit wordt die verplaatst met een box collider
    //    if (gameObject.GetComponent<Collider>() != null && gameObject.GetComponent<BoxCollider>() == null)
    //    {
    //         Destroy(gameObject.GetComponent<Collider>());
    //         gameObject.AddComponent<BoxCollider>();
    //    }

    //    // box collider wordt op object geplaatst
    //    if (gameObject.GetComponent<Collider>() == null)
    //    {
    //        gameObject.AddComponent<BoxCollider>();
    //    }

    //    // de prefab wordt vanuit de "Resources" folder ingeladen
    //    pijlenPrefabMesh = (GameObject) Resources.Load("PijlenPrefab");

    //    scaleScript = GameObject.Find("Manager").GetComponent<ScaleUploads>();
    //    scaleScript.gameObjects.Add(this.gameObject);
    //}

    //private void OnMouseOver()
    //{
    //    if (Input.GetMouseButtonDown(0) && spawned == false)
    //    {
    //        // de prefab wordt geinstantieerd op het moment dat er op het object geklikt wordt
    //        pijlenPrefab = Instantiate(pijlenPrefabMesh, transform.position, Quaternion.Euler(0, 0, 0));
    //        pijlenPrefab.transform.parent = gameObject.transform;

    //        // berekent afstand tussen de grond en object
    //        var groundToObject = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), transform.position);

    //        // prefab wordt op positie geplaatst gebaseerd op grootte/positie van object
    //        pijlenPrefab.transform.position = new Vector3(pijlenPrefab.transform.position.x, groundToObject + (largestSize.y / 3),
    //                                                      pijlenPrefab.transform.position.z);

    //        scaling = true;
    //        setScaleValues = true;
    //        nextSpawn = true;

    //        Invoke("SetScaleInitilialization", 1f);
    //    }

    //    if (Input.GetMouseButtonUp(0) && nextSpawn)
    //    {
    //        spawned = true;
    //        nextSpawn = false;
    //    }
    //}

    //private void Update()
    //{
    //    FindRenderer();
    //    ChangeCollider();
    //    UpdateScale();
    //    DefineHitBox();

    //    // als er niet op de prefab wordt geklikt wordt die verwijdert
    //    if (Input.GetMouseButtonDown(0) && spawned)
    //    {
    //        if (hitObject != "Pijlenprefab" && !EventSystem.current.IsPointerOverGameObject())
    //        { 
    //            Destroy(pijlenPrefab);

    //            spawned = false;
    //            scaling = false;
    //        }
    //    }
    //}

    //// er wordt bepaald wat de hitbox van de prefab zelf is, zodat bepaald kan worden wanneer er wel/niet op wordt geklikt
    //private void DefineHitBox()
    //{
    //    ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    //    if (Physics.Raycast(ray, out hit))
    //    {
    //        hitObject = hit.collider.name;
    //    }

    //    if (hitObject == "Afvalbak" || hitObject == "X" || hitObject == "Z" || hitObject == "Ry" || hitObject == "XZ")
    //    {
    //        hitObject = "Pijlenprefab";
    //    }
    //}

    //// de prefab wordt mee verschaald met de grootte van het object waar het omheen zit
    //private void UpdateScale()
    //{
    //    if (pijlenPrefab != null)
    //    {
    //        scalingXZ = Mathf.Max(largestSize.x, largestSize.z);

    //        pijlenPrefab.transform.parent = null;
    //        pijlenPrefab.transform.localScale = new Vector3(scalingXZ * scaleFactor, 3f, scalingXZ * scaleFactor);
    //        pijlenPrefab.transform.parent = gameObject.transform;
    //    }
    //}

    // de grootste renderer van een gameobject wordt uitgezocht omdat objecten veel children kunnen bevatten
    //private void FindRenderer()
    //{
    //    rendererOptions.Add(this.gameObject.transform);

    //    if (transform.childCount != 0)
    //    {
    //        foreach (Transform child in transform)
    //        {
    //            rendererOptions.Add(child);
    //        }
    //    }

    //    foreach (Transform option in rendererOptions)
    //    {
    //        if (option.GetComponent<Renderer>() != null)
    //        {
    //            // de grootste renderer wordt uitgekozen
    //            if (option.GetComponent<Renderer>().bounds.size.magnitude > largestSize.magnitude)
    //            {
    //                largestSize = option.GetComponent<Renderer>().bounds.size;
    //                rendChild = option;
    //            }
    //        }
    //    }

    //    rendererOptions.Clear();
    //}

    // gebaseerd op de grootste renderer wordt er een boxcollider toegevoegd aan het hoofdobject
    //private void ChangeCollider()
    //{
    //    var boxCollider = rendChild.gameObject.AddComponent<BoxCollider>();
    //    Destroy(rendChild.gameObject.GetComponent<BoxCollider>());

    //    gameObject.GetComponent<BoxCollider>().size = boxCollider.size;
    //    gameObject.GetComponent<BoxCollider>().center = new Vector3(0f, boxCollider.center.y, 0f);
    //}

    //private void SetScaleInitilialization()
    //{
    //    setScaleValues = false;
    //}
}
