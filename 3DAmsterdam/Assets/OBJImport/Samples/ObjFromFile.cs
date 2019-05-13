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

    void Start()
    {
        plaatsBlokje = GameObject.Find("PlaceBuilding");
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
                        loadedObject.tag = ("UploadedObj");
                        loadedObject.layer = LayerMask.NameToLayer("Default");

                        foreach (Transform child in loadedObject.transform)
                        {
                            child.gameObject.AddComponent<MeshCollider>();
                            child.gameObject.AddComponent<AddPijlenprefab>();
                            child.gameObject.AddComponent<HighLight>();
                        }

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
