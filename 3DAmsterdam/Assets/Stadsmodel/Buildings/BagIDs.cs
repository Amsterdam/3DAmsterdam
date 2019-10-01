using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagIDs : MonoBehaviour
{
    public Dictionary<string, float> gebouwenlijst = new Dictionary<string, float>();
    //public List<string> HighlightPanden = new List<string>();
    private MeshRenderer mr;

    public void SetHighlight(List<string> HighlightPanden)
    {
        int nummer = 0;
        float[] renderList = new float[1000];
        for (int i = 0; i < HighlightPanden.Count; i++)
        {
            if (gebouwenlijst.ContainsKey(HighlightPanden[i]))
            {
                renderList[nummer] = gebouwenlijst[HighlightPanden[i]];
                nummer++;
            }
        }

        mr = transform.GetComponent<MeshRenderer>();
        mr.material.SetInt("_SegmentsCount", nummer);
        if (nummer > 0)
        {
            mr.material.SetFloatArray("_pandcodes", renderList);
        }


    }

}
