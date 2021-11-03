﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.Cameras;
using System.Linq;
using Netherlands3D.Interface;
using System.Globalization;
using Netherlands3D.Interface.Selection;
using Netherlands3D.ObjectInteraction;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.Logging;

namespace Netherlands3D.LayerSystem
{
    public class SelectSubObjects : Interactable
    {
        public TileHandler tileHandler;

        private Ray ray;
        private string lastSelectedID = "";

        public static List<string> selectedIDs;

        [SerializeField]
        private LayerMask clickCheckLayerMask;
        private BinaryMeshLayer containerLayer;

        private const string emptyID = "null";
        private const int maximumRayPiercingLoops = 20;

        private RaycastHit lastRaycastHit;

        [SerializeField]
        private int submeshIndex = 0;

        private void Awake()
        {
            contextMenuState = ContextPointerMenu.ContextState.BUILDING_SELECTION;

            selectedIDs = new List<string>();
            containerLayer = gameObject.GetComponent<BinaryMeshLayer>();
        }

        public override void Select()
        {
            if (!enabled) return;

            base.Select();
            FindSelectedID();
        }
        public override void SecondarySelect()
        {
            if (!enabled) return;

            base.Select();
            //On a secondary click, only select if we did not make a multisselection yet.
            if (selectedIDs.Count < 2)
            {
                Select();
            }
            else{
                //Simply retrigger the selection list we already have to trigger the right state for the context menu
                HighlightObjectsWithIDs(selectedIDs);
            }
        }

        public override void Deselect()
        {
            if (!enabled) return;

            base.Deselect();
            ClearSelection();
        }

        /// <summary>
        /// Select a mesh ID underneath the pointer
        /// </summary>
        private void FindSelectedID()
        {
            //Clear selected ids if we are not adding to a multiselection
            if (!Selector.doingMultiselect) selectedIDs.Clear();

            //Try to find a selected mesh ID and highlight it
            StartCoroutine(GetSelectedMeshIDData(Selector.mainSelectorRay, (value) => { HighlightSelectedID(value); }));
        }

        /// <summary>
        /// Find selected ID's based on a area selection done by our selectiontools.
        /// We find BAG id's within an area using a WebRequest and an API.
        /// </summary>
        public void FindSelectedIDsInArea()
        {
            if (!enabled) return;

            SelectionTools selectionTools = FindObjectOfType<SelectionTools>();
            var vertices = selectionTools.GetVertices();
            var bounds = selectionTools.GetBounds();

            //Polygon selection
            StartCoroutine(GetAllIDsInPolygonRange(vertices.ToArray(), HighlightObjectsWithIDs));
        }

        /// <summary>
        /// Add a single object to highlight selection. If we clicked an empty ID, clear the selection if we are not in multiselect
        /// </summary>
        /// <param name="id">The object ID</param>
        public void HighlightSelectedID(string id)
        {
            if (!enabled) return;

            if (id == emptyID && !Selector.doingMultiselect)
            {
                ClearSelection();
            }
            else
            {
                List<string> singleIdList = new List<string>();
                //Allow clicking a single object multiple times to move them in and out of our selection
                if (Selector.doingMultiselect && selectedIDs.Contains(id))
                {
                    selectedIDs.Remove(id);
                }
                else
                {
                    singleIdList.Add(id);
                }
                HighlightObjectsWithIDs(singleIdList);
            }
        }

        /// <summary>
        /// Removes an object with this specific ID from the selected list, and update the highlights
        /// </summary>
        /// <param name="id">The unique ID of this item</param>
        public void DeselectSpecificID(string id)
        {
            if (!enabled) return;

            if (selectedIDs.Contains(id))
            {
                selectedIDs.Remove(id);
                HighlightObjectsWithIDs(selectedIDs);
            }
        }

        /// <summary>
        /// Add list of ID's to our selected objects list
        /// </summary>
        /// <param name="ids">List of IDs to add to our selection</param>
        private void HighlightObjectsWithIDs(List<string> ids = null)
        {
            if (!enabled) return;

            if (ids != null) selectedIDs.AddRange(ids);
            selectedIDs = selectedIDs.Distinct().ToList(); //Filter out any possible duplicates

            lastSelectedID = (selectedIDs.Count > 0) ? selectedIDs.Last() : emptyID;

            //Make the selected object blink/highlight
            containerLayer.Highlight(selectedIDs);

            //Analytic
            Analytics.SendEvent("SelectedBuilding", lastSelectedID);

            //Specific context menu /sidepanel items per selection count
            if (selectedIDs.Count == 1)
            {
                ShowBAGDataForSelectedID(lastSelectedID);
                ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.BUILDING_SELECTION);
            }
            else if (selectedIDs.Count > 1)
            {
                ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.MULTI_BUILDING_SELECTION);
				//Update sidepanel outliner
				PropertiesPanel.Instance.OpenObjectInformation("Selectie", true);
				PropertiesPanel.Instance.RenderThumbnail(PropertiesPanel.ThumbnailRenderMethod.HIGHLIGHTED_BUILDINGS);
				PropertiesPanel.Instance.AddTitle("Geselecteerde panden");
                foreach (var id in selectedIDs)
                {
					PropertiesPanel.Instance.AddSelectionOutliner(this.gameObject, "Pand " + id, id);
                }
            }
            else if (ContextPointerMenu.Instance.state != ContextPointerMenu.ContextState.CUSTOM_OBJECTS)
            {
                ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.DEFAULT);
            }
        }

        /// <summary>
        /// Clear our list of selected objects, and update the highlights
        /// </summary>
        public void ClearSelection()
        {
            if (!enabled) return;

            if (selectedIDs.Count != 0)
            {
                lastSelectedID = emptyID;
                selectedIDs.Clear();
            }

            if (ContextPointerMenu.Instance.state != ContextPointerMenu.ContextState.CUSTOM_OBJECTS)
            {
                ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.DEFAULT);
            }

            //Remove highlights by highlighting our empty list
            containerLayer.Highlight(selectedIDs);
        }

        /// <summary>
        /// Hides all objects that matches the list of ID's, and remove them from our selection list.
        /// </summary>
        public void HideSelectedIDs()
        {
            if (!enabled) return;

            if (selectedIDs.Count > 0)
            {
                //Adds selected ID's to our hidding objects of our layer
                containerLayer.Hide(selectedIDs);
                selectedIDs.Clear();
            }

            //If we hide something, make sure our context menu is reset to default again.
            //ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.DEFAULT);
        }

        /// <summary>
        /// Shows all hidden objects by clearing our selection and hiding that empty list
        /// </summary>
        public void UnhideAll()
        {
            if (!enabled) return;

            lastSelectedID = emptyID;
            selectedIDs.Clear();
            containerLayer.Hide(selectedIDs);
        }

        /// <summary>
        /// Method to allow other objects to display the information panel for the last ID we selected here.
        /// </summary>
        public void ShowBAGDataForSelectedID(string id = "")
        {
            if (!enabled) return;

            var thumbnailFrom = lastRaycastHit.point + (Vector3.up * 300) + (Vector3.back * 300);
            var lookAtTarget = lastRaycastHit.point;

            if (id != emptyID)
            {
				PropertiesPanel.Instance.OpenObjectInformation("", true);
                if (selectedIDs.Count > 1) Interface.SidePanel.PropertiesPanel.Instance.AddActionButtonText("< Geselecteerde panden", (action) =>
                {
					HighlightObjectsWithIDs();
                }
                );
				PropertiesPanel.Instance.displayBagData.ShowBuildingData(id);
            }
            else if (lastSelectedID != emptyID)
            {
				PropertiesPanel.Instance.OpenObjectInformation("", true);
				PropertiesPanel.Instance.displayBagData.ShowBuildingData(lastSelectedID);
            }
        }

        private void GetAllVertsInSelection(string id)
        {
            containerLayer.GetAllVerts(selectedIDs);
        }

        IEnumerator GetSelectedMeshIDData(Ray ray, System.Action<string> callback)
        {
            //Check area that we clicked, and add the (heavy) mesh collider there
            Vector3 planeHit = CameraModeChanger.Instance.CurrentCameraControls.GetPointerPositionInWorld();
            containerLayer.AddMeshColliders(planeHit);

            yield return new WaitForEndOfFrame();

            //No fire a raycast towards our meshcolliders to see what face we hit 
            if (Physics.Raycast(ray, out lastRaycastHit, 10000, clickCheckLayerMask.value) == false)
            {
                
                callback(emptyID);
                yield break;
            }

            //Get the mesh we selected and check if it has an ID stored in the UV2 slot
            Mesh mesh = lastRaycastHit.collider.gameObject.GetComponent<MeshFilter>().mesh;
            int triangleVertexIndex = lastRaycastHit.triangleIndex * 3;
            var vertexIndex = mesh.GetIndices(submeshIndex)[triangleVertexIndex];
            
            var hitUvCoordinate = mesh.uv2[vertexIndex];
            var gameObjectToHighlight = lastRaycastHit.collider.gameObject;

            //Fetch this tile's subject data (if we didnt already)
            SubObjects subObjects = gameObjectToHighlight.GetComponent<SubObjects>();
            if(!subObjects) gameObjectToHighlight.AddComponent<SubObjects>();

            //Pass down the ray we used to click to get the ID we clicked
            string id = subObjects.SelectByVertexIndex(vertexIndex);

            subObjects.HightlightBuildingWithID(id, Color.white);
        }
    }
}
