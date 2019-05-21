using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectDownLoad : MonoBehaviour
{
    bool selecting;

    [HideInInspector]
    public GameObject selectedDownloadObj;

    Ray rayCast;
    RaycastHit hit;
    Material[] mats;
    List<Transform> children;

    public Button selectButton;
    public Button downloadButton;

    bool deselect;

    public void Start()
    {
        children = new List<Transform>();
        selecting = false;
        deselect = false;
        downloadButton.gameObject.SetActive(false);
    }

    public void Update()
    {
        SelectDownloadObj();

        if (deselect)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].color = Color.white;
            }

            children.Clear();
            selectedDownloadObj = null;
            downloadButton.gameObject.SetActive(false);
            selectButton.gameObject.SetActive(true);
            deselect = false;
        }
    }

    public void StartSelecting()
    {
        selecting = true;
    }

    public void SelectDownloadObj()
    {
        if (selecting)
        {
            rayCast = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(rayCast, out hit))
            {
                if(hit.transform.gameObject.tag == "Sizeable")
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                       selectButton.gameObject.SetActive(false);
                       downloadButton.gameObject.SetActive(true);
                       selectedDownloadObj = hit.transform.gameObject;

                       foreach(Transform child in hit.transform)
                        {
                            children.Add(child);
                            MaterialChange();
                        }
                    }
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if(hit.transform.gameObject != selectedDownloadObj && !(EventSystem.current.IsPointerOverGameObject()))
                    {
                        deselect = true;
                        selecting = false;
                    }
                }
            }
        }
    }

    public void MaterialChange()
    {
        for (int i = 0; i < children.Count; i++)
        {
            mats = children[i].gameObject.GetComponent<MeshRenderer>().materials;

            for (int j = 0; j < mats.Length; j++)
            {
                var originalColour = mats[j].color;
                mats[j].color = Color.yellow;
            } 
        }
    }
}
