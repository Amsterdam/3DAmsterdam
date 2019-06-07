using UnityEngine;
using System.Collections;
using Serialize;
using static ObjExporter;
using System.Collections.Generic;

public class BuildingDeserializer : MonoBehaviour
{
    public void Deserialize(Building b, MeshMaterials [] meshMats, Dictionary<string, Material> mats)
    {
        Material mDef = new Material(Shader.Find("Standard"));

        foreach (var mm in meshMats)
        {
            GameObject child = new GameObject();
            child.transform.parent = transform.parent;
            child.AddComponent<MeshFilter>().sharedMesh = mm.m;
            Material[] childMats = new Material[mm.mats.Length];
            for (int i = 0; i < childMats.Length; i++)
            {
                Material foundMat;
                if (!mats.TryGetValue(mm.mats[i], out foundMat))
                {
                    foundMat = mDef;
                }
                childMats[i] = foundMat;
            }
            child.AddComponent<MeshRenderer>().sharedMaterials = childMats;
        }
        transform.position = b.pos.ToUnity();
        transform.rotation = b.rot.ToUnity();
        transform.localScale = b.scale.ToUnity();

        FetchTextures();
    }

    void FetchTextures()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i);
            var mf = c.GetComponent<MeshFilter>();
            var mr = c.GetComponent<MeshRenderer>();
            if (mf == null || mr == null) continue;
            for (int j = 0; j < mr.sharedMaterials.Length; j++)
            {
                FetchTexture(mr.sharedMaterials[j], "_MainTex");
                FetchTexture(mr.sharedMaterials[j], "_BumpMap");
                FetchTexture(mr.sharedMaterials[j], "_EnvironmentMap");
            }
        }
    }

    void FetchTexture(Material m, string texKeyName)
    {
        if (m.HasProperty(texKeyName))
        {
            var tex = m.GetTexture(texKeyName);
            string filename = tex.name;
            MtlExporter.GetTextureFromCacheOrDownload(filename, (Texture2D tex2, bool succes) =>
            {
                if (this == null) return;
                if (!succes) return;
                m.SetTexture(texKeyName, tex2);
            });
        }
    }
}
