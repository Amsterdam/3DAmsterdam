using System;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.Core;
using Netherlands3D.ObjectInteraction;
using Netherlands3D.Logging;
using Netherlands3D.Help;

namespace Netherlands3D.Interface
{
	public class LayerExporter : MonoBehaviour
	{
		private string selectedExportFormat = "";

		[SerializeField]
		List<TileSystem.Layer> selectableLayers;

		private bool[] exportLayerToggles = new bool[4] { true, true, true, true };

		private Bounds exportBounds;

		public GridSelection gridSelection;

		private string helpMessage = "<b>Shift+Klik+Sleep</b> om het download gebied te selecteren";

		[SerializeField]
		private Material downloadBlockMaterial;

		private bool acceptedTerms = false;

		public void OnEnable()
		{
			HelpMessage.Instance.Show(helpMessage);
			//Make sure you only subscribe once
			gridSelection.StartSelection(downloadBlockMaterial);

			gridSelection.onGridSelected.RemoveAllListeners();
			gridSelection.onGridSelected.AddListener(SetBounds);
		}

		private void OnDisable()
		{
			gridSelection.onGridSelected.RemoveListener(SetBounds);
		}

		public void SetBounds(Bounds gridBounds)
		{
			exportBounds = gridBounds;
			DisplayUI();
		}

		private void DisplayUI()
		{
			//TODO: send this boundingbox to the mesh selection logic, and draw the sidepanel
			PropertiesPanel.Instance.OpenObjectInformation("Download selectiegebied", true,10);

			RenderToThumbnail();

			PropertiesPanel.Instance.AddTitle("Lagen");
			PropertiesPanel.Instance.AddActionCheckbox("Gebouwen", Convert.ToBoolean(PlayerPrefs.GetInt("exportLayer0Toggle", 1)), (action) =>
			{
				exportLayerToggles[0] = action;
				PlayerPrefs.SetInt("exportLayer0Toggle", Convert.ToInt32(exportLayerToggles[0]));
			});
			PropertiesPanel.Instance.AddActionCheckbox("Bomen", Convert.ToBoolean(PlayerPrefs.GetInt("exportLayer1Toggle", 1)), (action) =>
			{
				exportLayerToggles[1] = action;
				PlayerPrefs.SetInt("exportLayer1Toggle", Convert.ToInt32(exportLayerToggles[1]));
			});
			PropertiesPanel.Instance.AddActionCheckbox("Maaiveld", Convert.ToBoolean(PlayerPrefs.GetInt("exportLayer2Toggle", 1)), (action) =>
			{
				exportLayerToggles[2] = action;
				PlayerPrefs.SetInt("exportLayer2Toggle", Convert.ToInt32(exportLayerToggles[2]));
			});
			PropertiesPanel.Instance.AddActionCheckbox("Rioolnetwerk", Convert.ToBoolean(PlayerPrefs.GetInt("exportLayer3Toggle", 1)), (action) =>
			{
				exportLayerToggles[3] = action;
				PlayerPrefs.SetInt("exportLayer3Toggle", Convert.ToInt32(exportLayerToggles[3]));
			});

			var exportFormats = new string[] { "AutoCAD DXF (.dxf)", "Collada DAE (.dae)" };
			selectedExportFormat = PlayerPrefs.GetString("exportFormat", exportFormats[0]);
			PropertiesPanel.Instance.AddActionDropdown(exportFormats, (action) =>
			{
				selectedExportFormat = action;
				PlayerPrefs.SetString("exportFormat", action);

			}, PlayerPrefs.GetString("exportFormat", exportFormats[0]));


			PropertiesPanel.Instance.AddTitle("Voorwaarden");
			PropertiesPanel.Instance.AddLink("Gebruiksvoorwaarden 3D BAG", "https://docs.3dbag.nl/en/copyright/");
			PropertiesPanel.Instance.AddLink("Rechtenbeleid 3D basisvoorziening", "https://docs.geostandaarden.nl/3dbv/prod/");
			PropertiesPanel.Instance.AddActionCheckbox("Ik ga akkoord met de voorwaarden", acceptedTerms, (action) =>
			{
				acceptedTerms = action;
				DisplayUI();//Redraw to enable/disable downloads button
			});

			PropertiesPanel.Instance.AddActionButtonBig("Downloaden", (action) =>
			{
				List<TileSystem.Layer> selectedLayers = new List<TileSystem.Layer>();
				for (int i = 0; i < selectableLayers.Count; i++)
				{
					if (exportLayerToggles[i])
					{
						selectedLayers.Add(selectableLayers[i]);
					}
				}
				print(selectedExportFormat);

				var amountOfCellsInBounds = (exportBounds.size.x / VisualGrid.Instance.CellSize) * (exportBounds.size.z / VisualGrid.Instance.CellSize);

				var bottomLeftRD = CoordConvert.UnitytoRD(exportBounds.min);
				var topRightRD = CoordConvert.UnitytoRD(exportBounds.max);

				Analytics.SendEvent("LayersExport", selectedExportFormat, $"{amountOfCellsInBounds} cells with bounds: {bottomLeftRD.x},{bottomLeftRD.y},{topRightRD.x},{topRightRD.y}");

				switch (selectedExportFormat)
				{
					case "AutoCAD DXF (.dxf)":
						Debug.Log("Start building DXF");
						GetComponent<DXFCreation>().CreateDXF(exportBounds, selectedLayers);
						break;
					case "Collada DAE (.dae)":
						Debug.Log("Start building collada");
						GetComponent<ColladaCreation>().CreateCollada(exportBounds, selectedLayers);
						break;
					default:
						WarningDialogs.Instance.ShowNewDialog("Exporteer " + selectedExportFormat + " nog niet geactiveerd.");
						break;
				}
			}).ToggleButtonInteraction(acceptedTerms);

			if(acceptedTerms) PropertiesPanel.Instance.AddTextfieldColor("Pas Op! bij een selectie van meer dan 16 tegels is het mogelijk dat uw browser niet genoeg geheugen heeft en crasht", Color.red, FontStyle.Normal);
		}

		private void RenderToThumbnail()
		{
			//Lets render a ortographic thumbnail for a proper grid topdown view
				PropertiesPanel.Instance.RenderThumbnailContaining(
				exportBounds,
				PropertiesPanel.ThumbnailRenderMethod.ORTOGRAPHIC,
				exportBounds.center + Vector3.up * 150.0f
			);
		}
	}
}