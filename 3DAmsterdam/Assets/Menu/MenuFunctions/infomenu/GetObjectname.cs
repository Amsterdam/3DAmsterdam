using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using ConvertCoordinates;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;
/// <summary>
/// Zoekt BagIds op op basis van RDcoordinaten
/// </summary>
/// 



public class GetObjectname :MonoBehaviour
{
    public GameObject BagdataVeld;
    public GameObject GebouwenFolder;
    public MenuFunctions MenuFunctionsScript;
    public GameObject DataVenster;

    public bool IsCanvas()
    {
        PointerEventData cursor = new PointerEventData(EventSystem.current);                            // This section prepares a list for all objects hit with the raycast
        cursor.position = Input.mousePosition;
        List<RaycastResult> objectsHit = new List<RaycastResult>();
        EventSystem.current.RaycastAll(cursor, objectsHit);
        int count = objectsHit.Count;
        bool antwoord = false;
        if (count > 0) { antwoord = true; };
        return antwoord;

    }
    public void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (IsCanvas()) { return; };

            // meshcollider toevogen aan gebouwen
            foreach (MeshFilter mshfilter in GebouwenFolder.GetComponentsInChildren<MeshFilter>())
            {
                if (mshfilter.gameObject.GetComponent<MeshCollider>()==null)
                {
                    mshfilter.gameObject.AddComponent<MeshCollider>().sharedMesh = mshfilter.sharedMesh;
                }
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit) == false) //hit nothing
            {
                return;
            }
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("UI"))    //UI geraakt
            {
                return;
            }

            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Panden"))    //pand geraakt
            {

                DataVenster.SetActive(true);
                string bagid = GetObjectnameopMeshData(hit);

                    //BagdataVeld.SetActive(true);
                    BagdataVeld.GetComponent<GetBagData>().Begin(bagid);

                setHighlight(bagid);
            }
        }

    }

    public string GetObjectnameopMeshData(RaycastHit hit)
    {
        string returnstring = "";
        if (hit.transform.GetComponent<ObjectMapping>() == null)
        {
            return "";
        }

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)//niets highlighten als raycasthit niet op mesh
        {

            return "";
        }
        //if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Panden"))
        //{

        //    return "";
        //}


        Vector2[] uvs = meshCollider.sharedMesh.uv2;
        float uvx = uvs[meshCollider.sharedMesh.triangles[hit.triangleIndex * 3]].x;

        Dictionary<float, string> bagidlijst = hit.transform.gameObject.GetComponent<ObjectMapping>().Objectenlijst;
        string bagid = "";
        foreach (KeyValuePair<float, string> pair in bagidlijst)
        {
            if (pair.Key == uvx)
            {
                bagid = pair.Value;
                returnstring = bagid;
                Debug.Log(returnstring);
                return returnstring;
            }
        }
        Debug.Log("geen bagid gevonden ");
        return "";
    }

    void setHighlight(string bagid)
    {

        List<string> HighLightPanden = new List<string>(0);
        if (bagid != "")
        {
            HighLightPanden.Add(bagid);
        }
        foreach (ObjectMapping h in GebouwenFolder.GetComponentsInChildren<ObjectMapping>())
        {
            h.SetHighlight(HighLightPanden);
        }
    }

    public void CloseView()
    {
        DataVenster.SetActive(false);
        List<string> HighLightPanden = new List<string>();
        HighLightPanden.Add("0");
        foreach (ObjectMapping h in GebouwenFolder.GetComponentsInChildren<ObjectMapping>())
        {
            h.SetHighlight(HighLightPanden);
        }
    }
    void OnDisable()
    {
        
    }



}
