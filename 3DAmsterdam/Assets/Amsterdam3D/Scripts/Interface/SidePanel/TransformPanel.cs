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

        //Rotation preview and apply
        rotateX.onValueChanged.AddListener(PreviewRotation);
        rotateY.onValueChanged.AddListener(PreviewRotation);
        rotateZ.onValueChanged.AddListener(PreviewRotation);
        rotateX.onEndEdit.AddListener(ApplyRotation);
        rotateY.onEndEdit.AddListener(ApplyRotation);
        rotateZ.onEndEdit.AddListener(ApplyRotation);

        //Scale preview and apply
        scaleX.onValueChanged.AddListener(PreviewScale);
        scaleY.onValueChanged.AddListener(PreviewScale);
        scaleZ.onValueChanged.AddListener(PreviewScale);
        scaleX.onEndEdit.AddListener(ApplyScale);
        scaleY.onEndEdit.AddListener(ApplyScale);
        scaleZ.onEndEdit.AddListener(ApplyScale);

        //Add listeners to change
        rdX.onValueChanged.AddListener(RDInputChanged);
        rdY.onValueChanged.AddListener(RDInputChanged);
        napZ.onValueChanged.AddListener(RDInputChanged);
    }

    public void SetTarget(GameObject targetGameObject)
    {
        transformableTarget = targetGameObject;

        ApplyRotation();
        ApplyTranslation();
        ApplyScale();

        //Sets our RD translation offset
        UpdateRDCoordinates();
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

        UpdateRDCoordinates();
    }
    private void PreviewRotation(string value = null)
    {
        //Empty fields default to 0
        if (string.IsNullOrEmpty(rotateX.text)) rotateX.text = emptyStringDefault;
        if (string.IsNullOrEmpty(rotateX.text)) rotateX.text = emptyStringDefault;
        if (string.IsNullOrEmpty(rotateX.text)) rotateX.text = emptyStringDefault;
    }
    private void PreviewScale(string value = null)
    {
        //Empty fields default to 0
        if (string.IsNullOrEmpty(scaleX.text)) scaleX.text = emptyStringDefault;
        if (string.IsNullOrEmpty(scaleY.text)) scaleY.text = emptyStringDefault;
        if (string.IsNullOrEmpty(scaleZ.text)) scaleZ.text = emptyStringDefault;

        scaleOffset.x += float.Parse(scaleX.text);
        scaleOffset.y += float.Parse(scaleY.text);
        scaleOffset.z += float.Parse(scaleZ.text);

        transformableTarget.transform.localScale = scaleOffset;
    }
    private void ApplyTranslation(string value = null)
    {
        moveOffset = CoordConvert.UnitytoRD(transformableTarget.transform.position);

        //Reset field values to 0
        translateX.text = "0";
        translateY.text = "0";
        translateZ.text = "0";
    }
    private void ApplyRotation(string value = null)
    {
        rotationOffset = transformableTarget.transform.rotation;
    }
    private void ApplyScale(string value = null)
    {
        scaleOffset = transformableTarget.transform.localScale;
    }

    private void UpdateRDCoordinates()
    {
        rdCoordinates = CoordConvert.UnitytoRD(transformableTarget.transform.position);
        rdX.text = rdCoordinates.x.ToString(CultureInfo.InvariantCulture);
        rdY.text = rdCoordinates.y.ToString(CultureInfo.InvariantCulture);
        napZ.text = rdCoordinates.z.ToString(CultureInfo.InvariantCulture);

        moveOffset = rdCoordinates;
    }

    private void RDInputChanged(string value = null)
    {
        rdCoordinates.x = double.Parse(rdX.text, CultureInfo.InvariantCulture);
        rdCoordinates.y = double.Parse(rdY.text, CultureInfo.InvariantCulture);
        rdCoordinates.z = double.Parse(napZ.text, CultureInfo.InvariantCulture);

        transformableTarget.transform.position = CoordConvert.RDtoUnity(rdCoordinates);
    }
}
