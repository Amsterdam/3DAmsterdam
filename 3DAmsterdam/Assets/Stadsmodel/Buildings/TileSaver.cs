using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class Uploader : MonoBehaviour
{
    public static string UploadUrl = "localhost:80/customUpload.php";
    public static string DownloadUrl = "localhost:80/customDownload.php?name=";

    [DllImport("__Internal")]
    private static extern void DownloadFile(string uri, string filename);

    public void StartUpload(string objData, Action<bool> onDone)
    {
        StartCoroutine(Upload(objData, onDone));
    }

    public IEnumerator Upload(string objData, Action<bool> onDone)
    {
        WWWForm form = new WWWForm();
        string randomName = "objData_" + UnityEngine.Random.Range(0, 1000) + ".obj";
        form.AddField("name", randomName);
        form.AddField("objData", objData);

        UnityWebRequest www = UnityWebRequest.Post(UploadUrl, form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            if (onDone != null) onDone(false);
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("Form upload complete!");
            // Download it
            Application.OpenURL(DownloadUrl + randomName);
            //  DownloadFile(DownloadUrl + randomName, randomName);
            if (onDone != null) onDone(true);
        }
    }
}

public class TileSaver
{
    public static void SaveGameObjects(GameObject[] gos, Action<bool> onDone)
    {
        string objData = ComposeObj(gos);
        var saver = new GameObject("SaveTile");
        var sr = saver.AddComponent<Uploader>();
        sr.StartUpload(objData, onDone);
    }

    public static string ComposeObj(GameObject [] gos)
    {
        StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
        List<int> offsets = new List<int>();

        foreach (var go in gos)
        {
            var mrs = go.GetComponentsInChildren<MeshFilter>();
            foreach (var mf in mrs)
            {
                var v = mf.sharedMesh.vertices;
                sw.WriteLine("# vertices");
                for (int i = 0; i < v.Length; i++)
                {
                    sw.WriteLine($"v {v[i].x} {v[i].y} {v[i].z}");
                }
                offsets.Add(v.Length);
            }
        }

        foreach (var go in gos)
        {
            var mrs = go.GetComponentsInChildren<MeshFilter>();
            foreach (var mf in mrs)
            {
                var u = mf.sharedMesh.uv;
                if (u != null && u.Length != 0)
                {
                    sw.WriteLine("# uvs");
                    for (int i = 0; i < u.Length; i++)
                    {
                        sw.WriteLine($"vt {u[i].x} {u[i].y}");
                    }
                }
            }
        }

        foreach (var go in gos)
        {
            var mrs = go.GetComponentsInChildren<MeshFilter>();
            foreach (var mf in mrs)
            {
                var n = mf.sharedMesh.normals;
                if (n != null && n.Length != 0)
                {
                    sw.WriteLine("# normals");
                    for (int i = 0; i < n.Length; i++)
                    {
                        sw.WriteLine($"vn {n[i].x} {n[i].y} {n[i].z}");
                    }
                }
            }
        }

        int ofsIdx = -1;
        foreach (var go in gos)
        {
            var mrs = go.GetComponentsInChildren<MeshFilter>();
            foreach (var mf in mrs)
            {
                sw.WriteLine("# faces");
                int ofs = (ofsIdx == -1 ? 0 : offsets[ofsIdx]) + 1;
                for (int j = 0; j < mf.sharedMesh.subMeshCount; j++)
                {
                    var indices = mf.sharedMesh.GetIndices(j);
                    for (int k = 0; k < indices.Length; k += 3)
                    {
                        sw.WriteLine($"f {indices[k] + ofs} {indices[k + 1] + ofs} {indices[k + 2] + ofs}");
                    }
                }
                ofsIdx++;
            }
        }

        return sw.ToString();
    }
}
