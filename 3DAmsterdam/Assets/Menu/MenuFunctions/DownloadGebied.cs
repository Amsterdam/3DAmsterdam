using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownloadGebied : MonoBehaviour
{
    Color oriColor = Color.black;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, 10000, LayerMask.GetMask("Panden")))
            return;

        var tileIdStr = hit.collider.gameObject.name.Split('_');
        if (tileIdStr.Length != 3)
            return;

        int x, y, z;
        if (!int.TryParse(tileIdStr[0], out x)) return;
        if (!int.TryParse(tileIdStr[1], out y)) return;
        if (!int.TryParse(tileIdStr[2], out z)) return;

        GameObject pand = GameObject.Find($"{x}_{y}_{z}");
        if (oriColor == Color.black)
        {
            oriColor = pand.GetComponentInChildren<MeshRenderer>().sharedMaterial.color;
        }

        // Optionally find other tile data 
        var panden = new GameObject[] { pand };
        MarkColor(panden, Color.red);

        TileSaver.SaveGameObjects(panden, (bool succes) =>
        {
          //   MarkColor(panden, succes ? Color.green : Color.red);
             StartCoroutine(ResetColor(panden));
        });
    }

    void MarkColor(GameObject [] gos, Color c)
    {
        foreach( var go in gos )
        {
            foreach( var mr in go.GetComponentsInChildren<MeshRenderer>())
            {
                for(int i = 0; i < mr.materials.Length; i++)
                {
                    mr.materials[i].color = c;
                }
            }
        }
    }

    IEnumerator ResetColor(GameObject[] gos)
    {
        yield return new WaitForSeconds(3);
        MarkColor(gos, oriColor);
    }
}
