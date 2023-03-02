using Netherlands3D.Events;
using Netherlands3D.Interface;
using Netherlands3D.Interface.SidePanel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DisplayBAGMetaData : MonoBehaviour
{
    [SerializeField]
    private StringListEvent onSelectedBuildings;

	[SerializeField]
	private Vector3Event clickedPosition;

    void Awake()
    {
        onSelectedBuildings.AddListenerStarted(SelectedBuildings);

		if(clickedPosition)
			clickedPosition.AddListenerStarted(SetClickedPosition);
    }

	private void SetClickedPosition(Vector3 arg0)
	{
		throw new NotImplementedException();
	}

	private void SelectedBuildings(List<string> selectedIDs)
	{
		if (selectedIDs.Count == 1)
		{
			ShowBAGDataForSelectedID(selectedIDs[0]);
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

	public void ShowBAGDataForSelectedID(string id = "")
	{
		if (!enabled) return;

		if (id != "null")
		{
			PropertiesPanel.Instance.OpenObjectInformation("", true);
			PropertiesPanel.Instance.displayBagData.ShowBuildingData(id);
		}
	}
}
