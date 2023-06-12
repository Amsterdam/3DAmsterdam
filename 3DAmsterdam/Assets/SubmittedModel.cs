using Netherlands3D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SubmittedModel : MonoBehaviour
{
    public string modelPath = "";
    public GameObject linkedModel;

    [SerializeField] private ConfigurationFile config;

    public SubmittedModels parentList;

    public void Approve()
    {
        StartCoroutine(ChangeStage("approved"));
    }

    public void Deny()
    {
        StartCoroutine(ChangeStage("denied"));
    }

    private IEnumerator ChangeStage(string targetStage)
    {
        var changeUrl = config.changeModelStageURL.
            Replace("/changerequests/", "").
            Replace("{modelpath}", modelPath).
            Replace("{newstage}", targetStage) 
            + $"?={parentList.code}";

        Debug.Log($"Moving {modelPath} to {targetStage} using {changeUrl}");

        using UnityWebRequest webRequest = UnityWebRequest.Get(changeUrl);
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Could not move {modelPath} to {targetStage}");
            Debug.LogWarning(webRequest.error);
        }
        else
        {
            Debug.Log($"Moved {modelPath} to {targetStage}");
            gameObject.SetActive(false);
        }
    }
}
