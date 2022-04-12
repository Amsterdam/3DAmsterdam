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
        public BoundaryFeature ComponentObject;
        public Image ComponentImage;
        public int MouseXOffset = 92;

        private BoundaryFeature placedBoundaryFeature;
        public bool isTopComponent;

        private SelectComponent selectComponent;

        private void Awake()
        {
            LibraryComponentSelectedEvent.Subscribe(OnSelect);
        }

        private void OnSelect(object sender, LibraryComponentSelectedEvent.LibraryComponentSelectedEventArgs e)
        {
            ComponentImage.sprite = e.Sprite;
            isTopComponent = e.IsTopComponent;
            ComponentObject = e.ComponentObject;
            selectComponent = e.SelectComponent;

            ComponentObject.SetSize(new Vector2(e.ComponentWidth - 0.0001f, e.ComponentHeight - 0.0001f));
            //ComponentObject.transform.localScale = new Vector3(e.ComponentWidth, e.ComponentHeight, 1);
        }

        private void Update()
        {
            if (Input.GetMouseButton(0) == false)
            {
                if (placedBoundaryFeature != null)
                {
                    placedBoundaryFeature = null; //place object and set this one to null to 
                }
                selectComponent.Deslect();
                return;
            }


            RaycastHit hit;
            var screenpoint = Input.mousePosition;

            Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(screenpoint);

            var mask = LayerMask.GetMask("Maskable");
            bool casted = Physics.Raycast(ray, out hit, Mathf.Infinity, mask);

            if (casted)
            {
                var wall = hit.transform.GetComponent<UitbouwMuur>();
                if (wall)
                {
                    bool allowed = (isTopComponent && wall.Side == WallSide.Top) || (!isTopComponent && (wall.Side == WallSide.Left || wall.Side == WallSide.Right || wall.Side == WallSide.Front || wall.Side == WallSide.Back));

                    if (allowed)
                    {
                        if (placedBoundaryFeature == null)
                        {
                            AddBoundaryFeatureToSaveData(ComponentObject.name, wall.transform.parent);
                        }

                        placedBoundaryFeature.SetWall(wall); //set wall again to update it if the wall changes
                        placedBoundaryFeature.transform.position = hit.point;
                        placedBoundaryFeature.transform.rotation = hit.transform.rotation;
                        ComponentImage.enabled = false;
                    }
                    else if (placedBoundaryFeature != null)
                    {
                        RemoveBoundaryFeatureFromSaveData(placedBoundaryFeature);
                        ComponentImage.enabled = true;
                    }
                }
            }
            else if (placedBoundaryFeature != null)
            {
                RemoveBoundaryFeatureFromSaveData(placedBoundaryFeature);
                ComponentImage.enabled = true;
            }
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
