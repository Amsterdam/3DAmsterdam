using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
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
        private NameAndURL urlPrefab;
        [SerializeField]
        private TransformPanel transformPanelPrefab;

        [SerializeField]
        private Camera thumbnailCameraPrefab;
        private Camera thumbnailRenderer;

        [SerializeField]
        private float cameraThumbnailObjectMargin = 0.1f;

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

        public void AddTransformPanel(GameObject transformable)
        {
            TransformPanel transformPanel = Instantiate(transformPanelPrefab, targetFieldsContainer);
            transformPanel.SetTarget(transformable);
        }

        public void OpenPanel(string title, bool clearOldfields = true)
        {
            if(clearOldfields) ClearGeneratedFields();

            objectPropertiesPanel.SetActive(true);
            titleText.text = title;
        }
        public void ClosePanel()
        {
            objectPropertiesPanel.SetActive(false);
        }

        public void RenderThumbnailFromPosition(Vector3 from, Vector3 to)
        {
            thumbnailRenderer.transform.position = from;
            thumbnailRenderer.transform.LookAt(to);
            
            thumbnailRenderer.Render();
        }

        
        public void RenderThumbnailContaining(Vector3[] points)
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
            RenderThumbnailContaining(bounds);
        }


		public void RenderThumbnailContaining(Bounds bounds)
        {
            var objectSizes = bounds.max - bounds.min;
            var objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
            var cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * thumbnailRenderer.fieldOfView);
            var distance = objectSize / cameraView;
            distance += cameraThumbnailObjectMargin * objectSize;

            thumbnailRenderer.transform.position = bounds.center - (distance * Vector3.forward) + (distance * Vector3.up);
            thumbnailRenderer.transform.LookAt(bounds.center);
            thumbnailRenderer.Render();
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
        public void AddURLText(string urlText, string urlPath)
        {
            Instantiate(urlPrefab, targetFieldsContainer).SetURL(urlText,urlPath);
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