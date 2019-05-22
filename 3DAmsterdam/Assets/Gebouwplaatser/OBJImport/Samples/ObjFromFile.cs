using Dummiesman;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using SimpleFileBrowser;

public class ObjFromFile : MonoBehaviour
{
    string objPath = string.Empty;
    string error = string.Empty;
    GameObject loadedObject;
    GameObject plaatsBlokje;
    Ray ray;
    RaycastHit hit;
    bool placeUpload = false;
    GameObject uploadGebouw;

    void Start()
    {
        plaatsBlokje = GameObject.Find("PlaceBuilding");
        uploadGebouw = GameObject.Find("Menus");
    }

    void Update()
    {
        PlaceUpload();
    }

    public void PlaceUpload()
    {
      ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (placeUpload)
        {
            if (Physics.Raycast(ray, out hit))
            {
                if (loadedObject != null)
                {
                    loadedObject.transform.position = hit.point;

                    if (Input.GetMouseButtonDown(0) && !(EventSystem.current.IsPointerOverGameObject()))
                    {
                        loadedObject.layer = LayerMask.NameToLayer("Default");
                        loadedObject.AddComponent<HighLight>();
                        loadedObject.AddComponent<PijlenPrefab>();
                        loadedObject.tag = "Sizeable";

                        foreach (Transform child in loadedObject.transform)
                        {
                            child.gameObject.AddComponent<MeshCollider>();
                            child.gameObject.AddComponent<HighLight>();
                            child.gameObject.tag = "Sizeable";
                        }

                        loadedObject = null;
                        placeUpload = false;
                    }
                }
            }
        }     
    }

    public void LoadUpload()
    {
        //file path
        if (!File.Exists(FileBrowser.Result))
        {
            error = "File doesn't exist.";
        }
        else
        {
            if (loadedObject != null)
                Destroy(loadedObject);
            
            loadedObject = new OBJLoader().Load(FileBrowser.Result);
            loadedObject.layer = LayerMask.NameToLayer("Ignore Raycast");
           
            placeUpload = true;

            error = string.Empty;
        }
    }
}
