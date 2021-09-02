using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AutoImportOBJIntoScene : AssetPostprocessor
{
    private const string autoFolder = "ImportIntoScene";
    private static bool continueImport = false;
    void OnPreprocessModel()
    {
        if (assetPath.Contains(autoFolder))
        {
            if (EditorUtility.DisplayDialog(
                "Auto import OBJ's into scene",
                "These models will be automatically placed into the current scene. Would you like to proceed?",
                "Proceed",
                "Cancel"
            ))
            {
                continueImport = true;
                ModelImporter modelImporter = assetImporter as ModelImporter;
                modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
            }
        }
    }

    void OnPostprocessModel(GameObject gameObject)
    {
        if (!continueImport) return;

        Debug.Log("Placed object into scene");
        PrefabUtility.InstantiatePrefab(gameObject,EditorSceneManager.GetActiveScene());
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (!continueImport) return;

        foreach (string assetPath in importedAssets)
        {
            if (assetPath.Contains(autoFolder))
            {
                var importedObject = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
                PrefabUtility.InstantiatePrefab(importedObject, EditorSceneManager.GetActiveScene());
            }
        }
    }
}