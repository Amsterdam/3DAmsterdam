using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
public abstract class ImportAPI : MonoBehaviour
{
    [HideInInspector]
    public string dataResult;

    public IEnumerator CallAPI(string apiUrl, string bogIndexInt, int resultIndex)
    {
        string url = apiUrl + bogIndexInt + "/";

        var request = UnityWebRequest.Get(url);
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                WarningDialogs.Instance.ShowNewDialog("Sorry, door een probleem met de server kan de informatie tijdelijk niet worden geladen.");
            }
            else{
                dataResult = request.downloadHandler.text;
            }
        }
    }
}
