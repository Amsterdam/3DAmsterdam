using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Export : MonoBehaviour
{
    public GameObject selectDownload;
    SelectDownLoad _selectDownload;

    void Start()
    {
        _selectDownload = selectDownload.GetComponent<SelectDownLoad>();
    }

    public void ExportDownload()
    {
        if(_selectDownload.selectedDownloadObj != null)
        {
            MeshFilter mesh1;

            mesh1 = _selectDownload.selectedDownloadObj.GetComponentInChildren<MeshFilter>();

            ObjExporter.MeshToFile(mesh1, "C:/Users/Rick/Desktop/OBjFile/" + _selectDownload.selectedDownloadObj.name + ".obj");
        }  
    }
}
