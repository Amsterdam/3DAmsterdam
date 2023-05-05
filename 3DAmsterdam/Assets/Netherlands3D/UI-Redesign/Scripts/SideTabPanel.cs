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
    public enum Panels
    {
        Search,
        Layer,
        SunTime,
        Car,
        Timeline,
        Measure,
        Viewpoints,
        Comments,
        Underground,
        Profile
    }

    public class SideTabPanel : MonoBehaviour
    {
        public static SideTabPanel Instance = null;
        private TransformPanel transformPanel;

        [SerializeField]
        private RectTransform movePanelRectTransform;

        [Header("Main Tabs")]
        [SerializeField]
        private Tabv2 startingActiveTab;

        //Main tabs
        [SerializeField]
        private Tabv2 objectInformationTab;
        [SerializeField]
        private Tabv2 layersTab;
        [SerializeField]
        private Tabv2 searchTab;
        [SerializeField]
        private Tabv2 settingsTab;
        [SerializeField]
        private Tabv2 sunTimeTab;

        [Header("Sub Tabs")]
        //Sub tabs
        [SerializeField]
        private Tabv2 profileTab;
        [SerializeField]
        private Tabv2 viewpointsTab;
        [SerializeField]
        private Tabv2 annotationsTab;


        [Header("Animation")]
        [SerializeField]
        private float animationSpeed = 5.0f;
        [SerializeField]
        private float collapsedShift = 300;
        private Coroutine panelAnimation;
        public bool open = true;


        [Header("Dynamic")]
        [SerializeField]
        private Transform generatedFieldsRootContainer;
        private Transform targetFieldsContainer;

        [SerializeField]
        private RenderTexture thumbnailRenderTexture;

        public DisplayBAGData displayBagData;
   
        [Header("Thumbnail rendering")]
        [SerializeField]
        private GameObject thumbnailPrefab;

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
        

		void Start()
		{
            //Find the transformpanel
            transformPanel = FindObjectOfType<TransformPanel>();

            if (Instance == null)
			{
				Instance = this;
			}

            //Open/closed at start
            if (startingActiveTab)
            {
                startingActiveTab.OpenTab(true);
            }
            else{
                movePanelRectTransform.anchoredPosition = Vector3.right * collapsedShift;
            }

            //Start with our main container. Groups may change this target.
            targetFieldsContainer = generatedFieldsRootContainer;

            //Our disabled thumbnail rendering camera. (We call .Render() via script to trigger a render)
            thumbnailRenderer = Instantiate(thumbnailCameraPrefab);

            transformPanel.gameObject.SetActive(false);
        }

		public void SetDynamicFieldsTargetContainer(Transform targetContainer)
        {
            generatedFieldsRootContainer = targetContainer;
            targetFieldsContainer = targetContainer;
        }

        public void OpenPanel(Panels panel)
        {
            switch (panel)
            {
                case Panels.Search:
                    searchTab.OpenTab(true);
                    break;
                case Panels.Layer:
                    layersTab.OpenTab(true);
                    break;
                case Panels.SunTime:
                    sunTimeTab.OpenTab(true);   
                    break;
                case Panels.Car:
                    //
                    break;
                case Panels.Timeline:
                    //
                    break;
                case Panels.Measure:
                    //
                    break;
                case Panels.Viewpoints:
                    viewpointsTab.OpenTab(true);   
                    break;
                case Panels.Comments:
                    break;
                case Panels.Underground:
                    break;
                case Panels.Profile:
                    profileTab.OpenTab(true);
                    break;

            }
        }

        /// <summary>
        /// Open the object meta data panel, setting it as a target, to spawn dynamic fields in.
        /// </summary>
        /// <param name="title">Set to change the sidebar title to something custom</param>
        /// <param name="clearOldfields">Clear current generated fields in the container</param>
        /// <param name="spacing">Optional spacing for the vertical layout group</param>
        public void OpenObjectInformation(bool clearOldfields = true, float spacing = 0.0f)
        {
            //SetDynamicFieldsTargetContainer(objectInformationTab.TabPanel.FieldsContainer);
            if (clearOldfields) ClearGeneratedFields();

            objectInformationTab.OpenTab(true);
            //verticalLayoutGroup.spacing = spacing;
        }

        /// <summary>
        /// Open the settings tab
        /// </summary>
        public void OpenSettings()
        {
            //SetDynamicFieldsTargetContainer(settingsTab.TabPanel.FieldsContainer);
            //Always reload settings, so clear fields first
            ClearGeneratedFields();
            settingsTab.OpenTab(true);
        }

        /// <summary>
        /// Open the static tile layers tab
        /// </summary>
        public void OpenLayers()
        {
            layersTab.OpenTab(true);
        }


        /// <summary>
        /// Open the custom objects tab
        /// </summary>
        /// <param name="setTargetTransformable">Optional target transformable to select by default</param>
        /// <param name="gizmoTransformType">The type of transformation we are doing to show the right gizmo type</param>
        public void OpenCustomObjects(Transformable setTargetTransformable = null, int gizmoTransformType = -1)
        {
            layersTab.OpenTab(true);

            if (setTargetTransformable)
                OpenTransformPanel(setTargetTransformable, gizmoTransformType);
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
        public void OpenPanel(TabPanelv2 panel)
        {
            open = true;

            CloseAllTabs();
            panel.gameObject.SetActive(true);

            if (panelAnimation != null) StopCoroutine(panelAnimation);

            if(this.gameObject.activeInHierarchy)
                panelAnimation = StartCoroutine(Animate());
		}

        private void CloseAllTabs()
        {
            objectInformationTab.TabPanel.gameObject.SetActive(false);
            layersTab.TabPanel.gameObject.SetActive(false);
            searchTab.TabPanel.gameObject.SetActive(false);
            //settingsTab.TabPanel.gameObject.SetActive(false);
            searchTab.TabPanel.gameObject.SetActive(false);
            sunTimeTab.TabPanel.gameObject.SetActive(false);
            profileTab.TabPanel.gameObject.SetActive(false);
            viewpointsTab.TabPanel.gameObject.SetActive(false);
            annotationsTab.TabPanel.gameObject.SetActive(false);
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
                if(transformPanel.gameObject)
                    transformPanel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Render a thumbnail containing the given world points
        /// </summary>
        /// <param name="points">The points that should be framed within the thumbnail</param>
        /// <param name="swapBuildingShaders">Render all layers or exclusively the selected buildings</param>
        public void RenderThumbnailContaining(Vector3[] points, ThumbnailRenderMethod thumbnailRenderMethod = ThumbnailRenderMethod.SAME_LAYER_AS_THUMBNAIL_CAMERA, Vector3 cameraSnapshotFromPosition = default)
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
            RenderThumbnailContaining(bounds, thumbnailRenderMethod, cameraSnapshotFromPosition);
        }

        /// <summary>
        /// Render a thumbnail containing the given bounds
        /// </summary>
        /// <param name="bounds">The bounds object that should be framed in the thumbnail</param>
        /// <param name="thumbnailRenderMethod">What type of snapshot rendering behaviour do we want</param>
        /// 
		public void RenderThumbnailContaining(Bounds bounds, ThumbnailRenderMethod thumbnailRenderMethod = ThumbnailRenderMethod.SAME_LAYER_AS_THUMBNAIL_CAMERA, Vector3 cameraSnapshotFromPosition = default)
        {
            var objectSizes = bounds.max - bounds.min;
            var objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
            var cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * thumbnailRenderer.fieldOfView);
            var distance = objectSize / cameraView;
            distance += cameraThumbnailObjectMargin * objectSize;

            thumbnailRenderer.transform.position = (cameraSnapshotFromPosition == Vector3.zero) ? bounds.center - (distance * Vector3.forward) + (distance * Vector3.up) : cameraSnapshotFromPosition;
            thumbnailRenderer.transform.LookAt(bounds.center);

            RenderThumbnail(thumbnailRenderMethod, bounds);
        }

        /// <summary>
        /// Render thumbnail from previous position
        /// </summary>
        /// <param name="swapBuildingShaders">Swap building shaders for one frame, so we can exclusively draw highlighted buildings</param>
		public void RenderThumbnail(ThumbnailRenderMethod thumbnailRenderMethod = ThumbnailRenderMethod.SAME_LAYER_AS_THUMBNAIL_CAMERA, Bounds bounds = default)
		{
            thumbnailRenderer.orthographic = false;

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
                case ThumbnailRenderMethod.ORTOGRAPHIC:
                    thumbnailRenderer.cullingMask = CameraModeChanger.Instance.ActiveCamera.cullingMask;
                    thumbnailRenderer.orthographic = true;
                    var ortoSize = (Math.Max(bounds.size.x, bounds.size.z) / 2.0f);
                    thumbnailRenderer.orthographicSize = ortoSize + (ortoSize*cameraThumbnailObjectMargin);
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

        public enum ThumbnailRenderMethod
        {
            SAME_AS_MAIN_CAMERA,
            HIGHLIGHTED_BUILDINGS,
            SAME_LAYER_AS_THUMBNAIL_CAMERA,
            ORTOGRAPHIC
        }

        private void ClearGeneratedFields(GameObject ignoreGameObject, List<GameObject> ignoreGameObjects)
        {
            thumbnailImage = null;
            foreach (Transform field in generatedFieldsRootContainer)
            {
                if(ignoreGameObject != null && (field.gameObject == ignoreGameObject || ignoreGameObject.transform.IsChildOf(field))) continue;
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
            ClearGeneratedFields(null,null);
        }
    }
}