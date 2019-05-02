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
    public GameObject Pijlenprefab;
    public GameObject pijlenprefab;
    public ScaleObject scaleObject;

    bool placingObject;
    bool instantiate;
    bool highlight;
    bool getColour;

    private float arrowPositioningY = 4f;
    private float arrowScaling = 2f;

    public LayerMask rayLayer;

    Color originalColour;

    float scalingX, scalingY, scalingZ, distanceToObject;

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

        pijlenprefab.transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public void PlaceActivation()
    {
        if(selectedObject != null)
        {
            Destroy(pijlenprefab);

            selectedObject.transform.gameObject.GetComponent<Renderer>().material.color = Color.white;

            selectedObject.transform.gameObject.AddComponent<HighLight>();

            selectedObject = null;
        }

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

        if (Physics.Raycast(ray, out hit, 1000))
        {
            if (hit.transform.gameObject.tag == "Sizeable" && selectedObject == null)
            {
                if (Input.GetMouseButtonDown(0) && placingObject == false)
                {
                    originalColour = hit.transform.gameObject.GetComponent<Renderer>().material.color;

                    HighLight highlight = hit.transform.gameObject.GetComponent<HighLight>();

                    hit.transform.gameObject.GetComponent<Renderer>().material.color = Color.red;

                    Destroy(highlight);

                    selectedObject = hit.transform.gameObject;

                    scaleObject.hoogte.value = selectedObject.transform.localScale.y / 100;
                    scaleObject.breedte.value = selectedObject.transform.localScale.x / 100;
                    scaleObject.lengte.value = selectedObject.transform.localScale.z / 100;

                    scaleObject.rotatie.value = selectedObject.transform.rotation.y / 360;

                    //pijlenprefab = Instantiate(Resources.Load("Pijlenprefab/PijlenPrefab", typeof(GameObject))) as GameObject;
                    pijlenprefab = Instantiate<GameObject>(Pijlenprefab, hit.transform.position, hit.transform.rotation);

                    foreach (Transform child in pijlenprefab.transform)
                    {
                        if (child.GetComponent<MeshCollider>() == null) child.gameObject.AddComponent<MeshCollider>();

                        child.gameObject.AddComponent<ChangeColorByMouseOver>();
                        child.gameObject.AddComponent<ChangeColorBack>();
                    }

                    pijlenprefab.transform.parent = hit.transform;

                    // de afstand tussen het object en de grond wordt berekend.\
                    Vector3 positionUnderObject = new Vector3(hit.transform.position.x, 0, hit.transform.position.z);
                    distanceToObject = Vector3.Distance(positionUnderObject, hit.transform.position);

                    // de positie van de pijlen worden op de juiste positie neergezet. Deze afstand is de afstand tot het object
                    // plus de 1/4de van het object zelf.
                    Renderer renderer = hit.transform.gameObject.GetComponent<Renderer>();

                    pijlenprefab.transform.position = new Vector3(renderer.bounds.center.x, distanceToObject + (renderer.bounds.size.y
                                                                 / arrowPositioningY), renderer.bounds.center.z);

                    // de juiste scaling factoren worden berekend voor x, y en z.
                    scalingX = renderer.bounds.size.x / (hit.transform.localScale.x / 1f);
                    scalingY = renderer.bounds.size.y / (hit.transform.localScale.y / 1f);
                    scalingZ = renderer.bounds.size.z / (hit.transform.localScale.z / 1f);

                    // de scale van de pijlen wordt aangepast met een scaling factor.
                    pijlenprefab.transform.localScale = new Vector3(scalingX, scalingY, scalingZ) * arrowScaling;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (hit.transform.gameObject != selectedObject && !(EventSystem.current.IsPointerOverGameObject()))
                    {
                        if (hit.transform.tag != "PijlenPrefab")
                        {
                            Destroy(pijlenprefab);

                            selectedObject.transform.gameObject.GetComponent<Renderer>().material.color = Color.white;

                            selectedObject.transform.gameObject.AddComponent<HighLight>();

                            selectedObject = null;
                        }
                    }
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (hit.transform.gameObject != selectedObject && !(EventSystem.current.IsPointerOverGameObject()))
                {
                    if (hit.transform.tag != "PijlenPrefab")
                    {
                        Destroy(pijlenprefab);

                        selectedObject.transform.gameObject.GetComponent<Renderer>().material.color = Color.white;

                        selectedObject.transform.gameObject.AddComponent<HighLight>();

                        selectedObject = null;
                    }
                }
            }

        }
    }
}
