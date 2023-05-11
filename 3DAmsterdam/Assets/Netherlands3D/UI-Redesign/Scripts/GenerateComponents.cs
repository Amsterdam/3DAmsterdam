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

namespace Netherlands3D.Interface.SidePanel
{
    public class GenerateComponents : MonoBehaviour
    {
        [SerializeField]
        private Transform generatedFieldsRootContainer;
        private Transform targetFieldsContainer;

        public static GenerateComponents Instance = null;

        [Header("Generated field prefabs:")]
        [SerializeField]
        private GameObject thumbnailPrefab;
        [SerializeField]
        private GameObject groupPrefab;
        [SerializeField]
        private GameObject titlePrefab;
        [SerializeField]
        private GameObject numberInputPrefab;
        [SerializeField]
        private GameObject textfieldPrefab;
        [SerializeField]
        private GameObject textfieldPrefabColor;
        [SerializeField]
        private GameObject loadingSpinnerPrefab;
        [SerializeField]
        private GameObject labelPrefab;
        [SerializeField]
        private GameObject labelPrefabColor;
        [SerializeField]
        private DataKeyAndValue dataFieldPrefab;
        [SerializeField]
        private GameObject seperatorLinePrefab;
        [SerializeField]
        private GameObject spacerPrefab;
        [SerializeField]
        private SelectionOutliner selectionOutlinerPrefab;
        [SerializeField]
        private NameAndURL urlPrefab;
        [SerializeField]
        private ActionButton buttonTextPrefab;
        [SerializeField]
        private ActionButton buttonBigPrefab;
        [SerializeField]
        private ActionSlider sliderPrefab;
        [SerializeField]
        private ActionDropDown dropdownPrefab;
        [SerializeField]
        private ActionCheckbox checkboxPrefab;
        [SerializeField]
        private Button gradientButtonPrefab;

        private TransformPanel transformPanel;
        [SerializeField]
        private TMP_InputField selectableText;

        /// <summary>
        /// Create a grouped field. All visuals added will be added to this group untill CloseGroup() is called.
        /// </summary>
        /// <returns>The new group object</returns>
        public GameObject CreateGroup()
        {
            GameObject newGroup = Instantiate(groupPrefab, targetFieldsContainer);
            targetFieldsContainer = newGroup.transform;
            return newGroup;
        }

        /// <summary>
        /// Closes the adding of items to this group, changing the target container to the root again.
        /// </summary>
        public void CloseGroup()
        {
            //Force a layout refresh on our group so the contentsizefitter scales according to the children again
            LayoutRebuilder.ForceRebuildLayoutImmediate(targetFieldsContainer.GetComponent<RectTransform>());

            targetFieldsContainer = generatedFieldsRootContainer;
        }
        public enum ThumbnailRenderMethod
        {
            SAME_AS_MAIN_CAMERA,
            HIGHLIGHTED_BUILDINGS,
            SAME_LAYER_AS_THUMBNAIL_CAMERA,
            ORTOGRAPHIC
        }

        #region Methods for generating the main field types (spawning prefabs)
        public void AddTitle(string titleText, bool selectable = false)
        {
            var newTitleField = Instantiate(titlePrefab, targetFieldsContainer);
            newTitleField.GetComponentInChildren<TextMeshProUGUI>().text = titleText;

            if (selectable)
            {
                MakeTextItemSelectable(newTitleField);
            }
        }

        private void MakeTextItemSelectable(GameObject textGameObject)
        {
            textGameObject.AddComponent<SelectableText>().SetFieldPrefab(selectableText);
        }

        public TMP_InputField AddNumberInput(string label, double defaultValue)
        {
            var newNumberInputField = Instantiate(numberInputPrefab, targetFieldsContainer);
            TMP_InputField inputField = newNumberInputField.GetComponent<TMP_InputField>();
            inputField.SetTextWithoutNotify(defaultValue.ToString());

            var allTexts = inputField.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in allTexts)
                if (text != inputField.textComponent) text.text = label;

            return inputField;
        }

        public Button AddGradientButton(string gradientName, GradientContainer gradient)
        {
            Button gradientButton = Instantiate(gradientButtonPrefab, targetFieldsContainer);
            gradientButton.name = gradientName;
            gradientButton.GetComponentInChildren<GradientImage>().SetGradient(gradient);
            gradientButton.gameObject.AddComponent<TooltipTrigger>().TooltipText = gradientName;
            return gradientButton;
        }

        public void AddTextfield(string content, bool selectable = false)
        {
            var newTextField = Instantiate(textfieldPrefab, targetFieldsContainer);
            newTextField.GetComponentInChildren<TextMeshProUGUI>().text = content;

            if (selectable)
            {
                MakeTextItemSelectable(newTextField);
            }
        }
        public void AddLabel(string labelText)
        {
            Instantiate(labelPrefab, targetFieldsContainer).GetComponentInChildren<TextMeshProUGUI>().text = labelText;
        }

        public void AddLabelColor(string text, Color color, TMPro.FontStyles style)
        {
            var gam = Instantiate(labelPrefabColor, targetFieldsContainer);
            var textComponent = gam.GetComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.color = color;
            textComponent.fontStyle = style;
        }

        public void AddTextfieldColor(string text, Color color, TMPro.FontStyles style)
        {
            var gam = Instantiate(textfieldPrefabColor, targetFieldsContainer);
            var textComponent = gam.GetComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.color = color;
            textComponent.fontStyle = style;
        }

        public void AddLoadingSpinner()
        {
            Instantiate(loadingSpinnerPrefab, targetFieldsContainer);
        }

        public DataKeyAndValue AddDataField(string keyTitle, string valueText, bool selectableValueText = true)
        {
            DataKeyAndValue dataKeyAndValue = Instantiate(dataFieldPrefab, targetFieldsContainer);
            dataKeyAndValue.SetTexts(keyTitle, valueText);

            if (selectableValueText)
            {
                MakeTextItemSelectable(dataKeyAndValue.ValueText.gameObject);
            }

            return dataKeyAndValue;
        }
        public void AddSeperatorLine()
        {
            Instantiate(seperatorLinePrefab, targetFieldsContainer);
        }
        public void AddSpacer(float height = 5.0f)
        {
            Instantiate(spacerPrefab, targetFieldsContainer).GetComponent<RectTransform>().sizeDelta = new Vector2(100, height);
        }
        public void AddLink(string urlText, string urlPath)
        {
            var nameAndUrl = Instantiate(urlPrefab, targetFieldsContainer);
            nameAndUrl.SetURL(urlText, urlPath);
        }
        public void AddActionButtonText(string buttonText, Action<string> clickAction)
        {
            var button = Instantiate(buttonTextPrefab, targetFieldsContainer);
            button.SetAction(buttonText, clickAction);
        }
        public ActionButton AddActionButtonBig(string buttonText, Action<string> clickAction)
        {
            ActionButton actionButton = Instantiate(buttonBigPrefab, targetFieldsContainer);
            actionButton.SetAction(buttonText, clickAction);
            return actionButton;
        }
        public void AddActionSlider(string minText, string maxText, float minValue, float maxValue, float defaultValue, Action<float> changeAction, bool wholeNumberSteps = false, string description = "")
        {
            Instantiate(sliderPrefab, targetFieldsContainer).SetAction(minText, maxText, minValue, maxValue, defaultValue, changeAction, wholeNumberSteps, description);
        }

        public ActionDropDown AddActionDropdown(string[] dropdownOptions, Action<string> selectOptionAction, string selected = "")
        {
            var actionDropdown = Instantiate(dropdownPrefab, targetFieldsContainer);
            actionDropdown.SetAction(dropdownOptions, selectOptionAction, selected);
            return actionDropdown;
        }

        public void AddActionCheckbox(string buttonText, bool checkedBox, Action<bool> checkAction)
        {
            Instantiate(checkboxPrefab, targetFieldsContainer).SetAction(buttonText, checkedBox, checkAction);
        }
        public void AddSelectionOutliner(GameObject linkedGameObject, string title, string id = "")
        {
            Instantiate(selectionOutlinerPrefab, targetFieldsContainer).Link(linkedGameObject, title, id);
        }
        public void AddCustomPrefab(GameObject prefab)
        {
            Instantiate(prefab, targetFieldsContainer);
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
        #endregion
    }
}
