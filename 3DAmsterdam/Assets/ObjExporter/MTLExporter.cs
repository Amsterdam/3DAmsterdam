using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


public class MtlExporter
{
    static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

    public static string GetMatName(Material mat)
    {
        return ((uint)mat.GetInstanceID()).ToString();
       // return mat.name;
        //return Hash128.Parse(mat.name).ToString();
    }

    public static string GetTexName(Texture2D tex)
    {
        return tex.imageContentsHash.ToString();
    }

    public static Dictionary<string, Material> MtlToMaterials(string mtlData)
    {
        if (string.IsNullOrEmpty(mtlData)) return null;
        Dictionary<string, Material> mats = new Dictionary<string, Material>();
        Shader s = Shader.Find("Standard");
        Color c;
        Texture2D tex;
        Material mat = new Material(s);
        
        using (StringReader sr = new StringReader(mtlData))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length > 0)
                {
                    string[] el = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    switch (el[0])
                    {
                        case "newmtl":
                            mat = new Material(s);
                            mat.name = el[1];
                            if (!mats.ContainsKey(mat.name))
                                mats.Add(mat.name, mat);
                            mat.name = el[1];
                            break;

                        case "Ns":
                            mat.SetFloat("_Glossiness", Convert.ToSingle(el[1], CultureInfo.InvariantCulture) / 1000);
                            break;

                        case "d":
                            c = mat.color;
                            mat.color = new Color(c.r, c.g, c.b, Convert.ToSingle(el[1], CultureInfo.InvariantCulture));
                            break;

                        case "Tr":
                            c = mat.color;
                            mat.color = new Color(c.r, c.g, c.b, 1.0f - Convert.ToSingle(el[1], CultureInfo.InvariantCulture));
                            break;

                        case "Kd":
                        case "kd":
                            c = mat.color;
                            mat.color = new Color(Convert.ToSingle(el[1], CultureInfo.InvariantCulture), Convert.ToSingle(el[2], CultureInfo.InvariantCulture), Convert.ToSingle(el[3], CultureInfo.InvariantCulture), c.a);
                            break;

                        case "Ks":
                        case "ks":
                            mat.SetColor("_SpecColor", new Color(Convert.ToSingle(el[1], CultureInfo.InvariantCulture), Convert.ToSingle(el[2], CultureInfo.InvariantCulture), Convert.ToSingle(el[3], CultureInfo.InvariantCulture)));
                            break;

                        case "Ka":
                        case "ka":
                            mat.SetColor("_EmissionColor", new Color(Convert.ToSingle(el[1], CultureInfo.InvariantCulture), Convert.ToSingle(el[2], CultureInfo.InvariantCulture), Convert.ToSingle(el[3], CultureInfo.InvariantCulture)));
                            break;

                        case "map_Kd":
                            tex = new Texture2D(2, 2);
                            tex.name = el[1];
                            mat.SetTexture("_MainTex", tex);
                            break;

                        case "map_Disp":
                            tex = new Texture2D(2, 2);
                            tex.name = el[1];
                            mat.SetTexture("_BumpMap", tex);
                            break;

                        case "map_Ka":
                            tex = new Texture2D(2, 2);
                            tex.name = el[1];
                            mat.SetTexture("_EmissionMap", tex);
                            break;
                    }
                }
            }
        }

        return mats;
    }

    static void AddTextureIfIsUnique(Dictionary<string, Texture2D> textures, Material mat, string keyWord)
    {
        if (mat.HasProperty(keyWord))
        {
            var tex = mat.GetTexture(keyWord) as Texture2D;
            if (tex == null) return;
            string hash = tex.imageContentsHash.ToString();
            if (!textures.ContainsKey(hash)) textures.Add(hash, tex);
        }
    }

    public static Texture2D [] GetUniqueTextures(MeshFilter[] mfs)
    {
        Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        foreach (var mf in mfs)
        {
            var mr = mf.GetComponent<MeshRenderer>();
            if (mr != null && mf != null)
            {
                for (int i = 0; i < mr.sharedMaterials.Length; i++)
                {
                    AddTextureIfIsUnique(textures, mr.sharedMaterials[i], "_MainTex");
                    AddTextureIfIsUnique(textures, mr.sharedMaterials[i], "_BumpMap");
                    AddTextureIfIsUnique(textures, mr.sharedMaterials[i], "_EmissionMap");
                }
            }
        }
        return textures.Select(kvp => kvp.Value).ToArray();
    }

    static void WriteColor(StringBuilder sb, Material mat, string id, string keyword)
    {
        if (mat.HasProperty(keyword))
        {
            var c = mat.GetColor(keyword);
            sb.AppendLine($"{id} {c.r} {c.g} {c.b}".Replace(',', '.'));
        }
        else sb.AppendLine($"{id} {0.0f} {0.0f} {0.0f}".Replace(',', '.'));
    }

    static void WriteTexture(StringBuilder sb, Material mat, string id, string keyword)
    {
        if (mat.HasProperty(keyword))
        {
            var tex = mat.GetTexture(keyword) as Texture2D;
            if (tex != null)
            {
                var texName = GetTexName(tex);
                sb.AppendLine($"{id} {texName}.png");
            }
        }
    }

    public static StringBuilder WriteMaterialToString(MeshFilter[] mfs, StringBuilder sb = null)
    {
        if (sb == null)
            sb = new StringBuilder();
        HashSet<string> materials = new HashSet<string>();
        int kWrittenMaterials = 0;
        foreach (var mf in mfs)
        {
            var mr = mf.GetComponent<MeshRenderer>();
            if (mr == null || mf == null) continue;
            for (int i = 0; i < mr.sharedMaterials.Length; i++)
            {
                var mat = mr.sharedMaterials[i];
                string name = GetMatName(mat);
                if (materials.Contains(name))
                    continue;
                materials.Add(name);

             //   if (kWrittenMaterials != 0) sb.AppendLine();
                sb.Append($"newmtl {name}\n");

                if (mat.HasProperty("_Glossiness*"))
                    sb.Append($"Ns {mat.GetFloat("_Glossiness") * 1000}\n".Replace(',', '.'));
                else
                    sb.Append($"Ns {0.0f}\n");

                if (mat.HasProperty("_Color"))
                    sb.Append($"d {mat.GetColor("_Color").a}\n".Replace(',', '.'));
                else
                    sb.Append($"d {1.0f}\n".Replace(',', '.'));

                sb.Append("illum 2\n");

                WriteColor(sb, mat, "Kd", "_Color");
                WriteColor(sb, mat, "Ks", "_SpecColor");
                WriteColor(sb, mat, "Ka", "_EmissionColor");

                WriteTexture(sb, mat, "map_Kd", "_MainTex");
                WriteTexture(sb, mat, "map_Disp", "_BumpMap");
                WriteTexture(sb, mat, "map_Ka", "_EmissionMap");

                //                newmtl floor
                //Ns 7.843137
                //Ka 0.000000 0.000000 0.000000
                //Kd 0.470400 0.470400 0.470400
                //Ks 0.000000 0.000000 0.000000
                //Ni 1.000000
                //d 0.000000
                //illum 2
                //map_Kd textures/ sponza_floor_a_diff.tga
                //map_Ka textures/ sponza_floor_a_diff.tga
                //map_Disp textures/ sponza_floor_a_ddn.tga

                kWrittenMaterials++;
            }
        }
        return sb;
    }

    public static void GetTextureFromCacheOrDownload(string filename, Action<Texture2D, bool> onDone)
    {
        Texture2D tex;
        if (textureCache.TryGetValue(filename, out tex))
        {
            onDone(tex, true);
            return;
        }

        Uploader.StartDownloadTexture(filename, (Texture2D tex2, bool succes) =>
        {
            if (succes)
            {
                if (!textureCache.ContainsKey(filename))
                    textureCache.Add(filename, tex2);
            }
            onDone(tex, succes);
        });
    }
}
