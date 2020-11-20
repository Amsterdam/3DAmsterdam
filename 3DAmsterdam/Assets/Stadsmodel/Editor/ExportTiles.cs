using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ConvertCoordinates;

public class ExportTiles 
{
    //public string tilename = "tile/67321/51834/16";

    // Start is called before the first frame update
    [MenuItem("Tools/Exporteer Maaiveld naar OBJ")]

    // Update is called once per frame

    private static void ExporteerOBJ()
    {
        GameObject Parent = GameObject.Find("Exportgebouwen");
        for (int i = 0; i < Parent.transform.childCount; i++)
        {
            SaveTile(Parent.transform.GetChild(i).gameObject);
        }
    }

    static void SaveTile(GameObject TileToSave)
    {
        List<string> tekstregels = new List<string>();
        string tekstregel;
        string tilename = TileToSave.name;
        

        //terugroteren
        Vector3RD Rdloc = CoordConvert.UnitytoRD(TileToSave.transform.localPosition);
        double rotatie = CoordConvert.RDRotation(Rdloc);
        TileToSave.transform.Rotate(new Vector3(0, -(float)rotatie, 0));

        //GameObject GO = new GameObject(tilename.Replace("/", "-"));
        Mesh MeshToSave = TileToSave.GetComponent<MeshFilter>().sharedMesh;

        Vector3[] newVertices = MeshToSave.vertices;

        foreach (Vector3 item in newVertices)
        {
            tekstregel = "v ";
            Vector3 UnityCoordinaat = item;
            UnityCoordinaat += TileToSave.transform.localPosition;
            
            Vector3RD coordinaat;
            coordinaat = CoordConvert.UnitytoRD(UnityCoordinaat);
            tekstregel += coordinaat.x.ToString().Replace(",",".") + " ";
            tekstregel += coordinaat.y.ToString().Replace(",", ".") + " ";
            tekstregel += coordinaat.z.ToString().Replace(",", ".");
            tekstregels.Add(tekstregel);
        }

        for (int i = 0; i < MeshToSave.triangles.Length-2; i+=3)
        {
            tekstregel = "f ";
            tekstregel += MeshToSave.triangles[i]+1+" ";
            tekstregel += MeshToSave.triangles[i+1]+1 + " ";
            tekstregel += MeshToSave.triangles[i+2]+1 + " ";
            tekstregels.Add(tekstregel);
        }


        System.IO.File.WriteAllLines(@"D:\Temp\obj\"+TileToSave.name.Replace("/","-")+".obj", tekstregels.ToArray());

        //Texture TextureToSave = TileToSave.GetComponent<MeshRenderer>().material.mainTexture;
        //Material MaterialToSave = TileToSave.GetComponent<MeshRenderer>().material;
        //AssetDatabase.CreateAsset(TextureToSave, "Assets/TerrainExport/textures/" + tilename.Replace("/", "-") + ".asset");
        //AssetDatabase.CreateAsset(MeshToSave, "Assets/TerrainExport/meshes/" + tilename.Replace("/", "-") + ".asset");
        //AssetDatabase.CreateAsset(MaterialToSave, "Assets/TerrainExport/materials/" + tilename.Replace("/", "-") + ".mat");

        //AssetDatabase.SaveAssets();

        //Object prefab = PrefabUtility.CreatePrefab("Assets/TerrainExport/" + tilename.Replace("/", "-") + ".prefab", TileToSave);
        //PrefabUtility.ReplacePrefab(TileToSave, prefab, ReplacePrefabOptions.ConnectToPrefab);




    }
}
