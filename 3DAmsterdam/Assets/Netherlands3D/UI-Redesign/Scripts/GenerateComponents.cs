using Netherlands3D.BAG;
using Netherlands3D.Cameras;
using Netherlands3D.Core.Colors;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Interface.Redesign;
using Netherlands3D.Interface.SidePanel ;

public class GenerateComponents : MonoBehaviour
{
    [SerializeField]
    private Transform generatedFieldsRootContainer;
    private Transform targetFieldsContainer;

    public static GenerateComponents Instance = null;

    [Header("Generated field prefabs:")]
    [SerializeField]
    private GameObject titlePrefab;
    [SerializeField]
    private GameObject textfieldPrefab;
    [SerializeField]
    private GameObject labelPrefab;
    [SerializeField]
    private GameObject dataFieldPrefab;
    [SerializeField]
    private GameObject seperatorLinePrefab;
    [SerializeField]
    private GameObject spacerPrefab;
    [SerializeField]
    private GameObject urlPrefab;

    private void Awake()
    {
        Instance = new GenerateComponents();
    }


    public void AddTitle(string titleText)
    {
        var newTitleField = Instantiate(titlePrefab, targetFieldsContainer);
        newTitleField.GetComponentInChildren<TextMeshProUGUI>().text = titleText;
    }

    public void AddLink(string urlText, string urlPath)
    {
        var nameAndUrl = Instantiate(urlPrefab, targetFieldsContainer);
        nameAndUrl.GetComponentInChildren<TextMeshProUGUI>().text = urlText;
        nameAndUrl.GetComponent<Netherlands3D.Interface.Redesign.OpenURL>().SetUrl(urlPath);
    }

    public void AddTextfield(string content)
    {
        var newTextField = Instantiate(textfieldPrefab, targetFieldsContainer);
        newTextField.GetComponentInChildren<TextMeshProUGUI>().text = content;
    }
    public void AddLabel(string labelText)
    {
        Instantiate(labelPrefab, targetFieldsContainer).GetComponentInChildren<TextMeshProUGUI>().text = labelText;
    }
        
    public void AddDataField(string keyTitle, string valueText, bool selectableValueText = true)
    {

        GameObject dataKeyAndValue = Instantiate(dataFieldPrefab, targetFieldsContainer);
        DataKeyAndValue dataKeyAndValueComp = dataKeyAndValue.GetComponent<DataKeyAndValue>();
        dataKeyAndValueComp.SetTexts(keyTitle, valueText);        
    }
        
    public void AddSeperatorLine()
    {
        Instantiate(seperatorLinePrefab, targetFieldsContainer);
    }
    public void AddSpacer(float height = 5.0f)
    {
        Instantiate(spacerPrefab, targetFieldsContainer).GetComponent<RectTransform>().sizeDelta = new Vector2(100, height);
    }

    private void ClearGeneratedFields(GameObject ignoreGameObject, List<GameObject> ignoreGameObjects)
    {
        foreach (Transform field in generatedFieldsRootContainer)
        {
            if (ignoreGameObject != null && (field.gameObject == ignoreGameObject || ignoreGameObject.transform.IsChildOf(field))) continue;
            else if (ignoreGameObjects != null && field.gameObject == ignoreGameObjects.Contains(field.gameObject)) continue;

            Destroy(field.gameObject);
        }
    }

    public void ClearGeneratedFields(GameObject ignoreGameObject = null)
    {
        ClearGeneratedFields(ignoreGameObject, null);
    }

    public void ClearGeneratedFields(List<GameObject> ignoreGameObjects)
    {
        ClearGeneratedFields(null, ignoreGameObjects);
    }

    public void ClearGeneratedFields()
    {
        ClearGeneratedFields(null, null);
    }
}

