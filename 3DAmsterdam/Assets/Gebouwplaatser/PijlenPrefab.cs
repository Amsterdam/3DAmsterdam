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

    private List<Vector3> rendererSizes;
    private Vector3 largestSize = Vector3.zero;

    private float scalingXZ, scalingY, scaleFactor = 2f;

    private void Start()
    {
        FindRenderer();
        Debug.Log("fdafds");
        pijlenPrefabMesh = (GameObject) Resources.Load("PijlenPrefab");
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0) && spawned == false)
        {
            pijlenPrefab = Instantiate(pijlenPrefabMesh, transform.position, Quaternion.identity);
            pijlenPrefab.transform.parent = gameObject.transform;
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
            scalingXZ = Mathf.Max(transform.localScale.x, transform.localScale.z);
            scalingY = pijlenPrefab.transform.localScale.y / scaleFactor;

            pijlenPrefab.transform.parent = null;
            pijlenPrefab.transform.localScale = new Vector3(scalingXZ, scalingY, scalingXZ) * scaleFactor;
            pijlenPrefab.transform.parent = gameObject.transform;
        }
    }

    private void FindRenderer()
    {
        if (transform.GetComponent<Renderer>() != null) rendererSizes.Add(transform.GetComponent<Renderer>().bounds.size);

        foreach(Transform child in transform)
        {
            Debug.Log(child);

            if (child.GetComponent<Renderer>() != null)
            {
                rendererSizes.Add(child.GetComponent<Renderer>().bounds.size);
            }
        }

        foreach(Vector3 size in rendererSizes)
        {
            Debug.Log(size);

            if (size.magnitude > largestSize.magnitude)
            {
                largestSize = size;
            }
        }
    }
}
