using Netherlands3D.JavascriptConnection;
using UnityEngine;

public class JsonDownloader : MonoBehaviour
{
    public void DownloadCityJSON()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string jsontext = CityJSONFormatter.GetJSON();
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsontext);
        JavascriptMethodCaller.DownloadByteArrayAsFile(bytes, bytes.Length, "gebouw_met_uitbouw.json");
#else
        Debug.Log("downloading file only works in a WebGL build");
        Debug.Log(CityJSONFormatter.GetJSON());
#endif
    }
}
