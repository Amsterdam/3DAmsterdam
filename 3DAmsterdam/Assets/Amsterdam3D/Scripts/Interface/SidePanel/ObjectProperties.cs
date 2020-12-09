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

        private Camera thumbnailRenderer;

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

			CreateThumbnailRenderCamera();
		}

		private void CreateThumbnailRenderCamera()
		{
			//Our render camera for thumbnails. 
            //We disable it so we cant manualy render a single frame using Camera.Render();
			thumbnailRenderer = new GameObject().AddComponent<Camera>();
			thumbnailRenderer.fieldOfView = 30;
			thumbnailRenderer.farClipPlane = 5000;
			thumbnailRenderer.targetTexture = thumbnailRenderTexture;
			thumbnailRenderer.enabled = false;
		}

		public void OpenPanel(string title)
        {
            ClearGeneratedFields();

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
            //Jiggle our contentsize fitter to force a resize.
            targetFieldsContainer.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            targetFieldsContainer = generatedFieldsRootContainer;
        }

        #region Methods for generating the main field types (spawning prefabs)
        public void AddTitle(string titleText)
        {
            Instantiate(titlePrefab, targetFieldsContainer).GetComponent<Text>().text = titleText;
        }
        public DataKeyAndValue AddDataField(string keyTitle, string valueText)
        {
            DataKeyAndValue dataKeyAndValue = Instantiate(dataFieldPrefab, generatedFieldsRootContainer);
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