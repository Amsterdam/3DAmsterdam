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
    private static bool skipNextQuestions = false;

    private static string currentProcessingAssetPath = "";
	private static bool flipNormals = false;

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

            if (skipNextQuestions || EditorUtility.DisplayDialog(
                "Auto import OBJ's into scene",
                "These models will be automatically placed into the current scene. Would you like to proceed?",
                "Proceed",
                "Cancel"
            ))
            {
                if(assetPath.Contains(".obj"))
                    CorrectOBJToSceneUnits(assetPath);

                skipNextQuestions = true;

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

                var replacedLine = $"v {-unityCoordinate.x} {unityCoordinate.y} {unityCoordinate.z}";
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
        if (!skipNextQuestions) return;

        if (flipNormals)
        {
            MeshFilter[] allMeshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in allMeshFilters)
            {
                FlipMeshNormals(meshFilter.sharedMesh);
            }
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
        if (!skipNextQuestions) return;

        Debug.Log("Imported all .obj files into scene.");
        skipNextQuestions = false;

        foreach (string assetPath in importedAssets)
        {
            if (assetPath.Contains(autoFolder))
            {
                var importedObject = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
                GameObject spawnedObject = (GameObject)PrefabUtility.InstantiatePrefab(importedObject, EditorSceneManager.GetActiveScene());
                //PrefabUtility.UnpackPrefabInstance(spawnedObject,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
                //spawnedObject.transform.DetachChildren();
                //MonoBehaviour.Destroy(spawnedObject);
            }
        }
    }
    /*
    private void BakeIntoTiles()
    {
        var xTiles = Mathf.RoundToInt(((float)Config.activeConfiguration.TopRightRD.x - (float)Config.activeConfiguration.BottomLeftRD.x) / (float)tileSize);
        var yTiles = Mathf.RoundToInt(((float)Config.activeConfiguration.TopRightRD.y - (float)Config.activeConfiguration.BottomLeftRD.y) / (float)tileSize);

        var totalTiles = xTiles * yTiles;
        int currentTile = 0;

        //Walk the tilegrid
        var tileRD = new Vector2Int(0, 0);
        for (int x = 0; x < xTiles; x++)
        {
            tileRD.x = (int)Config.activeConfiguration.BottomLeftRD.x + (x * tileSize);
            for (int y = 0; y < yTiles; y++)
            {
                currentTile++;
                tileRD.y = (int)Config.activeConfiguration.BottomLeftRD.y + (y * tileSize);
                string tileName = "buildings_" + tileRD.x + "_" + tileRD.y + "." + lodLevel;

                //If we supplied a filter we check if this tile contains this substring in order to be (re)generated
                if (exclusivelyGenerateTilesWithSubstring != "" && !tileName.Contains(exclusivelyGenerateTilesWithSubstring))
                {
                    print("Skipping tile because we supplied a specific name we want to replace.");
                    if (!Application.isBatchMode) yield return new WaitForEndOfFrame();
                    continue;
                }

                //Skip files if we enabled that option and it exists
                string assetFileName = TileCombineUtility.unityMeshAssetFolder + tileName + ".asset";
                if (skipExistingFiles && File.Exists(Application.dataPath + "/../" + assetFileName))
                {
                    print("Skipping existing tile: " + Application.dataPath + "/../" + assetFileName);
                    ProgressPreviewMap.Instance.ColorTile(x, y, TilePreviewState.SKIPPED);
                    if (!Application.isBatchMode) yield return new WaitForEndOfFrame();
                    continue;
                }

                //Spawn our tile container
                GameObject newTileContainer = new GameObject();
                newTileContainer.transform.position = CoordConvert.RDtoUnity(tileRD + tileOffset);
                newTileContainer.name = tileName;
                //And move children in this tile
                int childrenInTile = 0;

                MeshRenderer[] remainingBuildings = GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer meshRenderer in remainingBuildings)
                {
                    meshRenderer.gameObject.name = meshRenderer.gameObject.name.Replace(removePrefix, "");
                    if (bagIdsToSkip.Contains(meshRenderer.gameObject.name) && !IsReplacementObject(meshRenderer))
                    {
                        //Is this ID in the skip list, and it is not our own override? Remove it.
                        Destroy(meshRenderer.gameObject);
                    }
                    else
                    {
                        //Check if this object center falls within the tile we are creating
                        var childCenterPoint = CoordConvert.UnitytoRD(meshRenderer.bounds.center);
                        if (childCenterPoint.x < tileRD.x + tileSize && childCenterPoint.x > tileRD.x && childCenterPoint.y < tileRD.y + tileSize && childCenterPoint.y > tileRD.y)
                        {
                            //This child object center falls within this tile. Lets move it in there.
                            meshRenderer.transform.SetParent(newTileContainer.transform, true);
                            childrenInTile++;
                        }
                    }
                }

                if (childrenInTile == 0)
                {
                    Destroy(newTileContainer);
                    ProgressPreviewMap.Instance.ColorTile(x, y, TilePreviewState.EMPTY);
                    print($"<color={ConsoleColors.GeneralDataWarningHexColor}>No children found for tile {tileName}</color>");
                    continue;
                }

                //And when we are done, bake it.
                print($"<color={ConsoleColors.GeneralStartProgressHexColor}>Baking tile {tileName} with {childrenInTile} buildings</color>");
                if (!Application.isBatchMode) yield return new WaitForEndOfFrame();

                TileCombineUtility.CombineSource(newTileContainer, newTileContainer.transform.position, renderInViewport, defaultMaterial, true);
                print($"<color={ConsoleColors.GeneralSuccessHexColor}>Finished tile {tileName}</color>");

                ProgressPreviewMap.Instance.ColorTile(x, y, TilePreviewState.DONE);

                if (!Application.isBatchMode) yield return new WaitForEndOfFrame();
            }
        }

        print($"<color={ConsoleColors.GeneralSuccessHexColor}>All done!</color>");

        if (!Application.isBatchMode) yield return new WaitForEndOfFrame();
    }*/
}