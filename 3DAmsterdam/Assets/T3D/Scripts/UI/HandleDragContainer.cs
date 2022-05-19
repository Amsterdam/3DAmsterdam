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
        private UitbouwMuur previousSelectedWall;
        private Material wallOriginalMaterial;

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
            selectableLibraryItem = e.SelectableLibraryItem;
            selectMaterial = e.ComponentMaterial;
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
                    wallOriginalMaterial = previousSelectedWall.Material;

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

                    if (wall != previousSelectedWall && previousSelectedWall != null)
                    {
                        previousSelectedWall.SetMaterial(wallOriginalMaterial);
                    }

                    if (wall.Material != selectMaterial)
                    {
                        wallOriginalMaterial = wall.Material;
                        wall.SetMaterial(selectMaterial);
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
            if(previousSelectedWall)
                previousSelectedWall.SetMaterial(wallOriginalMaterial);

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
