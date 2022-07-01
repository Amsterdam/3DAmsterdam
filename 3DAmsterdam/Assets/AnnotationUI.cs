using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnnotationUI : MonoBehaviour
{
    [SerializeField]
    private RectTransform annotationBody;
    private RectTransform myRectTransform;
    [SerializeField]
    private Text numberText;
    private Annotator annotator;

    public int Id { get; private set; }
    public bool IsOpen => annotationBody.gameObject.activeInHierarchy;

    private void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();
        annotator = GetComponentInParent<Annotator>();
    }

    //called by button in inspector
    public void ToggleAnnotation()
    {
        float newHeight = IsOpen ? myRectTransform.sizeDelta.y - annotationBody.sizeDelta.y : myRectTransform.sizeDelta.y + annotationBody.sizeDelta.y;
        myRectTransform.sizeDelta = new Vector2(myRectTransform.sizeDelta.x, newHeight);

        annotationBody.gameObject.SetActive(!IsOpen);
        annotator.RecalculeteContentHeight();
    }

    public void DeleteAnnotation()
    {
        annotator.RemoveAnnotation(Id);
    }

    public void SetId(int id)
    {
        Id = id;
        numberText.text = (id + 1).ToString();
    }
}
