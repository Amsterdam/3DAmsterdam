using ConvertCoordinates;
using Netherlands3D.ObjectInteraction;
using RuntimeHandle;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.Interface.SidePanel
{
    public class TransformPanel : MonoBehaviour
    {
        [SerializeField]
        private Text transformTitle;

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

        private Transformable transformableTarget;
        private Vector3RD rdCoordinates;

        private Vector3 basePositionUnity;
        private Vector3RD basePosition;
        private Quaternion baseRotation;
        private Vector3 baseScale;

        private const string stringDecimal = "F2";
        private const string emptyStringDefault = "0";
        private const string scaleSuffix = "%";

        private static RuntimeTransformHandle gizmoHandles;

        private bool ignoreChangeEvents = false;
        private bool coordinateSystemLocal = true;

		public Transformable TransformableTarget { get => transformableTarget; }

		void Start()
        {
            //Store starting position so any transform changes can be added to that untill we lose focus
            translateX.onValueChanged.AddListener(TranslationInputChanged);
            translateY.onValueChanged.AddListener(TranslationInputChanged);
            translateZ.onValueChanged.AddListener(TranslationInputChanged);

            //Rotation preview and apply
            rotateX.onValueChanged.AddListener(RotationInputChanged);
            rotateY.onValueChanged.AddListener(RotationInputChanged);
            rotateZ.onValueChanged.AddListener(RotationInputChanged);

            //Scale preview and apply
            scaleX.onValueChanged.AddListener(ScaleInputChanged);
            scaleY.onValueChanged.AddListener(ScaleInputChanged);
            scaleZ.onValueChanged.AddListener(ScaleInputChanged);

            //Add listeners to change
            rdX.onValueChanged.AddListener(RDInputChanged);
            rdY.onValueChanged.AddListener(RDInputChanged);
            napZ.onValueChanged.AddListener(RDInputChanged);
        }

        /// <summary>
        /// Create the object containing our 3D gizmos if it does not exist yet
        /// </summary>
		private void CreateGizmoHandles()
        {
            if (gizmoHandles) return;

            gizmoHandles = RuntimeTransformHandle.Create(null, HandleType.POSITION);
            gizmoHandles.autoScale = true;
            gizmoHandles.space = HandleSpace.LOCAL;
        }
        
        //Public methods we can trigger via serialized unity button click events
        public void TranslationGizmo()
        {
            if (gizmoHandles) gizmoHandles.type = HandleType.POSITION;
        }
        public void RotationGizmo()
        {
            if (gizmoHandles) gizmoHandles.type = HandleType.ROTATION;
        }
        public void ScaleGizmo()
        {
            if (gizmoHandles) gizmoHandles.type = HandleType.SCALE;
        }

        public void DisableGizmo()
		{
            if (gizmoHandles) gizmoHandles.gameObject.SetActive(false);
        }

		/// <summary>
		/// Define the target transformable our transformpanel will manipulate
		/// </summary>
		/// <param name="target">The target transformable we want to manipulate using the transform panel</param>
		public void SetTarget(Transformable target)
        {
            transformTitle.text = target.name;
            transformableTarget = target;

            ApplyTransformOffsets(); //Our starting percentage scale is always 100% (even if our imported/created stuff has a strange scale)
            TargetWasTransformed();

            //Target our 3D gizmo on the same object
            CreateGizmoHandles();
            gizmoHandles.gameObject.SetActive(true);
            gizmoHandles.target = transformableTarget.transform;
            gizmoHandles.enabled = true;
            gizmoHandles.movedHandle.RemoveAllListeners();
            gizmoHandles.movedHandle.AddListener(TargetWasTransformed);

            //Display this panel
            gameObject.SetActive(true);

            //StartCoroutine(SortLayout());
            this.transform.SetSiblingIndex(1);
        }
        private IEnumerator SortLayout()
        {
            //Give our parent layout group a juggle, so it respects our new size
            VerticalLayoutGroup layoutGroup = transform.parent.GetComponent<VerticalLayoutGroup>();
            layoutGroup.enabled = false;
            yield return new WaitForEndOfFrame();
            layoutGroup.enabled = true;
        }

        /// <summary>
        /// Something else transformed our target. Update all parameters.
        /// </summary>
        public void TargetWasTransformed()
        {
            ignoreChangeEvents = true;

            SetRDCoordinateFields();
            UpdateWithCurrentTransform();

            ignoreChangeEvents = false;
        }

        /// <summary>
        /// Update the input fields using the transform values of our current targeted transformable
        /// </summary>
        private void UpdateWithCurrentTransform()
        {
            if (coordinateSystemLocal)
            {
                Vector3 localCurrentTransformPoint = TransformExtensions.InverseTransformPointUnscaled(transformableTarget.transform, basePositionUnity);
                Vector3 targetTransformPoint = TransformExtensions.InverseTransformPointUnscaled(transformableTarget.transform, transformableTarget.transform.position);

                var translationX = targetTransformPoint.x - localCurrentTransformPoint.x;
                var translationY = targetTransformPoint.y - localCurrentTransformPoint.y;
                var translationZ = targetTransformPoint.z - localCurrentTransformPoint.z;

                translateX.text = (translationX).ToString(stringDecimal, CultureInfo.InvariantCulture);
                translateY.text = (translationZ).ToString(stringDecimal, CultureInfo.InvariantCulture);
                translateZ.text = (translationY).ToString(stringDecimal, CultureInfo.InvariantCulture);
            }
            else{
                translateX.text = (rdCoordinates.x - basePosition.x).ToString(stringDecimal, CultureInfo.InvariantCulture);
                translateY.text = (rdCoordinates.y - basePosition.y).ToString(stringDecimal, CultureInfo.InvariantCulture);
                translateZ.text = (rdCoordinates.z - basePosition.z).ToString(stringDecimal, CultureInfo.InvariantCulture);
            }

            rotateX.text = (transformableTarget.transform.eulerAngles.x - baseRotation.eulerAngles.x).ToString(stringDecimal, CultureInfo.InvariantCulture);
            rotateY.text = (transformableTarget.transform.eulerAngles.z - baseRotation.eulerAngles.z).ToString(stringDecimal, CultureInfo.InvariantCulture);
            rotateZ.text = (transformableTarget.transform.eulerAngles.y - baseRotation.eulerAngles.y).ToString(stringDecimal, CultureInfo.InvariantCulture);

            scaleX.text = ((transformableTarget.transform.localScale.x / baseScale.x) * 100.0f).ToString(stringDecimal, CultureInfo.InvariantCulture) + scaleSuffix;
            scaleY.text = ((transformableTarget.transform.localScale.z / baseScale.z) * 100.0f).ToString(stringDecimal, CultureInfo.InvariantCulture) + scaleSuffix;
            scaleZ.text = ((transformableTarget.transform.localScale.y / baseScale.y) * 100.0f).ToString(stringDecimal, CultureInfo.InvariantCulture) + scaleSuffix;
        }

        /// <summary>
        /// Use the current rotation/translation and scale as our base 0,0,0 value
        /// </summary>
        private void ApplyTransformOffsets()
        {
            ApplyRotation();
            ApplyTranslation();
            ApplyScale();
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
            if (ignoreChangeEvents) return;

            if (coordinateSystemLocal)
            {
                //Local always translates from our saved base position
                transformableTarget.transform.position = basePositionUnity;
                transformableTarget.transform.Translate(
                    float.Parse(MakeInputParsable(translateX.text), CultureInfo.InvariantCulture),
                    float.Parse(MakeInputParsable(translateZ.text), CultureInfo.InvariantCulture),
                    float.Parse(MakeInputParsable(translateY.text), CultureInfo.InvariantCulture)
                );

                Vector3RD previewTranslation = CoordConvert.UnitytoRD(transformableTarget.transform.position);
                rdX.text = previewTranslation.x.ToString(stringDecimal, CultureInfo.InvariantCulture);
                rdY.text = previewTranslation.y.ToString(stringDecimal, CultureInfo.InvariantCulture);
                napZ.text = previewTranslation.z.ToString(stringDecimal, CultureInfo.InvariantCulture);
            }
            else{
                Vector3RD previewTranslation = basePosition;
                previewTranslation.x += double.Parse(MakeInputParsable(translateX.text), CultureInfo.InvariantCulture);
                previewTranslation.y += double.Parse(MakeInputParsable(translateY.text), CultureInfo.InvariantCulture);
                previewTranslation.z += double.Parse(MakeInputParsable(translateZ.text), CultureInfo.InvariantCulture);

                transformableTarget.transform.position = CoordConvert.RDtoUnity(previewTranslation);

                //Preview the RD coordinates directly in the RD input
                rdX.text = previewTranslation.x.ToString(stringDecimal, CultureInfo.InvariantCulture);
                rdY.text = previewTranslation.y.ToString(stringDecimal, CultureInfo.InvariantCulture);
                napZ.text = previewTranslation.z.ToString(stringDecimal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// The rotate text input fields is applied to our target object rotation
        /// </summary>
        /// <param name="value">Required string field for event handlers</param>
        private void RotationInputChanged(string value = null)
        {
            if (ignoreChangeEvents) return;

            transformableTarget.transform.rotation = baseRotation;
            transformableTarget.transform.Rotate(
                float.Parse(MakeInputParsable(rotateX.text)),
                float.Parse(MakeInputParsable(rotateY.text)),
                float.Parse(MakeInputParsable(rotateZ.text))
            );

            if(coordinateSystemLocal)
            {
                ApplyTranslation();
            }
        }

        /// <summary>
        /// The scale input text fields are used as a multiplier on top of our base scale.
        /// </summary>
        /// <param name="value">Required string field for event handlers</param>
        private void ScaleInputChanged(string value = null)
        {
            if (ignoreChangeEvents) return;

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
        private void ApplyTranslation()
        {
            basePositionUnity = transformableTarget.transform.position;
            basePosition = CoordConvert.UnitytoRD(basePositionUnity);

            //Reset field values to 0 meter
            translateX.text = "0";
            translateY.text = "0";
            translateZ.text = "0";
        }

        /// <summary>
        /// Applies the rotation (uses this rotation as 0,0,0)
        /// </summary>
        private void ApplyRotation()
        {
            baseRotation = transformableTarget.transform.rotation;

            //Reset field values to 0 degrees
            rotateX.text = "0";
            rotateY.text = "0";
            rotateZ.text = "0";
        }

        /// <summary>
        /// Applies the scale (uses this scale as 0,0,0)
        /// </summary>
        private void ApplyScale()
        {
            baseScale = transformableTarget.transform.localScale;

            //Reset field values to 100%
            scaleX.text = "100";
            scaleY.text = "100";
            scaleZ.text = "100";
        }

        /// <summary>
        /// Set our current base position to the current RD coordinates.
        /// </summary>
        private void SetRDCoordinateFields()
        {
            rdCoordinates = CoordConvert.UnitytoRD(transformableTarget.transform.position);
            rdX.text = rdCoordinates.x.ToString(stringDecimal, CultureInfo.InvariantCulture);
            rdY.text = rdCoordinates.y.ToString(stringDecimal, CultureInfo.InvariantCulture);
            napZ.text = rdCoordinates.z.ToString(stringDecimal, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Moves the target object to our text input RD coordinates
        /// </summary>
        /// <param name="value"></param>
        private void RDInputChanged(string value = null)
        {
            if (ignoreChangeEvents) return;

            if (rdX.text == "") rdX.text = "0";
            if (rdY.text == "") rdY.text = "0";
            if (napZ.text == "") napZ.text = "0";

            rdCoordinates.x = double.Parse(MakeInputParsable(rdX.text), CultureInfo.InvariantCulture);
            rdCoordinates.y = double.Parse(MakeInputParsable(rdY.text), CultureInfo.InvariantCulture);
            rdCoordinates.z = double.Parse(MakeInputParsable(napZ.text), CultureInfo.InvariantCulture);

            transformableTarget.transform.position = CoordConvert.RDtoUnity(rdCoordinates);
        }
    }
}