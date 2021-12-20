using Netherlands3D.Core;
using Netherlands3D;
using Netherlands3D.AssetGeneration;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class BakeChildrenIntoTiles : MonoBehaviour
{
    [SerializeField]
    private int tileSize = 1000;

    [SerializeField]
    private string lodLevel = "2.2";

    [SerializeField]
    private bool skipExistingFiles = false;

    [SerializeField]
    private bool renderInViewport = false;

    [SerializeField]
    private Material defaultMaterial;

    [Tooltip("A .txt list of bag ID's on new lines, with all IDS that should be skipped")]
    [SerializeField]
    private string optionalSkipListPath = "";
    private List<string> bagIdsToSkip;

    void Start()
    {
        ReadSkipIDs();
        StartCoroutine(BakeIntoTiles());
    }

    private void ReadSkipIDs()
    {
        bagIdsToSkip = new List<string>();

        //Read optional txt file with id's to skip to our skiplist
        if (optionalSkipListPath != "" && File.Exists(optionalSkipListPath))
        {
            bagIdsToSkip = File.ReadAllLines(optionalSkipListPath).ToList<string>();
        }
    }

    private IEnumerator BakeIntoTiles()
    {
        var xTiles = Mathf.RoundToInt(((float)Config.activeConfiguration.TopRightRD.x - (float)Config.activeConfiguration.BottomLeftRD.x) / (float)tileSize);
        var yTiles = Mathf.RoundToInt(((float)Config.activeConfiguration.TopRightRD.y - (float)Config.activeConfiguration.BottomLeftRD.y) / (float)tileSize);

        var totalTiles = xTiles * yTiles;
        int currentTile = 0;

        yield return ProgressPreviewMap.Instance.Initialize(xTiles, yTiles);

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

                print("Working on tile " + tileName);
                if (!Application.isBatchMode) yield return new WaitForEndOfFrame();

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
                newTileContainer.transform.position = CoordConvert.RDtoUnity(tileRD + (Vector2.one*(tileSize/2)));
                newTileContainer.name = tileName;
                //And move children in this tile
                int childrenInTile = 0;

                MeshRenderer[] remainingBuildings = GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer meshRenderer in remainingBuildings)
                {
                    if (bagIdsToSkip.Contains(meshRenderer.gameObject.name))
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
    }
}
