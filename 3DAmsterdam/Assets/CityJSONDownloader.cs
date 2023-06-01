using Netherlands3D.T3DPipeline;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using System.IO;
using UnityEngine.Events;
using Netherlands3D.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CityJSONDownloader : MonoBehaviour
{
#if UNITY_WEBGL
    //FileUpload.jslib methods
    [DllImport("__Internal")]
    private static extern void DownloadFromIndexedDB(string filePath, string callbackObject, string callbackMethod);

    [DllImport("__Internal")]
    private static extern void SyncFilesFromIndexedDB(string callbackObject, string callbackMethod);

    [DllImport("__Internal")]
    private static extern void SyncFilesToIndexedDB(string callbackObject, string callbackMethod);
#endif

    private GameObject targetGameObject;

    [Header("Progress indiction events")]
    [SerializeField] private UnityEvent<float> progress = new();
    [SerializeField] private UnityEvent<string> description = new();
    [SerializeField] private UnityEvent<string> stageDescription = new();

    private string tempFileName = "";
    private string tempStreamwritePath = "";
    [SerializeField] private int writesToShowFeedback = 100;

    public void DownloadMeshAsCityJSON(GameObject gameObjectWithMesh)
    {
        if (gameObjectWithMesh.TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh)
        {
            targetGameObject = gameObjectWithMesh;
            StartCoroutine(DownloadWithProgress(meshFilter.sharedMesh, CoordConvert.UnitytoRD(gameObjectWithMesh.transform.position)));
        }
        else
        {
            Debug.LogWarning("Object does not have a MeshFilter with a Mesh", gameObjectWithMesh);
        }
    }

    private IEnumerator DownloadWithProgress(Mesh mesh, Vector3RD rdCoordinate)
    {
        description.Invoke("Downloaden als CityJSON");
        stageDescription.Invoke("Converteren...");
        progress.Invoke(0.001f);
        yield return new WaitForEndOfFrame();

        var triangles = mesh.triangles;
        var vertices = mesh.vertices;

        float totalWrites = (triangles.Length/3) + vertices.Length;
        float currentWrite = 0;

        if (triangles.Length > 2 && vertices.Length > 0)
        {
            //stream write cityjson lines
            tempFileName = targetGameObject.name + "_CityJSON.json";
            tempStreamwritePath = Application.persistentDataPath + "/" + tempFileName;
            var streamWriter = new StreamWriter(tempStreamwritePath);
            streamWriter.Write("{\"type\":\"CityJSON\",\"version\":\"1.0\",\"CityObjects\":{\"" + targetGameObject.name + "\":{\"type\":\"Building\",\"geometry\":[{\"type\":\"MultiSurface\",\"lod\":3.3,\"boundaries\":[");
            
            //Streamwrite own triangle array
            for (int i = 0; i < triangles.Length; i+=3)
            {
                if (i > 0) streamWriter.Write(",");
                //The 3 indices making up the triangle
                var a = triangles[i];
                var b = triangles[i + 1];
                var c = triangles[i + 2];
                streamWriter.Write(string.Format("[[{0},{1},{2}]]", a, b, c));

                currentWrite++;
                if (currentWrite % writesToShowFeedback == 0)
                {
                    progress.Invoke(currentWrite / totalWrites);
                    stageDescription.Invoke($"Wegschrijven: {Mathf.Round((currentWrite / totalWrites) * 100)}%");
                    yield return null;
                }
            }
            streamWriter.Write("]}]}},\"vertices\":[");

            //Streamwrite own vertices array
            for (int i = 0; i < vertices.Length; i++)
            {
                if (i > 0) streamWriter.Write(",");
                //Triangle indices
                var vertex = vertices[i];
                streamWriter.Write(string.Format("[{0},{1},{2}]", vertex.x, vertex.y, vertex.z));

                currentWrite++;
                if (currentWrite % writesToShowFeedback == 0)
                {
                    progress.Invoke(currentWrite / totalWrites);
                    stageDescription.Invoke($"Wegschrijven: {Mathf.Round((currentWrite/totalWrites)*100)}%");
                    yield return null;
                }
            }

            //And close json
            streamWriter.Write("]}");
            streamWriter.Close();


#if UNITY_WEBGL && !UNITY_EDITOR
            //Make sure file exists in JS side of IndexedDB registry before downloading
            SyncFilesToIndexedDB(this.gameObject.name, nameof(SyncedFromIndexedDB));
#elif UNITY_EDITOR
            string selectedPath = EditorUtility.SaveFilePanel("Select File", "", $"{tempFileName}", "json");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                File.Copy(tempStreamwritePath, selectedPath,true);
                Debug.Log($"{selectedPath} saved.");
            }

            ClearTempFile();
            stageDescription.Invoke("Gereed");
            progress.Invoke(1.0f);
#endif
        }
    }

    /// <summary>
    /// Starts download on IndexedDB/JS side after sync.
    /// </summary>
    public void SyncedFromIndexedDB()
    {
        DownloadFromIndexedDB(tempFileName, this.gameObject.name, nameof(DownloadDoneFromIndexedDB));
    }

    /// <summary>
    /// Download method done on IndexedDB/JS side
    /// </summary>
    public void DownloadDoneFromIndexedDB(string filename)
    { 
        Debug.Log($"Done downloading via IndexedDB {filename}");
        ClearTempFile();
        stageDescription.Invoke("Gereed");
        progress.Invoke(1.0f);
    }

    private void ClearTempFile()
    {
        if (File.Exists(tempStreamwritePath))
            File.Delete(tempStreamwritePath);
    }

}
