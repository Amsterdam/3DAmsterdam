using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class PlacePrefabs : MonoBehaviour
{
    public string AssetbundlesUrl = "file:///D://Github/WebGL/Meubilair/";
    private GameObject building, buildingIns;

    private bool placingObject = false, instantiate = false;


    public void PlaceActivation(GameObject _building)
    {
        building = _building;

        placingObject = true;
        instantiate = true;
    }

    public void PlaatsGebouw(string Objectnaam)
    {

        string pad = AssetbundlesUrl + Objectnaam;
        StartCoroutine(LoadAssetBundle(pad));
    }

    IEnumerator LoadAssetBundle(string pad)
    {
        
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(pad))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log("kan niet bij de assetbundles");
            }
            else
            {
                // Get downloaded asset bundle
                AssetBundle myLoadedAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                GameObject[] prefabs = myLoadedAssetBundle.LoadAllAssets<GameObject>();
                GameObject prefab = prefabs[0];
                var parts = pad.Split('/');
                prefab.name = parts[parts.Length - 1];
                //Instantiate(prefab);
                MeshCollider[] mcs = prefab.GetComponentsInChildren<MeshCollider>();
                foreach (MeshCollider mc in mcs)
                {
                    mc.enabled = false;
                }


                PlaceActivation(prefab);
                myLoadedAssetBundle.Unload(false);

            }
        }

        yield return null;
    }

    public void PlaatsMeubilair(string Objectnaam)
    {
        string pad = AssetbundlesUrl + Objectnaam;
        StartCoroutine(LoadAssetBundle(pad));
    }


    void Update()
    {
        if (placingObject)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (instantiate)
                {
                    buildingIns = (GameObject)Instantiate(building, hit.point, Quaternion.identity);

                    buildingIns.tag = "CustomPlaced";
                    buildingIns.gameObject.layer = 2;
                    instantiate = false;

                    
                }

                if (Input.GetMouseButtonDown(0))
                {
                    //Debug.Log("LINKER MUISKNOP");

                   
                    buildingIns.gameObject.layer = 11;
                    placingObject = false;

                    MeshCollider[] mcs = buildingIns.GetComponentsInChildren<MeshCollider>();
                    foreach (MeshCollider mc in mcs)
                    {
                        mc.enabled = true;
                    }
                    buildingIns.AddComponent<ColliderCheck>();
                }


                if (Input.GetMouseButtonDown(1))
                {
                    Debug.Log("RECHTER MUISKNOP");
                    placingObject = false;
                    Destroy(buildingIns);

                    return;
                }

                buildingIns.transform.position = hit.point;
            }
        }
    }
}
