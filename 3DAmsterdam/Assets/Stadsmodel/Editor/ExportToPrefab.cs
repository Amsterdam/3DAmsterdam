using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExportToPrefab 
{
    public static  int volgnummer;
    [MenuItem("Tools/Exporteer Panden naar Prefabs")]
    // Start is called before the first frame update
    
    private static void ExporteerOBJ()
    {
        int volgnummer = 0;
        GameObject Subparent;
        GameObject Parent = GameObject.Find("Panden");
        for (int i = 0; i < Parent.transform.childCount; i++)
        {
            Subparent = Parent.transform.GetChild(i).gameObject;
            for (int ii = 0; ii < Subparent.transform.childCount; ii++)
            {
                string kindnaam = Subparent.transform.GetChild(ii).name;
                SaveTile(Subparent.transform.GetChild(ii).gameObject);
            }
        }
    }
    static void SaveTile(GameObject TileToSave)
    {

        string tilename = TileToSave.name;

        //Texture TextureToSave = TileToSave.GetComponent<MeshRenderer>().material.mainTexture;
        //Material MaterialToSave = TileToSave.GetComponent<MeshRenderer>().material;
        Mesh MeshToSave = TileToSave.GetComponent<MeshFilter>().sharedMesh;
        try
        {

       
        MeshToSave.name = tilename;
        //AssetDatabase.CreateAsset(TextureToSave, "Assets/TerrainExport/textures/" + tilename.Replace("/", "-") + ".asset");
        AssetDatabase.CreateAsset(MeshToSave, "Assets/Panden/meshes/" + tilename + ".asset");
        //AssetDatabase.CreateAsset(MaterialToSave, "Assets/TerrainExport/materials/" + tilename.Replace("/", "-") + ".mat");

        AssetDatabase.SaveAssets();
        
        Object prefab = PrefabUtility.CreatePrefab("Assets/Panden/" + tilename + ".prefab", TileToSave);
        
        PrefabUtility.ReplacePrefab(TileToSave, prefab, ReplacePrefabOptions.ConnectToPrefab);

        }
        catch (System.Exception)
        {

            ;
        }


    }
}
