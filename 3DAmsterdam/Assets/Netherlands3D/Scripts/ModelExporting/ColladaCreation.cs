using Netherlands3D.Core;
using Netherlands3D.Interface;
using Netherlands3D.TileSystem;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ColladaCreation : ModelFormatCreation
{
    [SerializeField]
    private LoadingScreen loadingScreen;
    private MeshClipper.RDBoundingBox boundingbox;

    private bool coordinatesToZero = true;
    public bool CoordinatesToZero { get => coordinatesToZero; set => coordinatesToZero = value; }

    public void CreateCollada(Bounds UnityBounds, List<Layer> layerList)
    {
        StartCoroutine(CreateFile(UnityBounds, layerList));
    }

	private IEnumerator CreateFile(Bounds UnityBounds, List<Layer> layerList)
    {
        FreezeLayers(layerList, true);
        Debug.Log(layerList.Count);
        Vector3RD bottomLeftRD = CoordConvert.UnitytoRD(UnityBounds.min);
        Vector3RD topRightRD = CoordConvert.UnitytoRD(UnityBounds.max);
        boundingbox = new MeshClipper.RDBoundingBox(bottomLeftRD.x, bottomLeftRD.y, topRightRD.x, topRightRD.y);
        ColladaFile colladaFile = new ColladaFile();

        yield return null;
        MeshClipper meshClipper = new MeshClipper();

        loadingScreen.ShowMessage("Collada-bestand genereren...");
        loadingScreen.ProgressBar.Percentage(0.1f);
        yield return new WaitForEndOfFrame();

        int layercounter = 0;
        foreach (var layer in layerList)
        {
            layercounter++;
            loadingScreen.ProgressBar.Percentage((float)layercounter / ((float)layerList.Count+1));
            loadingScreen.ProgressBar.SetMessage("Laag '" + layer.name + "' wordt omgezet...");
            yield return new WaitForEndOfFrame();

            List<GameObject> gameObjectsToClip = GetTilesInLayer(layer, bottomLeftRD, topRightRD);
            if (gameObjectsToClip.Count == 0)
            {
                continue;
            }
            foreach (var gameObject in gameObjectsToClip)
            {
                meshClipper.SetGameObject(gameObject);
                for (int submeshID = 0; submeshID < gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount; submeshID++)
                {
                    string layerName = gameObject.GetComponent<MeshRenderer>().sharedMaterials[submeshID].name.Replace(" (Instance)", "").Split(' ')[0];
                    loadingScreen.ProgressBar.SetMessage("Laag '" + layer.name + "' object " + layerName + " wordt uitgesneden...");
                    yield return new WaitForEndOfFrame();

                    meshClipper.ClipSubMesh(boundingbox, submeshID);
                    if (coordinatesToZero)
                    {
						//Move the coordinates so our bottom left of our clipped mesh is at the center of the scene
						for (int i = 0; i < meshClipper.clippedVerticesRD.Count; i++)
						{
                            var vert = meshClipper.clippedVerticesRD[i];
                            vert.x -= boundingbox.minX;
                            vert.y -= boundingbox.minY;
                            meshClipper.clippedVerticesRD[i] = vert;
                        }
					}

                    colladaFile.AddObject(meshClipper.clippedVerticesRD, layerName, gameObject.GetComponent<MeshRenderer>().sharedMaterials[submeshID]);
                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
        }

        loadingScreen.ProgressBar.Percentage((float)layerList.Count / ((float)layerList.Count + 1));
        loadingScreen.ProgressBar.SetMessage("Het Collada (.dae) bestand wordt afgerond...");
        yield return new WaitForEndOfFrame();

        //Create the collada file XML contents, and add our geo info (supported from collada 1.5)
        colladaFile.CreateCollada(true,CoordConvert.UnitytoWGS84(UnityBounds.min));

        //Save the collada file, with the coordinates embedded in the name.
        colladaFile.Save("Collada-RD-" + bottomLeftRD.x.ToString(CultureInfo.InvariantCulture) + "_" + bottomLeftRD.y.ToString(CultureInfo.InvariantCulture) + ".dae");

        FreezeLayers(layerList, false);
        loadingScreen.Hide();
        Debug.Log("file saved");
    }
}
