using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw
{
    public class SelectMaterial : SelectableLibraryItem
    {
        public Material dragMaterial;

        protected override void OnLibraryItemSelected()
        {
            LibraryComponentSelectedEvent.RaiseMaterialSelected(this, DragContainerImage, IsTopComponent, dragMaterial, this);
        }
    }
}
