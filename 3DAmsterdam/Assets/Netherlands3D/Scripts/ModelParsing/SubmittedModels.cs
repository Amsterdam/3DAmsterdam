using Netherlands3D;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SubmittedModels : MonoBehaviour
{
    private string code = "";
    [SerializeField] private ConfigurationFile config;

    [SerializeField] private UnityEvent<string> loadModel;
    [SerializeField] private UnityEvent<string> unloadModel;

    [SerializeField] private GameObject template;

    private List<GameObject> items = new List<GameObject>();

    [Serializable]
    public class ApprovalStages
    {
        public string[] submitted;
        public string[] approved;
        public string[] denied;
    }

    private void Awake()
    {
        template.gameObject.SetActive(false);
    }

    public void SetCode(string code)
    {
        this.code = code;
        this.gameObject.SetActive(true);

        Refresh();
    }

    private void Clear()
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            var item = items[i];
            Destroy(item);

            items.RemoveAt(i);
        }
    }

    public void Refresh()
    {
        Clear();
        StartCoroutine(LoadList());
    }

    private void DrawLists(ApprovalStages approvalStagesLists)
    {
        foreach(var item in approvalStagesLists.submitted)
        {
            var newListItem = Instantiate(template,template.transform.parent);
            newListItem.GetComponentInChildren<TMP_Text>().text = item;
            newListItem.SetActive(true);

            items.Add(newListItem);
        }
    }

    private IEnumerator LoadList()
    {
        var listUrl = $"{config.submittedModelsURL}?code={code}";
        Debug.Log($"Getting list from: {listUrl}");
        using UnityWebRequest webRequest = UnityWebRequest.Get(listUrl);

        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning(webRequest.error);
        }
        else
        {
            var json = webRequest.downloadHandler.text;

            var approvalStagesLists = JsonUtility.FromJson<ApprovalStages>(json);
            DrawLists(approvalStagesLists);
        }
    }

    public void LoadModel(BaseEventData modelBullet)
    {
        var isOn = modelBullet.selectedObject.GetComponent<Toggle>().isOn;
        var textMesh = modelBullet.selectedObject.GetComponentInChildren<TMP_Text>();
        Debug.Log(textMesh.text);

        if (isOn)
        {
            //Toggle on atm? This click will disable/ unload model
            unloadModel.Invoke(textMesh.text);
        }
        else{
            loadModel.Invoke(textMesh.text);
        }
    }
}
