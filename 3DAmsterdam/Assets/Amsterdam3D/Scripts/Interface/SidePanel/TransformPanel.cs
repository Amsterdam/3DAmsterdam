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

    private const string stringDecimal = "F2";
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
    /// Something else transformed our target. Update all parameters.
    /// </summary>
    public void TargetWasTransformed()
    {
        ApplyRotation();
        ApplyTranslation();
        ApplyScale();

        UpdateRDCoordinates();
    }

    /// <summary>
    /// Forces an input string to be parsable.
    /// </summary>
    /// <param name="input">The source string</param>
    /// <returns></returns>
    private string MakeInputParsable(string input)
    {
        if (string.IsNullOrEmpty(input)) return emptyStringDefault;
        if (input == "-") return "-" + emptyStringDefault;
        return input;
	}

    /// <summary>
    /// The translation text input fields are applied to our target object position
    /// </summary>
    /// <param name="value">Required string field for event handlers</param>
    private void TranslationInputChanged(string value = null)
    {
        Vector3RD previewTranslation = basePosition;
        previewTranslation.x += double.Parse(MakeInputParsable(translateX.text), CultureInfo.InvariantCulture);
        previewTranslation.y += double.Parse(MakeInputParsable(translateY.text), CultureInfo.InvariantCulture);
        previewTranslation.z += double.Parse(MakeInputParsable(translateZ.text), CultureInfo.InvariantCulture);

        transformableTarget.transform.position = CoordConvert.RDtoUnity(previewTranslation);

        //Preview the RD coordinates directly in the RD input
        rdX.text = previewTranslation.x.ToString(stringDecimal,CultureInfo.InvariantCulture);
        rdY.text = previewTranslation.y.ToString(stringDecimal, CultureInfo.InvariantCulture);
        napZ.text = previewTranslation.z.ToString(stringDecimal, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// The rotate text input fields is applied to our target object rotation
    /// </summary>
    /// <param name="value">Required string field for event handlers</param>
    private void RotationInputChanged(string value = null)
    {
        transformableTarget.transform.rotation = baseRotation;
        transformableTarget.transform.Rotate(
            float.Parse(MakeInputParsable(rotateX.text)),
            float.Parse(MakeInputParsable(rotateY.text)),
            float.Parse(MakeInputParsable(rotateZ.text))
        );
    }

    /// <summary>
    /// The scale input text fields are used as a multiplier on top of our base scale.
    /// </summary>
    /// <param name="value">Required string field for event handlers</param>
    private void ScaleInputChanged(string value = null)
    {
        Vector3 normalisedScaler = new Vector3(
            baseScale.x * (float.Parse(MakeInputParsable(scaleX.text)) / 100.0f),
            baseScale.y * (float.Parse(MakeInputParsable(scaleY.text)) / 100.0f),
            baseScale.z * (float.Parse(MakeInputParsable(scaleZ.text)) / 100.0f)
        );

        transformableTarget.transform.localScale = normalisedScaler;
    }

    /// <summary>
    /// Applies the translation (uses this position as 0,0,0)
    /// </summary>
    /// <param name="value">Required string field for event handlers</param>
    private void ApplyTranslation(string value = null)
    {
        basePosition = CoordConvert.UnitytoRD(transformableTarget.transform.position);

        //Reset field values to 0
        translateX.text = "0";
        translateY.text = "0";
        translateZ.text = "0";
    }

    /// <summary>
    /// Applies the rotation (uses this rotation as 0,0,0)
    /// </summary>
    /// <param name="value">Required string field for event handlers</param>
    private void ApplyRotation(string value = null)
    {
        baseRotation = transformableTarget.transform.rotation;

        //Reset field values to 0
        rotateX.text = "0";
        rotateY.text = "0";
        rotateZ.text = "0";
    }

    /// <summary>
    /// Applies the scale (uses this scale as 0,0,0)
    /// </summary>
    /// <param name="value">Required string field for event handlers</param>
    private void ApplyScale(string value = null)
    {
        baseScale = transformableTarget.transform.localScale;

        //Reset field values to 0
        scaleX.text = "0";
        scaleY.text = "0";
        scaleZ.text = "0";
    }

    /// <summary>
    /// Set our current base position to the current RD coordinates.
    /// </summary>
    private void UpdateRDCoordinates()
    {
        rdCoordinates = CoordConvert.UnitytoRD(transformableTarget.transform.position);
        rdX.text = rdCoordinates.x.ToString(stringDecimal, CultureInfo.InvariantCulture);
        rdY.text = rdCoordinates.y.ToString(stringDecimal, CultureInfo.InvariantCulture);
        napZ.text = rdCoordinates.z.ToString(stringDecimal, CultureInfo.InvariantCulture);

        basePosition = rdCoordinates;
    }

    /// <summary>
    /// Moves the target object to our text input RD coordinates
    /// </summary>
    /// <param name="value"></param>
    private void RDInputChanged(string value = null)
    {
        rdCoordinates.x = double.Parse(rdX.text, CultureInfo.InvariantCulture);
        rdCoordinates.y = double.Parse(rdY.text, CultureInfo.InvariantCulture);
        rdCoordinates.z = double.Parse(napZ.text, CultureInfo.InvariantCulture);

        transformableTarget.transform.position = CoordConvert.RDtoUnity(rdCoordinates);
    }
}
