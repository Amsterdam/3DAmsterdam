using Netherlands3D.Cameras;
using Netherlands3D.T3D.Uitbouw;
using Netherlands3D.T3D.Uitbouw.BoundaryFeatures;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// - Update the selected icon
/// - Do raycast to Uitbouw, when hit, place the component is real size on the selected Uitbouw wall
/// </summary>

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class HandleDragContainer : MonoBehaviour
    {
        LibraryComponentSelectedEvent.LibraryEventArgsType DragType;

        public BoundaryFeature ComponentObject;
        public Image ComponentImage;
        public int MouseXOffset = 92;

        private BoundaryFeature placedBoundaryFeature;
        public bool isTopComponent;

        private SelectableLibraryItem selectableLibraryItem;
        private Material selectMaterial;
        private Vector2 selectMaterialTextureScale;
        private UitbouwMuur previousSelectedWall;
        private Material wallOriginalMaterial;
        private Vector2 wallOriginalMaterialTextureScale;

        private void OnEnable()
        {
            LibraryComponentSelectedEvent.OnComponentSelectedEvent += LibraryComponentSelected;
            LibraryComponentSelectedEvent.OnMaterialSelectedEvent += LibraryComponentSelectedEvent_OnMaterialSelectedEvent;
        }

        private void LibraryComponentSelectedEvent_OnMaterialSelectedEvent(object source, LibraryComponentSelectedEvent.LibraryEventArgs args)
        {
            DragType = args.Type;
            var e = (LibraryComponentSelectedEvent.LibraryMaterialSelectedEventargs)args;
            ComponentImage.sprite = e.Sprite;
            //ComponentImage.SetNativeSize();
            ComponentImage.rectTransform.sizeDelta = new Vector2(50f, 50f);
            ComponentImage.pixelsPerUnitMultiplier = 2f;
            selectableLibraryItem = e.SelectableLibraryItem;
            selectMaterial = e.ComponentMaterial;
            selectMaterialTextureScale = e.TextureScale;
            //throw new NotImplementedException();
        }

        private void OnDisable()
        {
            LibraryComponentSelectedEvent.OnComponentSelectedEvent -= LibraryComponentSelected;
            LibraryComponentSelectedEvent.OnMaterialSelectedEvent -= LibraryComponentSelectedEvent_OnMaterialSelectedEvent;
        }

        private void LibraryComponentSelected(object source, LibraryComponentSelectedEvent.LibraryEventArgs args)
        {
            //if (args.Type == LibraryComponentSelectedEvent.LibraryEventArgsType.BoundaryFeature)
            //{
            DragType = args.Type;
            var e = (LibraryComponentSelectedEvent.LibraryComponentSelectedEventargs)args;
            ComponentImage.sprite = e.Sprite;
            ComponentImage.SetNativeSize();
            ComponentImage.pixelsPerUnitMultiplier = 1f;
            isTopComponent = e.IsTopComponent;
            ComponentObject = e.ComponentObject;
            selectableLibraryItem = e.SelectableLibraryItem;

            ComponentObject.SetSize(new Vector2(e.ComponentWidth - 0.0001f, e.ComponentHeight - 0.0001f));
            //ComponentObject.transform.localScale = new Vector3(e.ComponentWidth, e.ComponentHeight, 1);
            //}
        }

        private void Update()
        {
            if (!Input.GetMouseButton(0))
            {
                placedBoundaryFeature = null;
                selectableLibraryItem.Deslect();

                if (previousSelectedWall)
                {
                    wallOriginalMaterial = previousSelectedWall.Material;
                    wallOriginalMaterialTextureScale = previousSelectedWall.TextureScale;
                }
                return;
            }

            ProcessMousePosition();
        }

        private void ProcessMousePosition()
        {
            bool casted = MouseRaycast(out var hit);
            if (!casted)
            {
                ResetContainer();
                return;
            }

            var wall = hit.transform.GetComponent<UitbouwMuur>();
            if (!wall)
            {
                ResetContainer();
                return;
            }

            bool allowed = (isTopComponent && wall.Side == WallSide.Top) || (!isTopComponent && (wall.Side == WallSide.Left || wall.Side == WallSide.Right || wall.Side == WallSide.Front || wall.Side == WallSide.Back));
            if (!allowed)
            {
                ResetContainer();
                return;
            }

            switch (DragType)
            {
                case LibraryComponentSelectedEvent.LibraryEventArgsType.BoundaryFeature:
                    PlaceBoundaryFeature(hit.point, hit.transform.rotation, wall);
                    break;
                case LibraryComponentSelectedEvent.LibraryEventArgsType.Material:
                    
                    if (wallOriginalMaterial == null)
                    {
                        wallOriginalMaterial = wall.Material;
                        wallOriginalMaterialTextureScale = wall.TextureScale;
                    }

                    if (wall != previousSelectedWall && previousSelectedWall != null)
                    {
                        previousSelectedWall.SetMaterial(wallOriginalMaterial, wallOriginalMaterialTextureScale);
                        wallOriginalMaterial = wall.Material;
                        wallOriginalMaterialTextureScale = wall.TextureScale;
                    }

                    if (wall.Material != selectMaterial)
                    {
                        wallOriginalMaterial = wall.Material;
                        wallOriginalMaterialTextureScale = wall.TextureScale;
                        wall.SetMaterial(selectMaterial, selectMaterialTextureScale);
                    }
                    break;
            }

            previousSelectedWall = wall;
        }

        private void ResetContainer()
        {
            if (placedBoundaryFeature != null)
            {
                RemoveBoundaryFeatureFromSaveData(placedBoundaryFeature);
            }
            if (previousSelectedWall && wallOriginalMaterial)
                previousSelectedWall.SetMaterial(wallOriginalMaterial, wallOriginalMaterialTextureScale);

            ComponentImage.enabled = true;
        }

        private void PlaceBoundaryFeature(Vector3 position, Quaternion rotation, UitbouwMuur wall)
        {

            if (placedBoundaryFeature == null)
            {
                AddBoundaryFeatureToSaveData(ComponentObject.name, wall.transform.parent);
            }

            placedBoundaryFeature.SetWall(wall); //set wall again to update it if the wall changes
            placedBoundaryFeature.transform.position = position;
            placedBoundaryFeature.transform.rotation = rotation;
            ComponentImage.enabled = false;
        }

        private bool MouseRaycast(out RaycastHit hit)
        {
            var screenpoint = Input.mousePosition;

            Ray ray = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.ScreenPointToRay(screenpoint);

            var mask = LayerMask.GetMask("Uitbouw");
            var casted = Physics.Raycast(ray, out hit, Mathf.Infinity, mask);
            return casted;
        }

        private void AddBoundaryFeatureToSaveData(string prefabName, Transform parent)
        {
            var state = State.ActiveState as PlaceBoundaryFeaturesState;
            if (state)
            {
                placedBoundaryFeature = Instantiate(ComponentObject, parent);
                state.AddNewBoundaryFeatureToSaveData(placedBoundaryFeature, prefabName);
            }
        }

        private void RemoveBoundaryFeatureFromSaveData(BoundaryFeature feature)
        {
            feature.DeleteFeature(); //handles all the destruction logic
        }
    }
}
