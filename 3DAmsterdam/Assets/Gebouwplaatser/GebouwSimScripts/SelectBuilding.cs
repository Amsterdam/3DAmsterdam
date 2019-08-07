using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectBuilding : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;

    List<Transform> children;

    Material[] mats;
    Material[] parentMats;

    List<Color> originalColours;

    GameObject selectedObj;

    List<Color> originalParentColors;

    public API api;

    public void Start()
    {
        children = new List<Transform>();
        originalColours = new List<Color>();
        originalParentColors = new List<Color>();
    }

    public void FixedUpdate()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        SelectHoogbouw();
    }

    public void SelectHoogbouw()
    {
        if(Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Hoogbouw") && hit.transform.tag != "Sizeable" && hit.transform.tag != "CustomPlaced")
            {
                api.bagID = hit.transform.name;

                if (Input.GetMouseButtonDown(0))
                {
                    StartCoroutine(api.OnResponse(api.request));

                    if (selectedObj != null)
                    {
                        if (parentMats.Length != 0)
                        {
                            for (int i = 0; i < parentMats.Length; i++)
                            {
                                parentMats[i].color = originalParentColors[i];
                            }
                        }

                        for (int i = 0; i < children.Count; i++)
                        {
                            mats = children[i].gameObject.GetComponent<Renderer>().materials;

                            for (int j = 0; j < mats.Length; j++)
                            {
                                if (mats.Length > 0)
                                {
                                    mats[j].color = originalColours[j];
                                }
                            }
                        }
                    }

                    selectedObj = hit.transform.gameObject;

                    parentMats = hit.transform.gameObject.GetComponent<Renderer>().materials;

                    foreach (Transform child in hit.transform)
                    {
                        children.Add(child);
                    }

                    for (int i = 0; i < parentMats.Length; i++)
                    {
                        originalParentColors.Add(parentMats[i].color);
                        parentMats[i].color = Color.red;
                    }

                    for (int i = 0; i < children.Count; i++)
                    {
                        mats = children[i].gameObject.GetComponent<Renderer>().materials;

                        for (int j = 0; j < mats.Length; j++)
                        {
                            if (mats.Length > 0)
                            {
                                originalColours.Add(mats[j].color);
                                mats[j].color = Color.red;
                            }
                        }
                    }
                }              
            }

            if(hit.transform.gameObject != selectedObj)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if(parentMats.Length != 0)
                    {
                        for (int i = 0; i < parentMats.Length; i++)
                        {
                            parentMats[i].color = originalParentColors[i];
                        }
                    }
                  
                    for (int i = 0; i < children.Count; i++)
                    {
                        mats = children[i].gameObject.GetComponent<Renderer>().materials;

                        for (int j = 0; j < mats.Length; j++)
                        {
                            if (mats.Length > 0)
                            {
                                mats[j].color = originalColours[j];
                            }
                        }
                    }
                }        
            }
        }
    }
}
