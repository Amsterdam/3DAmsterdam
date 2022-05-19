using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class SelectMaterial : SelectableLibraryItem
    {
        //public GameObject DragContainer;
        //public Sprite DragContainerImage;
        //public Color SelectedColor;
        //public bool IsTopComponent;
        public Material dragMaterial;

        //private Toggle toggle;
        //private Image image;

        //void Start()
        //{
        //    toggle = GetComponent<Toggle>();
        //    image = GetComponent<Image>();
        //}

        protected override void OnLibraryItemSelected()
        {
            LibraryComponentSelectedEvent.RaiseMaterialSelected(this, DragContainerImage, IsTopComponent, dragMaterial, this);
        }

        //public void Deslect()
        //{
        //    SetMaterialSelected(false);
        //}

        //void SetMaterialSelected(bool changed)
        //{
        //    toggle.isOn = changed;
        //    image.color = changed ? SelectedColor : Color.white;

        //    if (changed)
        //    {
        //        DragContainer.transform.position = Input.mousePosition;
        //        DragContainer.SetActive(true);
        //        DragContainer.GetComponent<HandleDragContainer>().ComponentImage.enabled = true;

        //    }
        //    else if (toggle.group.AnyTogglesOn() == false)
        //    {
        //        DragContainer.SetActive(false);
        //    }

        //}

        //public void OnDrag(PointerEventData eventData)
        //{
        //    SetMaterialSelected(true);
        //}
    }
}
