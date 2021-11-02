using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class BoundaryFeatureEditHandler : MonoBehaviour
    {
        public BoundaryFeature ActiveFeature { get; private set; }

        [SerializeField]
        private Sprite editSprite, deleteSprite;
        
        public static Sprite EditSprite;
        public static Sprite DeleteSprite;

        private void Awake()
        {
            EditSprite = editSprite;
            DeleteSprite = deleteSprite;
        }

        public void SelectFeature(BoundaryFeature feature)
        {
            //DeselectFeature();

            //editbutton.enabled = true;
            //deleteButton.enabled = true;

            feature.SetMode(EditMode.Reposition);
            ActiveFeature = feature;

            //deleteButton = CoordinateNumbers.Instance.CreateButton();
            //deleteButton.GetComponent<Button>().onClick.AddListener(DeleteFeature);
        }

        public void DeselectFeature()
        {
            if (ActiveFeature)
                ActiveFeature.SetMode(EditMode.None);

            //editbutton.GetComponent<Button>().onClick.RemoveAllListeners();
            //deleteButton.GetComponent<Button>().onClick.RemoveAllListeners();

            //editbutton.enabled = false;
            //deleteButton.enabled = false;
            ActiveFeature = null;
        }

        private void Update()
        {
            ProcessUserInput();
        }
        private void ProcessUserInput()
        {
            Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
            LayerMask boundaryFeaturesMask = LayerMask.GetMask("StencilMask");
            LayerMask uiMask = LayerMask.GetMask("UI");
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                if (ActiveFeature)
                {
                    DeselectFeature();
                }
                else if (Physics.Raycast(ray, out var hit, Mathf.Infinity, boundaryFeaturesMask))
                {
                    var bf = hit.collider.GetComponent<BoundaryFeature>();
                    if (bf)
                    {
                        SelectFeature(bf);
                    }
                }
            }
        }
    }
}
