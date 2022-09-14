using System;
using System.Collections;
using System.Collections.Generic;
using ConvertCoordinates;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class AnnotationUISaveData : SaveDataContainer
{
    //public int Id; //unique id for this boundary feature
    public string ParentCityObject;
    public Vector3 ConnectionPoint;
    public string AnnotationText;

    public AnnotationUISaveData(string instanceId) : base(instanceId)
    {

    }

    public void SetId(string id)
    {
        InstanceId = id;
    }
}

public class AnnotationUI : MonoBehaviour
{
    [SerializeField]
    private RectTransform annotationBody;
    private RectTransform myRectTransform;
    [SerializeField]
    private Text numberText;
    private AnnotationState annotator;
    [SerializeField]
    private InputField inputField;
    [SerializeField]
    private RectTransform collapseIcon;
    [SerializeField]
    private Image numberBackgroundImage;
    [SerializeField]
    private Color selectedColor;
    private Color normalColor;

    private AnnotationUISaveData saveData;

    public int Id { get; private set; }
    public bool IsOpen => annotationBody.gameObject.activeInHierarchy;
    public string Text => saveData.AnnotationText == null ? string.Empty : saveData.AnnotationText; // can load as null for some reason
    public string ParentCityObject => saveData.ParentCityObject;
    public Vector3RD ConnectionPointRD => CoordConvert.UnitytoRD(saveData.ConnectionPoint);

    private void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();
        annotator = GetComponentInParent<AnnotationState>();
        saveData = new AnnotationUISaveData("undefined");
        normalColor = numberBackgroundImage.color;
    }

    private void OnEnable()
    {
        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    }

    public void OnInputFieldValueChanged(string newText)
    {
        saveData.AnnotationText = newText;
    }

    //called by button in inspector
    public void ToggleAnnotation()
    {
        float newHeight = IsOpen ? myRectTransform.sizeDelta.y - annotationBody.sizeDelta.y : myRectTransform.sizeDelta.y + annotationBody.sizeDelta.y;
        myRectTransform.sizeDelta = new Vector2(myRectTransform.sizeDelta.x, newHeight);
        collapseIcon.Rotate(new Vector3(0, 0, 180));

        annotationBody.gameObject.SetActive(!IsOpen);
        annotator.RecalculeteContentHeight();
    }

    public void SetText(string text)
    {
        inputField.text = text;
    }

    public void DeleteAnnotation()
    {
        annotator.RemoveAnnotation(Id);
    }

    public void Initialize(int id, string parentCityObjectId, Vector3 connectionPoint)
    {
        SetId(id);
        saveData.ConnectionPoint = connectionPoint;
        saveData.ParentCityObject = parentCityObjectId;
    }

    public void SetId(int id)
    {
        Id = id;
        numberText.text = (id + 1).ToString();
        saveData.SetId(numberText.text);
    }

    public void SetSelectedColor(bool selected)
    {
        var color = selected ? selectedColor : normalColor;
        color.a = 256f;
        numberBackgroundImage.color = color;
        
        //numberBackgroundImage.color = new Color(1, 1, 0, 1);
    }

    private void OnDestroy()
    {
        saveData.DeleteSaveData();
    }
}
