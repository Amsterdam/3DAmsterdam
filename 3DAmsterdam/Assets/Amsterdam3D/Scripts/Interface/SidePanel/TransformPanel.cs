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

    private Vector3RD basePosition;
    private Quaternion baseRotation;
    private Vector3 baseScale;

    private const string emptyStringDefault = "0";

    void Start()
    {
        //Store starting position so any transform changes can be added to that untill we lose focus
        translateX.onValueChanged.AddListener(TranslationInputChanged);
        translateY.onValueChanged.AddListener(TranslationInputChanged);
        translateZ.onValueChanged.AddListener(TranslationInputChanged);
        translateX.onEndEdit.AddListener(ApplyTranslation);
        translateY.onEndEdit.AddListener(ApplyTranslation);
        translateZ.onEndEdit.AddListener(ApplyTranslation);

        //Rotation preview and apply
        rotateX.onValueChanged.AddListener(RotationInputChanged);
        rotateY.onValueChanged.AddListener(RotationInputChanged);
        rotateZ.onValueChanged.AddListener(RotationInputChanged);
        rotateX.onEndEdit.AddListener(ApplyRotation);
        rotateY.onEndEdit.AddListener(ApplyRotation);
        rotateZ.onEndEdit.AddListener(ApplyRotation);

        //Scale preview and apply
        scaleX.onValueChanged.AddListener(ScaleInputChanged);
        scaleY.onValueChanged.AddListener(ScaleInputChanged);
        scaleZ.onValueChanged.AddListener(ScaleInputChanged);
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

    /// <summary>
    /// Something else transformed out target. Update all parameters.
    /// </summary>
    public void TargetWasTransformed()
    {
        ApplyRotation();
        ApplyTranslation();
        ApplyScale();

        UpdateRDCoordinates();
    }

    private string ForceStringToANumber(string input)
    {
        if (string.IsNullOrEmpty(input)) return emptyStringDefault;
        if (input == "-") return "-" + emptyStringDefault;
        return input;
	}

    private void TranslationInputChanged(string value = null)
    {
        Vector3RD previewTranslation = basePosition;
        previewTranslation.x += double.Parse(ForceStringToANumber(translateX.text), CultureInfo.InvariantCulture);
        previewTranslation.y += double.Parse(ForceStringToANumber(translateY.text), CultureInfo.InvariantCulture);
        previewTranslation.z += double.Parse(ForceStringToANumber(translateZ.text), CultureInfo.InvariantCulture);

        transformableTarget.transform.position = CoordConvert.RDtoUnity(previewTranslation);

        //Preview the RD coordinates directly in the RD input
        rdX.text = previewTranslation.x.ToString(CultureInfo.InvariantCulture);
        rdY.text = previewTranslation.y.ToString(CultureInfo.InvariantCulture);
        napZ.text = previewTranslation.z.ToString(CultureInfo.InvariantCulture);
    }
    private void RotationInputChanged(string value = null)
    {
        transformableTarget.transform.rotation = baseRotation;
        transformableTarget.transform.Rotate(
            float.Parse(ForceStringToANumber(rotateX.text)),
            float.Parse(ForceStringToANumber(rotateY.text)),
            float.Parse(ForceStringToANumber(rotateZ.text))
        );
    }
    private void ScaleInputChanged(string value = null)
    {
        transformableTarget.transform.localScale = new Vector3(
            baseScale.x + float.Parse(ForceStringToANumber(scaleX.text)),
            baseScale.y + float.Parse(ForceStringToANumber(scaleY.text)),
            baseScale.z + float.Parse(ForceStringToANumber(scaleZ.text))
        );
    }
    private void ApplyTranslation(string value = null)
    {
        basePosition = CoordConvert.UnitytoRD(transformableTarget.transform.position);

        //Reset field values to 0
        translateX.text = "0";
        translateX.text = "0";
        translateX.text = "0";
    }
    private void ApplyRotation(string value = null)
    {
        baseRotation = transformableTarget.transform.rotation;

        //Reset field values to 0
        rotateX.text = "0";
        rotateY.text = "0";
        rotateZ.text = "0";
    }
    private void ApplyScale(string value = null)
    {
        baseScale = transformableTarget.transform.localScale;

        //Reset field values to 0
        scaleX.text = "0";
        scaleY.text = "0";
        scaleZ.text = "0";
    }

    private void UpdateRDCoordinates()
    {
        Debug.Log("Base RD position set");
        rdCoordinates = CoordConvert.UnitytoRD(transformableTarget.transform.position);
        rdX.text = rdCoordinates.x.ToString(CultureInfo.InvariantCulture);
        rdY.text = rdCoordinates.y.ToString(CultureInfo.InvariantCulture);
        napZ.text = rdCoordinates.z.ToString(CultureInfo.InvariantCulture);

        basePosition = rdCoordinates;
    }

    private void RDInputChanged(string value = null)
    {
        rdCoordinates.x = double.Parse(rdX.text, CultureInfo.InvariantCulture);
        rdCoordinates.y = double.Parse(rdY.text, CultureInfo.InvariantCulture);
        rdCoordinates.z = double.Parse(napZ.text, CultureInfo.InvariantCulture);

        transformableTarget.transform.position = CoordConvert.RDtoUnity(rdCoordinates);
    }
}
