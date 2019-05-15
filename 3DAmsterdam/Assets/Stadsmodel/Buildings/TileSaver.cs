using System.Collections.Generic;
using UnityEngine;


public class TileSaver
{

    public static void Save(Vector3 [] tileIds)
    {
        var btm = UnityEngine.Object.FindObjectOfType<BuildingTileManager>();
        foreach( var tid in tileIds)
        {
            var btd = btm.Get(tid);
            if (btd == null)
                continue;
            Save(btd.Gameobjecten.ToArray());
        }
        
    }

    public static void Save2(Vector3[] TileIds)
    {

        foreach (var tid in TileIds)
        {
            int x = Mathf.RoundToInt(tid.x);
            int y = Mathf.RoundToInt(tid.y);
            int z = Mathf.RoundToInt(tid.z);
            GameObject panden = GameObject.Find($"{x}_{y}_{z}");

            // Optionally find other tile data 
            Save(new GameObject[] { panden });
        }
    }

    public static void Save(GameObject [] gos)
    {
        foreach( var go in gos )
        {
            var mrs = go.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in mrs)
            {
                for (int i = 0; i < mr.materials.Length; i++)
                {
                    mr.materials[i].color = Color.red;
                }
            }
        }
    }
}
