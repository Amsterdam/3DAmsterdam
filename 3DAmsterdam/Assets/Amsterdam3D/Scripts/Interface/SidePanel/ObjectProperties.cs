using Amsterdam3D.CameraMotion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class ObjectProperties : MonoBehaviour
    {
        [SerializeField]
        private GameObject objectPropertiesPanel;
        [SerializeField]
        private Transform generatedFieldsRootContainer;

        private Transform targetFieldsContainer;

        [SerializeField]
        private RenderTexture thumbnailRenderTexture;

        public DisplayBAGData displayBagData;

        public static ObjectProperties Instance = null;

        [SerializeField]
        private Text titleText;

        [Header("Generated field prefabs:")]
        [SerializeField]
        private GameObject groupPrefab;
        [SerializeField]
        private GameObject titlePrefab;
        [SerializeField]
        private DataKeyAndValue dataFieldPrefab;
        [SerializeField]
        private GameObject seperatorLinePrefab;
        [SerializeField]
        private SelectionOutliner selectionOutlinerPrefab;
        [SerializeField]
        private NameAndURL urlPrefab;
        [SerializeField]
        private ActionButton buttonPrefab;
        [SerializeField]
        private TransformPanel transformPanelPrefab;

        private TransformPanel currentTransformPanel;

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

        public int ThumbnailExclusiveLayer { get => thumbnailRenderer.gameObject.layer; }

		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}

			//Start with our main container. Groups may change this target.
			targetFieldsContainer = generatedFieldsRootContainer;

			//Properties panel is disabled at startup
			objectPropertiesPanel.SetActive(false);

            //Our disabled thumbnail rendering camera. (We call .Render() via script to trigger a render)
            thumbnailRenderer = Instantiate(thumbnailCameraPrefab);
        }

        public void OpenTransformPanel(Transformable transformable, int gizmoTransformType = -1)
        {
            currentTransformPanel = Instantiate(transformPanelPrefab, targetFieldsContainer);
            currentTransformPanel.SetTarget(transformable);

			switch (gizmoTransformType)
			{
                case 0:
                    currentTransformPanel.TranslationGizmo();
                    break;
                case 1:
                    currentTransformPanel.RotationGizmo();
                    break;
                case 2:
                    currentTransformPanel.ScaleGizmo();
                    break;
				default:
					break;
			}
		}

        public void OpenPanel(string title, bool clearOldfields = true)
        {
            if(clearOldfields) ClearGeneratedFields();

            objectPropertiesPanel.SetActive(true);
            titleText.text = title;
        }
        public void ClosePanel()
        {
            DeselectTransformable();
            ClearGeneratedFields();
            objectPropertiesPanel.SetActive(false);
        }

        /// <summary>
        /// Deselect the currently selected transformable
        /// </summary>
        /// <param name="transformable">Optional specific transformable reference. Only deselects if transformable matches.</param>
        public void DeselectTransformable(Transformable transformable = null, bool disableContainerPanel = false)
        {
            if (currentTransformPanel)
            {
                if (transformable == null || (transformable != null && currentTransformPanel.TransformableTarget == transformable))
                {
                    Selector.Instance.ClearHighlights();
                    currentTransformPanel.DisableGizmo();
                    Transformable.lastSelectedTransformable = null;
                    if(disableContainerPanel) objectPropertiesPanel.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Render a thumbnail containing the given world points
        /// </summary>
        /// <param name="points">The points that should be framed within the thumbnail</param>
        /// <param name="swapBuildingShaders">Render all layers or exclusively the selected buildings</param>
        public void RenderThumbnailContaining(Vector3[] points, bool swapBuildingShaders = false)
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
            RenderThumbnailContaining(bounds, swapBuildingShaders);
        }

        /// <summary>
        /// Render a thumbnail containing the given bounds
        /// </summary>
        /// <param name="bounds">The bounds object that should be framed in the thumbnail</param>
        /// <param name="swapBuildingShaders">Render all layers or exclusively the selected buildings</param>
		public void RenderThumbnailContaining(Bounds bounds, bool swapBuildingShaders = false)
        {
            var objectSizes = bounds.max - bounds.min;
            var objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
            var cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * thumbnailRenderer.fieldOfView);
            var distance = objectSize / cameraView;
            distance += cameraThumbnailObjectMargin * objectSize;

            thumbnailRenderer.transform.position = bounds.center - (distance * Vector3.forward) + (distance * Vector3.up);
            thumbnailRenderer.transform.LookAt(bounds.center);

            RenderThumbnail(swapBuildingShaders);
        }

        /// <summary>
        /// Render thumbnail from previous position
        /// </summary>
        /// <param name="swapBuildingShaders">Swap building shaders for one frame, so we can exclusively draw highlighted buildings</param>
		public void RenderThumbnail(bool swapBuildingShaders = false)
		{
            thumbnailRenderer.cullingMask = (swapBuildingShaders) ? renderAllLayersMask.value : thumbnailCameraPrefab.cullingMask;

            if (swapBuildingShaders)
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

        #region Methods for generating the main field types (spawning prefabs)
        public void AddTitle(string titleText)
        {
            Instantiate(titlePrefab, targetFieldsContainer).GetComponent<Text>().text = titleText;
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
        public void AddLink(string urlText, string urlPath)
        {
            Instantiate(urlPrefab, targetFieldsContainer).SetURL(urlText,urlPath);
        }
        public void AddActionButton(string buttonText, Action<string> clickAction)
        {
            Instantiate(buttonPrefab, targetFieldsContainer).SetAction(buttonText,clickAction);
        }

        public void AddSelectionOutliner(GameObject linkedGameObject, string title, string id = "")
        {
            Instantiate(selectionOutlinerPrefab, targetFieldsContainer).Link(linkedGameObject,title,id);
        }
        public void ClearGeneratedFields()
        {
            foreach (Transform field in generatedFieldsRootContainer)
            {
                Destroy(field.gameObject);
            }
        }
        #endregion
    }
}