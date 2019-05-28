using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class ObjUploader : MonoBehaviour
{
    public static string Key = "87ajdf898##@@jjKJA";
    public static string Server = "localhost:80/";
    public static string UploadUrl = Server + "customUpload.php";
    public static string DownloadUrl = Server + "customDownload.php?";

    [DllImport("__Internal")]
    private static extern void DownloadFile(string uri, string filename);

    public void StartUpload(string objData, string mtlData, Texture2D [] textures, Action<bool> onDone)
    {
        StartCoroutine(Upload(objData, mtlData, textures, onDone));
    }

    public IEnumerator Upload(string objData, string mtlData, Texture2D [] textures, Action<bool> onDone)
    {
        WWWForm form = new WWWForm();
        var guid = Guid.NewGuid().ToString();
        string randomName = "terrain_" + guid + ".zip";
        form.AddField("secret", Key);

        using (MemoryStream zipToOpen = new MemoryStream())
        {
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create, true))
            {
                archive.AddData(guid.ToString() + "_ObjFile.obj", objData);
                archive.AddData(guid.ToString() + "_MtlFile.mtl", mtlData);
                foreach (var tex in textures)
                {
                    if (!string.IsNullOrEmpty(tex.name) && tex.width > 0 && tex.height > 0  &&
                        tex.isReadable)
                    {
                        try
                        {
                            var bytes = tex.EncodeToPNG();
                            if (bytes != null && bytes.Length != 0)
                            {
                                archive.AddData(tex.name, bytes);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                }
            }
            //using (var fs = File.Open(Path.GetFileNameWithoutExtension(randomName) + ".zip", FileMode.Create))
            //{
            //    zipToOpen.Seek(0, SeekOrigin.Begin);
            //    zipToOpen.CopyTo(fs);
            //}
            byte[] buffer = zipToOpen.ToArray();
            form.AddBinaryData("file", buffer, randomName, "application/zip");
        }

        UnityWebRequest www = UnityWebRequest.Post(UploadUrl, form);
        yield return www.SendWebRequest();
        bool bSucces;

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            bSucces = false;
        }
        else
        {
            bSucces = true;
            Debug.Log("Form upload complete!");
            // Download it
            yield return new WaitForSeconds(2);// Wait 2 seconds for the file to be placed in the correct location
            string dlUrl = "window.open(" + DownloadUrl + "name="+ randomName + "&secret=" + Key + ")";
            Application.ExternalEval(dlUrl);
            //  Application.OpenURL(DownloadUrl + randomName);
            //  DownloadFile(DownloadUrl + randomName, randomName);
        }

        if (onDone != null) onDone(bSucces);
        Destroy(gameObject);
    }
}

public class TileSaver
{
    public static void SaveGameObjects(GameObject[] gos, Action<bool> onDone)
    {
        string objData = ObjExporter.GameObjectsToString(gos, true);
        string mtlData = ObjExporter.GameObjectsMaterialToString(gos);
        Texture2D[] textures = ObjExporter.GetUniqueTextures(gos);
        var saver = new GameObject("SaveTile");
        var sr = saver.AddComponent<ObjUploader>();
        sr.StartUpload(objData, mtlData, textures, onDone);
    }

    public static void SaveMeshFilters(MeshFilter [] mfs, Action<bool> onDone)
    {
        string objData = ObjExporter.MeshFiltersToString(mfs, true);
        string mtlData = ObjExporter.WriteMaterial(mfs);
        Texture2D[] textures = ObjExporter.GetUniqueTextures(mfs);
        var saver = new GameObject("SaveTile");
        var sr = saver.AddComponent<ObjUploader>();
        sr.StartUpload(objData, mtlData, textures, onDone);
    }
}
