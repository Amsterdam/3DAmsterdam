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
            ComponentImage.sprite = e.Image.sprite;
            isTopComponent = e.IsTopComponent;
            ComponentObject = e.ComponentObject;
            selectComponent = e.SelectComponent;

            ComponentObject.SetSize(new Vector2(e.ComponentWidth, e.ComponentHeight));
            //ComponentObject.transform.localScale = new Vector3(e.ComponentWidth, e.ComponentHeight, 1);
        }

        private void Update()
        {
            if (Input.GetMouseButton(0) == false && placedBoundaryFeature != null)
            {
                Destroy(placedBoundaryFeature.gameObject);
                //placedBoundaryFeature = null;
                selectComponent.SetToggle(false);
                return;
            }

            if (Input.GetMouseButton(0) == false) return;

            RaycastHit hit;
            var screenpoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y + MouseXOffset, 0);
            Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(screenpoint);

            var mask = LayerMask.GetMask("Maskable");
            bool casted = Physics.Raycast(ray, out hit, Mathf.Infinity, mask);

            if (casted)
            {
                var wall = hit.transform.GetComponent<UitbouwMuur>();
                if (wall)
                {
                    bool allowed = (isTopComponent && wall.Side == WallSide.Top) || (!isTopComponent && (wall.Side == WallSide.Left || wall.Side == WallSide.Right || wall.Side == WallSide.Front));

                    if (allowed)
                    {
                        if (placedBoundaryFeature == null)
                        {
                            placedBoundaryFeature = Instantiate(ComponentObject);
                            placedBoundaryFeature.SetWall(wall);
                        }

                        placedBoundaryFeature.transform.position = hit.point;
                        placedBoundaryFeature.transform.rotation = hit.transform.rotation;

                        ComponentImage.enabled = false;
                    }
                }
            }
            else if (placedBoundaryFeature != null)
            {
                Destroy(placedBoundaryFeature.gameObject);
                //PlacedObject = null;
                ComponentImage.enabled = true;
            }
        }
    }
}
