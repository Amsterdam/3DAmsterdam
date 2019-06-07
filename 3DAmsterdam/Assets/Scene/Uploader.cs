using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Uploader : MonoBehaviour
{
    public static string Key = "87ajdf898##@@jjKJA";
    public static string Server = "localhost:80/";
    public static string UploadUrl = Server + "customUpload.php";
    public static string DownloadUrl = Server + "customDownload.php?";

    [DllImport("__Internal")]
    private static extern void DownloadFile(string uri, string filename);

    static Uploader NewUploader()
    {
        GameObject go = new GameObject("Uploader");
        return go.AddComponent<Uploader>();
    }

    public static void StartUploadPackage(string objData, string mtlData, Texture2D[] textures, Action<bool> onDone)
    {
        var u = NewUploader();
        u.StartCoroutine(u.UploadPackage(objData, mtlData, textures, onDone));
    }

    public static void StartUploadScene(string id, string json, Action<bool> onDone)
    {
        var fn = string.Join("", id, ".json");
        var u = NewUploader();
        u.StartCoroutine(u.UploadString(fn, json, onDone));
    }

    public static void StartUploadObj(string id, string objData, Action<bool> onDone)
    {
        var fn = string.Join("", id, ".obj");
        var u = NewUploader();
        u.StartCoroutine(u.UploadString(fn, objData, onDone));
    }

    public static void StartUploadMtl(string id, string mtlData, Action<bool> onDone)
    {
        var fn = string.Join("", id, ".mtl");
        var u = NewUploader();
        u.StartCoroutine(u.UploadString(fn, mtlData, onDone));
    }

    public static void StartUploadTextures(Texture2D[] textures, Action<bool> onDone)
    {
        foreach (var tex in textures)
        {
            var fn = string.Join("", tex.imageContentsHash, ".png");
            var u = NewUploader();
            u.StartCoroutine(u.UploadTexture(fn, tex, onDone));
        }
    }

    public static void StartDownloadScene(string id, Action<string, bool> onDone)
    {
        var fn = string.Join("", id, ".json");
        var u = NewUploader();
        u.StartCoroutine(u.DownloadString(fn, onDone));
    }

    public static void StartDownloadObj(string id, Action<string, bool> onDone)
    {
        var fn = string.Join("", id, ".obj");
        var u = NewUploader();
        u.StartCoroutine(u.DownloadString(fn, onDone));
    }

    public static void StartDownloadMtl(string id, Action<string, bool> onDone)
    {
        var fn = string.Join("", id, ".mtl");
        var u = NewUploader();
        u.StartCoroutine(u.DownloadString(fn, onDone));
    }

    public static void StartDownloadTexture(string filename, Action<Texture2D, bool> onDone)
    {
        var u = NewUploader();
        u.StartCoroutine(u.DownloadTexture(filename, onDone));
    }

    IEnumerator Upload(byte[] data, string mimeType, string filename, bool downloadAfterUpload, Action<bool> onDone)
    {
        WWWForm form = new WWWForm();
        var guid = Guid.NewGuid().ToString();
        form.AddField("secret", Key);
        form.AddBinaryData("file", data, filename, mimeType);
        UnityWebRequest www = UnityWebRequest.Post(UploadUrl, form);
        yield return www.SendWebRequest();

        bool bSucces = false;
        if (!(www.isNetworkError || www.isHttpError))
        {
            Debug.Log("Completed Upload " + filename);

            if (downloadAfterUpload)
            {
                // Download it
                yield return new WaitForSeconds(2);// Wait 2 seconds for the file to be placed in the correct location
                string dlUrl = "window.open(" + DownloadUrl + "name=" + filename + "&secret=" + Key + ")";

                // TODO: Replace for possibly more appropriate method
                Application.ExternalEval(dlUrl);
                //  Application.OpenURL(DownloadUrl + randomName);
                //  DownloadFile(DownloadUrl + randomName, randomName);
            }

            bSucces = true;
        }

        if (onDone != null) onDone(bSucces);
        if (!bSucces) Debug.Log(www.error);

        Destroy(gameObject);
    }

    IEnumerator UploadPackage(string objData, string mtlData, Texture2D[] textures, Action<bool> onDone)
    {
        var guid = Guid.NewGuid().ToString();
        string filename = guid.ToString() + ".zip";
        byte[] buffer;

        using (MemoryStream zipToOpen = new MemoryStream())
        {
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create, true))
            {
                archive.AddData(guid.ToString() + "_ObjFile.obj", objData);
                archive.AddData(guid.ToString() + "_MtlFile.mtl", mtlData);
                var texArchive = archive.CreateEntry("textures").Archive;
                foreach (var tex in textures)
                {
                    if (tex.width > 0 && tex.height > 0 && tex.isReadable)
                    {
                        try
                        {
                            var bytes = tex.EncodeToPNG();
                            if (bytes != null && bytes.Length != 0)
                            {
                                texArchive.AddData(tex.imageContentsHash.ToString() + ".png", bytes);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                }
            }
            buffer = zipToOpen.ToArray();
        }

        yield return Upload(buffer, "application/zip", filename, true, onDone);
    }

    IEnumerator UploadString(string filename, string data, Action<bool> onDone)
    {
        yield return Upload(Encoding.ASCII.GetBytes(data), "application/x-binary", filename, false, onDone);
    }

    IEnumerator UploadTexture(string filename, Texture2D tex, Action<bool> onDone)
    {
        yield return Upload(tex.EncodeToPNG(), "image/png", filename, false, onDone);
    }

    IEnumerator DownloadString(string filename, Action<string, bool> onDone)
    {
        string url = DownloadUrl + "name=" + filename + "&secret=" + Key;
        var www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        string str = "";
        bool bSucces = false;
        if (!(www.isNetworkError || www.isHttpError))
        {
            str = Encoding.ASCII.GetString(www.downloadHandler.data);
            bSucces = true;
        }
        if (onDone != null) onDone(str, bSucces);
    }

    IEnumerator DownloadTexture(string filename, Action<Texture2D, bool> onDone)
    {
        string url = DownloadUrl + "name=" + filename + "&secret=" + Key;
        var www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        bool succes = false;
        Texture2D tex = null;
        if (!(www.isNetworkError || www.isHttpError))
        {
            tex = new Texture2D(2, 2);
            if (tex.LoadImage(www.downloadHandler.data))
            {
                succes = true;
            }
        }
        if (onDone != null) onDone(tex, succes);
    }
}