using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class SelectionSaver
{
    public static void SaveGameObjects(GameObject[] gos, Action<bool> onDone)
    {
        List<MeshFilter> lfilters = new List<MeshFilter>();
        foreach (var go in gos)
        {
            var filters = go.GetComponentsInChildren<MeshFilter>();
            lfilters.AddRange(filters);
        }
        SaveMeshFilters(lfilters.ToArray(), onDone);
    }

    public static void SaveMeshFilters(MeshFilter [] mfs, Action<bool> onDone)
    {
        mfs = mfs.Distinct().ToArray();
        string filename = Guid.NewGuid().ToString();
        string objData = ObjExporter.WriteObjToString(filename + ".mtl", mfs, Matrix4x4.identity);
        string mtlData = MtlExporter.WriteMaterialToString(mfs).ToString();
        List<Texture2D> textures = MtlExporter.GetUniqueTextures(mfs);
        Uploader.StartUploadPackage(filename, objData, mtlData, textures.ToArray(), onDone);
    }
}
