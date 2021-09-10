using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Text.RegularExpressions;
using ConvertCoordinates;
using Netherlands3D;

public class AutoImportOBJIntoScene : AssetPostprocessor
{
    private const string autoFolder = "ImportIntoScene";
    private static bool continueImport = false;

    private static string currentProcessingAssetPath = "";

    void OnPreprocessModel()
    {
        //Make sure if our preprocessor changes the asset, it is not imported again
        if (currentProcessingAssetPath == assetPath)
        {
            return;
        }

        currentProcessingAssetPath = assetPath;
        if (assetPath.Contains(autoFolder) && assetPath.Contains(".obj"))
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            modelImporter.isReadable = true;
            modelImporter.normalSmoothingSource = ModelImporterNormalSmoothingSource.FromAngle;
            modelImporter.normalSmoothingAngle = 5;

            if (continueImport || EditorUtility.DisplayDialog(
                "Auto import OBJ's into scene",
                "These models will be automatically placed into the current scene. Would you like to proceed?",
                "Proceed",
                "Cancel"
            ))
            {
                if(assetPath.Contains(".obj"))
                    CorrectOBJToSceneUnits(assetPath);

                continueImport = true;

                modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
            }
        }
    }

    private void CorrectOBJToSceneUnits(string filePath)
    {
        //Load up our application config set in our scene
        Config.activeConfiguration = EditorSceneManager.GetActiveScene().GetRootGameObjects()[0].GetComponent<ApplicationConfiguration>().ConfigurationFile;

        //Replace line vertex positions with corrected ones
        string[] objLines = File.ReadAllLines(filePath);
        for (int i = 0; i < objLines.Length; i++)
        {
            var lineWithSingleSpaces = Regex.Replace(objLines[i], @"\s+", " ");
            if (lineWithSingleSpaces.Contains("v "))
            {
                string[] lineParts = lineWithSingleSpaces.Split(' ');

                Vector3RD doubleCoordinate = new Vector3RD(
                    double.Parse(lineParts[1]),
                    double.Parse(lineParts[2]),
                    double.Parse(lineParts[3])
                );
                Vector3 unityCoordinate = CoordConvert.RDtoUnity(doubleCoordinate);

                var replacedLine = $"v {unityCoordinate.x} {unityCoordinate.y} {unityCoordinate.z}";
                objLines[i] = replacedLine;
            }
            else if (lineWithSingleSpaces.Contains("o "))
            {
                //Unity OBJ importer only splits up groups as seperate objects. So make sure our objects are groups, and remove BAG prefixes.
                objLines[i] = objLines[i].Replace("o ", "g ").Replace("NL.IMBAG.Pand.","");
            }
        }
        Debug.Log("Corrected OBJ file vert positions");

        File.WriteAllLines(filePath,objLines);
    }

    void OnPostprocessModel(GameObject gameObject)
    {
        if (!continueImport) return;

        MeshFilter[] allMeshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        foreach(var meshFilter in allMeshFilters)
        {
            FlipMeshNormals(meshFilter.sharedMesh);
        }

        Debug.Log("Placed object into scene");
        PrefabUtility.InstantiatePrefab(gameObject,EditorSceneManager.GetActiveScene());
    }

    private void FlipMeshNormals(Mesh mesh)
    {
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
            normals[i] = -normals[i];
        mesh.normals = normals;

        for (int m = 0; m < mesh.subMeshCount; m++)
        {
            int[] triangles = mesh.GetTriangles(m);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int temp = triangles[i + 0];
                triangles[i + 0] = triangles[i + 1];
                triangles[i + 1] = temp;
            }
            mesh.SetTriangles(triangles, m);
        }
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