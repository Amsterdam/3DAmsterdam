using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class SelectComponent : SelectableLibraryItem
    {
        public float ComponentWidth;
        public float ComponentHeight;

        public BoundaryFeature ComponentObject;

        protected override void OnLibraryItemSelected()
        {
            LibraryComponentSelectedEvent.RaiseComponentSelected(this, DragContainerImage, IsTopComponent, ComponentWidth, ComponentHeight, ComponentObject, this);
        }
    }
}
