using ConvertCoordinates;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TransformPanel : MonoBehaviour
{
    [Header("Input field references")]
    [SerializeField]
    private InputField translateX;
    [SerializeField]
    private InputField translateY;
    [SerializeField]
    private InputField translateZ;

    [SerializeField]
    private InputField rotateX;
    [SerializeField]
    private InputField rotateY;
    [SerializeField]
    private InputField rotateZ;

    [SerializeField]
    private InputField scaleX;
    [SerializeField]
    private InputField scaleY;
    [SerializeField]
    private InputField scaleZ;

    [SerializeField]
    private InputField rdX;
    [SerializeField]
    private InputField rdY;
    [SerializeField]
    private InputField napZ;

    private GameObject transformableTarget;
    private Vector3RD rdCoordinates;

    private Vector3RD moveOffset;
    private Quaternion rotationOffset;
    private Vector3 scaleOffset;

    private const string emptyStringDefault = "0";

    void Start()
    {
        //Store starting position so any transform changes can be added to that untill we lose focus
        translateX.onValueChanged.AddListener(PreviewTranslation);
        translateY.onValueChanged.AddListener(PreviewTranslation);
        translateZ.onValueChanged.AddListener(PreviewTranslation);

        translateX.onEndEdit.AddListener(ApplyTranslation);
        translateY.onEndEdit.AddListener(ApplyTranslation);
        translateZ.onEndEdit.AddListener(ApplyTranslation);

        //Add listeners to change
        rdX.onValueChanged.AddListener(RDInputChanged);
        rdY.onValueChanged.AddListener(RDInputChanged);
        napZ.onValueChanged.AddListener(RDInputChanged);
    }

    public void SetTarget(GameObject targetGameObject)
    {
        transformableTarget = targetGameObject;
        GetRDCoordinates();
    }

    private void PreviewTranslation(string value = null)
    {
        //Empty fields default to 0
        if (string.IsNullOrEmpty(translateX.text)) translateX.text = emptyStringDefault;
        if (string.IsNullOrEmpty(translateY.text)) translateY.text = emptyStringDefault;
        if (string.IsNullOrEmpty(translateZ.text)) translateZ.text = emptyStringDefault;

        Vector3RD previewTranslation = moveOffset;
        previewTranslation.x += double.Parse(translateX.text);
        previewTranslation.y += double.Parse(translateY.text);
        previewTranslation.z += double.Parse(translateZ.text);

        transformableTarget.transform.position = CoordConvert.RDtoUnity(previewTranslation);
    }
    private void ApplyTranslation(string value = null)
    {
        moveOffset = CoordConvert.UnitytoRD(transformableTarget.transform.position);

        //Reset field values to 0
        translateX.text = "0";
        translateY.text = "0";
        translateZ.text = "0";
    }

    private void GetRDCoordinates()
    {
        rdCoordinates = CoordConvert.UnitytoRD(transformableTarget.transform.position);
        rdX.text = rdCoordinates.x.ToString(CultureInfo.InvariantCulture);
        rdY.text = rdCoordinates.y.ToString(CultureInfo.InvariantCulture);
        napZ.text = rdCoordinates.z.ToString(CultureInfo.InvariantCulture);
    }

    private void RDInputChanged(string value = null)
    {
        rdCoordinates.x = double.Parse(rdX.text, CultureInfo.InvariantCulture);
        rdCoordinates.y = double.Parse(rdY.text, CultureInfo.InvariantCulture);
        rdCoordinates.z = double.Parse(napZ.text, CultureInfo.InvariantCulture);

        transformableTarget.transform.position = CoordConvert.RDtoUnity(rdCoordinates);
    }
}
