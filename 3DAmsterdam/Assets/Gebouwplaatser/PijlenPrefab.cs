using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PijlenPrefab : MonoBehaviour
{
    private GameObject pijlenPrefab, pijlenPrefabMesh;
    private bool spawned = false;

    private string hitObject;
    private RaycastHit hit;
    private Ray ray;

    private List<Transform> rendererOptions;
    private Vector3 largestSize = Vector3.zero;
    private Transform rendChild;

    private float scalingXZ, scalingY, scaleFactor = 2f;

    private void Start()
    {
        rendererOptions = new List<Transform>();

        FindRenderer();
        AddCollider();

        pijlenPrefabMesh = (GameObject) Resources.Load("PijlenPrefab");
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0) && spawned == false)
        {
            pijlenPrefab = Instantiate(pijlenPrefabMesh, transform.position, Quaternion.identity);
            pijlenPrefab.transform.parent = gameObject.transform;

            var groundToObject = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), transform.position);

            pijlenPrefab.transform.position = new Vector3(pijlenPrefab.transform.position.x, groundToObject + (largestSize.y / 3),
                                                          pijlenPrefab.transform.position.z);
        }

        if (Input.GetMouseButtonUp(0))
        {
            spawned = true;
        }
    }

    private void Update()
    {
        DefineCollider();
        UpdateScale();

        if (Input.GetMouseButtonDown(0) && spawned)
        {
            if (hitObject != "Pijlenprefab")
            {
                Destroy(pijlenPrefab);

                spawned = false;
            }
        }
    }

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

    private void UpdateScale()
    {
        if (transform.hasChanged && pijlenPrefab != null)
        {
            if (transform.localScale != new Vector3(1, 1, 1))
            {
                scalingXZ = Mathf.Max(transform.localScale.x, transform.localScale.z);
                scalingY = pijlenPrefab.transform.localScale.y / scaleFactor;
            } else
            {
                scalingXZ = Mathf.Max(largestSize.x, largestSize.z);
                scalingY = pijlenPrefab.transform.localScale.y / scaleFactor;
            }

            pijlenPrefab.transform.parent = null;
            pijlenPrefab.transform.localScale = new Vector3(scalingXZ, scalingY, scalingXZ) * scaleFactor;
            pijlenPrefab.transform.parent = gameObject.transform;
        }
    }

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
                if (option.GetComponent<Renderer>().bounds.size.magnitude > largestSize.magnitude)
                {
                    largestSize = option.GetComponent<Renderer>().bounds.size;
                    rendChild = option;
                }
            }
        }
    }

    private void AddCollider()
    {
        if (gameObject.GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();

            var boxCollider = rendChild.gameObject.AddComponent<BoxCollider>();
            Destroy(rendChild.gameObject.GetComponent<BoxCollider>());

            gameObject.GetComponent<BoxCollider>().size = boxCollider.size;
            gameObject.GetComponent<BoxCollider>().center = new Vector3(0f, boxCollider.center.y, 0f);
        }
    }
}
