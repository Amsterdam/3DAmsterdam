using Netherlands3D;
using Netherlands3D.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SubmittedModelsList : MonoBehaviour
{
    [HideInInspector] public string code = "";

    [SerializeField] private ConfigurationFile config;
    [SerializeField] private UnityEvent<string> selectModel;
    [SerializeField] private UnityEvent<string> deselectModel;
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

    public void GetListWithCode(string code)
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
            var submittedModel = newListItem.GetComponent<SubmittedModel>();
            submittedModel.modelPath = item;
            submittedModel.GetComponentInChildren<TMP_Text>().text = Path.GetFileName(item);
            submittedModel.parentList = this;

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
        var submittedModel = modelBullet.selectedObject.GetComponent<SubmittedModel>();
        Debug.Log(submittedModel.modelPath);

        if (isOn)
        {
            //Toggle on atm? This click will disable/ unload model
            deselectModel.Invoke(submittedModel.modelPath);
        }
        else{
            selectModel.Invoke(config.downloadSubmittedModel.Replace("{modelpath}", submittedModel.modelPath) + $"?code={code}");
        }
    }
}
