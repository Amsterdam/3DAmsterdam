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
    Ray ray;
    RaycastHit hit;
    bool placeUpload = false;

    void Update()
    {
        PlaceUpload();
    }

    //void OnGUI()
    //{
    //    objPath = GUI.TextField(new Rect(0, 0, 256, 32), objPath);

    //    GUI.Label(new Rect(0, 0, 256, 32), "Obj Path:");
    //    if (GUI.Button(new Rect(256, 32, 64, 32), "Load File"))
    //    {
    //        //file path
    //        if (!File.Exists(FileBrowser.Result))
    //        {
    //            error = "File doesn't exist.";

    //        }
    //        else
    //        {
    //            if (loadedObject != null)
    //                Destroy(loadedObject);

    //            loadedObject = new OBJLoader().Load(objPath);
    //            loadedObject.layer = LayerMask.NameToLayer("Ignore Raycast");

    //            placeUpload = true;

    //            error = string.Empty;
    //        }
    //    }

    //    if (!string.IsNullOrWhiteSpace(error))
    //    {
    //        GUI.color = Color.red;
    //        GUI.Box(new Rect(0, 64, 256 + 64, 32), error);
    //        GUI.color = Color.white;
    //    }
    //}

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
