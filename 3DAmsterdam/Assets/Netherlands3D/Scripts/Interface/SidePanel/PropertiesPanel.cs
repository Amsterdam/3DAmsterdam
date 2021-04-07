﻿using Netherlands3D.BAG;
using Netherlands3D.Cameras;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.SidePanel
{
    public class PropertiesPanel : MonoBehaviour
    {
        [SerializeField]
        private RectTransform movePanelRectTransform;

        [Header("Tabs")]
        [SerializeField]
        private Tab startingActiveTab;

		[SerializeField]
        private Tab customObjectsTab;
        [SerializeField]
        private Tab objectInformationTab;
        [SerializeField]
        private Tab annotationsTab;
        [SerializeField]
        private Tab settingsTab;

        [Header("Animation")]
        [SerializeField]
        private float animationSpeed = 5.0f;
        [SerializeField]
        private float collapsedShift = 300;
        private Coroutine panelAnimation;
        private bool open = true;

        [SerializeField]
        private Transform generatedFieldsRootContainer;
        private Transform targetFieldsContainer;

        [SerializeField]
        private RenderTexture thumbnailRenderTexture;

        public DisplayBAGData displayBagData;

        public static PropertiesPanel Instance = null;

        [SerializeField]
        private Text titleText;

		[Header("Generated field prefabs:")]
        [SerializeField]
        private GameObject thumbnailPrefab;
        [SerializeField]
        private GameObject groupPrefab;
        [SerializeField]
        private GameObject titlePrefab;
        [SerializeField]
        private GameObject textfieldPrefab;
        [SerializeField]
        private GameObject loadingSpinnerPrefab;
        [SerializeField]
        private GameObject labelPrefab;
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
        private TransformPanel transformPanel;

        [Header("Thumbnail rendering")]
        [SerializeField]
        private Camera thumbnailCameraPrefab;
        private Camera thumbnailRenderer;
        [SerializeField]
        private LayerMask renderAllLayersMask;
        [SerializeField]
        private float cameraThumbnailObjectMargin = 0.1f;
        [SerializeField]
        private Material buildingsExclusiveShader;
        [SerializeField]
        private Material defaultBuildingsShader;

        [SerializeField]
        private VerticalLayoutGroup verticalLayoutGroup;

        private GameObject thumbnailImage = null;

        public int ThumbnailExclusiveLayer { get => thumbnailRenderer.gameObject.layer; }

		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}

			//Start with our main container. Groups may change this target.
			targetFieldsContainer = generatedFieldsRootContainer;

            //Our disabled thumbnail rendering camera. (We call .Render() via script to trigger a render)
            thumbnailRenderer = Instantiate(thumbnailCameraPrefab);

            startingActiveTab.OpenTab(true);
            transformPanel.gameObject.SetActive(false);
        }

        private void SetDynamicFieldsTargetContainer(Transform targetContainer)
        {
            generatedFieldsRootContainer = targetContainer;
            targetFieldsContainer = targetContainer;
        }

        /// <summary>
        /// Open the object meta data panel, setting it as a target, to spawn dynamic fields in.
        /// </summary>
        /// <param name="title">Set to change the sidebar title to something custom</param>
        /// <param name="clearOldfields">Clear current generated fields in the container</param>
        /// <param name="spacing">Optional spacing for the vertical layout group</param>
        public void OpenObjectInformation(string title, bool clearOldfields = true, float spacing = 0.0f)
        {
            SetDynamicFieldsTargetContainer(objectInformationTab.TabPanel.FieldsContainer);
            if (clearOldfields) ClearGeneratedFields();

            objectInformationTab.OpenTab();
            verticalLayoutGroup.spacing = spacing;
            OpenPanel();
        }

        /// <summary>
        /// Open the settings tab
        /// </summary>
        public void OpenSettings()
        {
            SetDynamicFieldsTargetContainer(settingsTab.TabPanel.FieldsContainer);
            //Always reload settings, so clear fields first
            ClearGeneratedFields();
            settingsTab.OpenTab();
            OpenPanel();
        }

        /// <summary>
        /// Open the annotations tab
        /// </summary>
        public void OpenAnnotations()
        {
            annotationsTab.OpenTab(true);
            OpenPanel();
        }

        /// <summary>
        /// Open the custom objects tab
        /// </summary>
        /// <param name="setTargetTransformable">Optional target transformable to select by default</param>
        /// <param name="gizmoTransformType">The type of transformation we are doing to show the right gizmo type</param>
        public void OpenCustomObjects(Transformable setTargetTransformable = null, int gizmoTransformType = -1)
        {
            customObjectsTab.OpenTab(true);

            if (setTargetTransformable)
                OpenTransformPanel(setTargetTransformable, gizmoTransformType);

            OpenPanel();
        }
        public void OpenTransformPanel(Transformable transformable, int gizmoTransformType = -1)
        {
            transformPanel.SetTarget(transformable);
            switch (gizmoTransformType)
            {
                case 0:
                    transformPanel.TranslationGizmo();
                    break;
                case 1:
                    transformPanel.RotationGizmo();
                    break;
                case 2:
                    transformPanel.ScaleGizmo();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Slide the panel open (if it is closed)
        /// </summary>
        /// <param name="title">Title to show on top of the panel</param>
        public void OpenPanel(string title = "")
        {
            if (title != "")
            {
                titleText.text = title;
            }
            open = true;

            if (panelAnimation != null) StopCoroutine(panelAnimation);
            panelAnimation = StartCoroutine(Animate());
		}

        public void ClosePanel()
        {
            open = false;
            if (panelAnimation != null) StopCoroutine(panelAnimation);
            panelAnimation = StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            while (open && movePanelRectTransform.anchoredPosition.x > 0)
            {
                movePanelRectTransform.anchoredPosition = Vector3.Lerp(movePanelRectTransform.anchoredPosition, Vector3.zero, animationSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
            while (!open && movePanelRectTransform.anchoredPosition.x < collapsedShift)
            {
                movePanelRectTransform.anchoredPosition = Vector3.Lerp(movePanelRectTransform.anchoredPosition, Vector3.right * collapsedShift, animationSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }

        /// <summary>
        /// Deselect the currently selected transformable
        /// </summary>
        /// <param name="transformable">Optional specific transformable reference. Only deselects if transformable matches.</param>
        public void DeselectTransformable(Transformable transformable = null, bool disableContainerPanel = false)
        {
            if (transformable == null || (transformable != null && transformPanel.TransformableTarget == transformable))
            {
                Selector.Instance.ClearHighlights();
                transformPanel.DisableGizmo();
                Transformable.lastSelectedTransformable = null;
                transformPanel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Render a thumbnail containing the given world points
        /// </summary>
        /// <param name="points">The points that should be framed within the thumbnail</param>
        /// <param name="swapBuildingShaders">Render all layers or exclusively the selected buildings</param>
        public void RenderThumbnailContaining(Vector3[] points, ThumbnailRenderMethod thumbnailRenderMethod = ThumbnailRenderMethod.SAME_LAYER_AS_THUMBNAIL_CAMERA)
        {
            //Find our centroid
            var centroid = new Vector3(0, 0, 0);
            var totalPoints = points.Length;
            foreach (var point in points)
            {
                centroid += point;
            }
            centroid /= totalPoints;
            Bounds bounds = new Bounds(centroid, Vector3.zero);

            //Expand our bounds
            foreach (Vector3 point in points)
            {
                bounds.Encapsulate(point);
            }
            RenderThumbnailContaining(bounds, thumbnailRenderMethod);
        }

        /// <summary>
        /// Render a thumbnail containing the given bounds
        /// </summary>
        /// <param name="bounds">The bounds object that should be framed in the thumbnail</param>
        /// <param name="swapBuildingShaders">Render all layers or exclusively the selected buildings</param>
		public void RenderThumbnailContaining(Bounds bounds, ThumbnailRenderMethod thumbnailRenderMethod = ThumbnailRenderMethod.SAME_LAYER_AS_THUMBNAIL_CAMERA)
        {
            var objectSizes = bounds.max - bounds.min;
            var objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
            var cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * thumbnailRenderer.fieldOfView);
            var distance = objectSize / cameraView;
            distance += cameraThumbnailObjectMargin * objectSize;

            thumbnailRenderer.transform.position = bounds.center - (distance * Vector3.forward) + (distance * Vector3.up);
            thumbnailRenderer.transform.LookAt(bounds.center);

            RenderThumbnail(thumbnailRenderMethod);
        }

        /// <summary>
        /// Render thumbnail from previous position
        /// </summary>
        /// <param name="swapBuildingShaders">Swap building shaders for one frame, so we can exclusively draw highlighted buildings</param>
		public void RenderThumbnail(ThumbnailRenderMethod thumbnailRenderMethod = ThumbnailRenderMethod.SAME_LAYER_AS_THUMBNAIL_CAMERA)
		{
			switch (thumbnailRenderMethod)
			{
				case ThumbnailRenderMethod.SAME_AS_MAIN_CAMERA:
                    thumbnailRenderer.cullingMask = CameraModeChanger.Instance.ActiveCamera.cullingMask;
                    break;
				case ThumbnailRenderMethod.HIGHLIGHTED_BUILDINGS:
                    thumbnailRenderer.cullingMask = renderAllLayersMask.value;
                    break;
				case ThumbnailRenderMethod.SAME_LAYER_AS_THUMBNAIL_CAMERA:
                    thumbnailRenderer.cullingMask = thumbnailCameraPrefab.cullingMask;
                    break;
				default:
					break;
			}

            //Switchs shaders temporarily on buildings to exclusively draw the highlighted ones
			if (thumbnailRenderMethod == ThumbnailRenderMethod.HIGHLIGHTED_BUILDINGS)
            {
                var renderersOnBuildingsLayer = FindObjectsOfType<Renderer>().Where(c => c.gameObject.layer == LayerMask.NameToLayer("Buildings")).ToArray();
                foreach (var renderer in renderersOnBuildingsLayer)
                {
                    renderer.material.shader = buildingsExclusiveShader.shader;
                    Debug.Log("Swapping " + renderer.gameObject.name);
                }
                thumbnailRenderer.Render();

                foreach (var renderer in renderersOnBuildingsLayer)
                    renderer.material.shader = defaultBuildingsShader.shader;
            }
            else
            {
                thumbnailRenderer.Render();
            }

            //We only allow one thumbnail to be in our properties panel
            if(!thumbnailImage)
                AddThumbnail();
        }

        public void AddThumbnail()
        {
            thumbnailImage = Instantiate(thumbnailPrefab, targetFieldsContainer);
            thumbnailImage.transform.SetAsFirstSibling();
        }

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
            SAME_LAYER_AS_THUMBNAIL_CAMERA
		}

        #region Methods for generating the main field types (spawning prefabs)
        public void AddTitle(string titleText)
        {
            Instantiate(titlePrefab, targetFieldsContainer).GetComponent<Text>().text = titleText;
        }
        public void AddTextfield(string content)
        {
            Instantiate(textfieldPrefab, targetFieldsContainer).GetComponent<Text>().text = content;
        }
        public void AddLabel(string labelText)
        {
            Instantiate(labelPrefab, targetFieldsContainer).GetComponent<Text>().text = labelText;
        }

        public void AddLoadingSpinner()
        {
            Instantiate(loadingSpinnerPrefab, targetFieldsContainer);
        }

        public DataKeyAndValue AddDataField(string keyTitle, string valueText)
        {
            DataKeyAndValue dataKeyAndValue = Instantiate(dataFieldPrefab, targetFieldsContainer);
            dataKeyAndValue.SetTexts(keyTitle, valueText);
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
            Instantiate(urlPrefab, targetFieldsContainer).SetURL(urlText,urlPath);
        }
        public void AddActionButtonText(string buttonText, Action<string> clickAction)
        {
            Instantiate(buttonTextPrefab, targetFieldsContainer).SetAction(buttonText,clickAction);
        }
        public void AddActionButtonBig(string buttonText, Action<string> clickAction)
        {
            Instantiate(buttonBigPrefab, targetFieldsContainer).SetAction(buttonText, clickAction);
        }
        public void AddActionSlider(string minText, string maxText, float minValue, float maxValue, float defaultValue, Action<float> changeAction, bool wholeNumberSteps = false)
        {
            Instantiate(sliderPrefab, targetFieldsContainer).SetAction(minText, maxText, minValue, maxValue, defaultValue, changeAction, wholeNumberSteps);
        }

        public void AddActionDropdown(string[] dropdownOptions, Action<string> selectOptionAction, string selected = "")
        {
            Instantiate(dropdownPrefab, targetFieldsContainer).SetAction(dropdownOptions, selectOptionAction, selected);
        }

        public void AddActionCheckbox(string buttonText, bool checkedBox, Action<bool> checkAction)
        {
            Instantiate(checkboxPrefab, targetFieldsContainer).SetAction(buttonText, checkedBox, checkAction);
        }
        public void AddSelectionOutliner(GameObject linkedGameObject, string title, string id = "")
        {
            Instantiate(selectionOutlinerPrefab, targetFieldsContainer).Link(linkedGameObject,title,id);
        }
        public void AddCustomPrefab(GameObject prefab)
        {
            Instantiate(prefab, targetFieldsContainer);
        }
        public void ClearGeneratedFields()
        {
            thumbnailImage = null;
            foreach (Transform field in generatedFieldsRootContainer)
            {
                Destroy(field.gameObject);
            }
        }
        #endregion
    }
}