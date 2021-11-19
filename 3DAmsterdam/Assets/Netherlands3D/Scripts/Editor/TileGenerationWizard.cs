using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering;
using TileBakeLibrary;

public class TileGenerationWizard : ScriptableWizard
{
	public string sourceFolder = "";
	public string targetFolder = "";

    private CityJSONToTileConverter converter;

    [MenuItem("Netherlands 3D/Tile Generation Wizard")]
	static void CreateWizard()
	{
		var wizard = DisplayWizard<TileGenerationWizard>("Tile Generation Wizard", "Create", "Cancel");		
	}

    void OnWizardCreate()
    {
        Debug.Log("Creating binary tile files");
        helpString = "Converting...";

        if (sourceFolder == "") sourceFolder = Application.dataPath;
        if (targetFolder == "") targetFolder = Application.dataPath;

        converter = new CityJSONToTileConverter();
        converter.SetSourcePath(sourceFolder);
        converter.SetTargetPath(targetFolder);
        converter.Convert();
    }

    void OnWizardUpdate()
    {
        helpString = "Please select the source path containing the .cityjson files.\n Set the target folder where you want generated the binary tiles to be placed.";
    }

    void OnWizardOtherButton()
    {
        helpString = "Canceled";
        converter.Cancel();
    }
}