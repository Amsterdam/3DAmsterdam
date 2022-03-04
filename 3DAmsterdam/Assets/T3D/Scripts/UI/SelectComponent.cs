using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class SelectComponent : MonoBehaviour, IDragHandler
    {
        public GameObject DragContainer;
        public Sprite DragContainerImage;
        public Color SelectedColor;
        public bool IsTopComponent;
        public float ComponentWidth;
        public float ComponentHeight;

        public BoundaryFeature ComponentObject;

        private Toggle toggle;
        private Image image;

        void Start()
        {
            toggle = GetComponent<Toggle>();
            image = GetComponent<Image>();
        }

        public void Deslect()
        {
            SetComponentSelected(false);
        }

        void SetComponentSelected(bool changed)
        {
            toggle.isOn = changed;
            image.color = changed ? SelectedColor : Color.white;

            if (changed)
            {
                DragContainer.transform.position = Input.mousePosition;
                DragContainer.SetActive(true);
                DragContainer.GetComponent<HandleDragContainer>().ComponentImage.enabled = true;

            }
            else if (toggle.group.AnyTogglesOn() == false)
            {
                DragContainer.SetActive(false);
            }

            LibraryComponentSelectedEvent.Raise(this, DragContainerImage, IsTopComponent, ComponentWidth, ComponentHeight, ComponentObject, this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            SetComponentSelected(true);
        }
    }
}
