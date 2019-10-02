using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMapping : MonoBehaviour
{
    public Dictionary<string, float> Objectenlijst = new Dictionary<string, float>();
    private MeshRenderer mr;
    public Material HighlightMaterial;
    public Material DefaultMaterial;

    public void SetHighlight(List<string> HighlightObjecten)
    {
        int nummer = 0;
        float[] renderList = new float[1000];
        for (int i = 0; i < HighlightObjecten.Count; i++)
        {
            if (Objectenlijst.ContainsKey(HighlightObjecten[i]))
            {
                renderList[nummer] = Objectenlijst[HighlightObjecten[i]];
                nummer++;
            }
        }

        mr = transform.GetComponent<MeshRenderer>();
        
        if (nummer > 0)
        {
            mr.material = HighlightMaterial;
            mr.material.color = DefaultMaterial.color;
            mr.material.SetInt("_SegmentsCount", nummer);
            mr.material.SetFloatArray("_pandcodes", renderList);
        }
        else
        {
            mr.sharedMaterial = DefaultMaterial;
        }


    }

}
