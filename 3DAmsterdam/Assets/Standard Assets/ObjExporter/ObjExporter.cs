using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Linq;

public class ObjExporter : MonoBehaviour
{
    static int AppendMesh(StringBuilder sb, MeshFilter mf, bool preTransform, int vertexOffset)
    {
        Mesh m = mf.sharedMesh;
        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

        sb.Append("g ").Append(mf.name).Append("\n");
        foreach (Vector3 v in m.vertices)
        {
            Vector3 vt = v;
            if (preTransform) vt = mf.transform.TransformPoint(v);
            sb.Append($"v {vt.x} {vt.y} {vt.z}\n");
        }
        sb.Append("\n");
        var normals = m.normals;
        if (normals == null || normals.Length == 0)
            normals = new Vector3[m.vertexCount];
        foreach (Vector3 n in normals)
        {
            Vector3 nt = n;
            if (preTransform) nt = mf.transform.TransformVector(n);
            sb.Append($"vn {nt.x} {nt.y} {nt.z}\n");
        }
        sb.Append("\n");
        var uvs = m.uv;
        if (uvs == null || uvs.Length == 0)
            uvs = new Vector2[m.vertexCount];
        foreach (Vector3 uv in uvs)
        {
            sb.Append($"vt {uv.x} {uv.y}\n");
        }
        var vo = vertexOffset;
        for (int material = 0; material < m.subMeshCount; material++)
        {
            sb.Append("\n");
            var cleanMatName = CleanMaterialName(mats[material].name);
            sb.Append("usemtl ").Append(cleanMatName).Append("\n");
            sb.Append("usemap ").Append(cleanMatName).Append("\n");

            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int i0 = triangles[i + 0] + 1 + vo;
                int i1 = triangles[i + 1] + 1 + vo;
                int i2 = triangles[i + 2] + 1 + vo;
                sb.Append($"f {i0} {i1} {i2}\n");
            }
        }
        return m.vertexCount;
    }

    static void WriteColor(StringBuilder sb, Material mat, string id, string keyword)
    {
        if ( mat.HasProperty(keyword))
        {
            var c = mat.GetColor(keyword);
            sb.AppendLine($"{id} {c.r} {c.g} {c.b}");
        }
    }

    static void WriteTextureAndAssignName(StringBuilder sb, Material mat, string id, string keyword, string extension, HashSet<Texture2D> textures)
    {
        if ( mat.HasProperty(keyword))
        {
            var c = mat.GetTexture(keyword) as Texture2D;
            if (c &&!textures.Contains(c))
            {
                textures.Add(c);
                var texName = $"texture{textures.Count}" + extension;
                c.name = texName;
                sb.AppendLine($"{id} {texName}");
            }
        }
    }

    static string CleanMaterialName(string name)
    {
        name = SuperReplace(@"!@#$%&*()-',./\{}+~` ", '_', name);
        return name.Trim();
    }

    static string SuperReplace(string chars, char replace, string input)
    {
        foreach( var c in chars )
        {
            input = input.Replace(c, replace);
        }
        return input;
    }

    static void GetTexture(Material mat, string keyword, HashSet<Texture2D> textures)
    {
        if ( mat.HasProperty(keyword))
        {
            var t = mat.GetTexture(keyword) as Texture2D;
            if ( t != null && !textures.Contains(t) )
            {
                textures.Add(t);
            }
        }
    }

    // ---------- Public accessors ----------------------------------------------------------------------------

    public static string WriteMaterial(MeshFilter[] mfs, StringBuilder sb = null, HashSet<string> materials = null, HashSet<Texture2D> textures = null)
    {
        if (sb == null)
            sb = new StringBuilder();
        if (materials == null)
            materials = new HashSet<string>();
        if (textures == null)
            textures = new HashSet<Texture2D>();
        int kWrittenMaterials = 0;
        foreach (var mf in mfs)
        {
            var mr = mf.GetComponent<MeshRenderer>();
            if (mr == null) continue;
            for (int i = 0; i < mr.sharedMaterials.Length; i++)
            {
                var mat = mr.sharedMaterials[i];
                string name = CleanMaterialName(mat.name);
                if (materials.Contains(name))
                    continue;
                materials.Add(name);

                if (kWrittenMaterials != 0) sb.AppendLine();
                sb.Append($"newmtl {name}\n");

                if (mat.HasProperty("_Glossiness*"))
                    sb.Append($"Ns {mat.GetFloat("_Glossiness") * 1000}\n");

                if (mat.HasProperty("_Color"))
                    sb.Append($"d {1.0f - mat.GetColor("_Color").a}\n");

                WriteColor(sb, mat, "Kd", "_Color");
                WriteColor(sb, mat, "Ks", "_SpecColor");
                WriteColor(sb, mat, "Ka", "_EmissionColor");

                WriteTextureAndAssignName(sb, mat, "map_Kd", "_MainTex", ".png", textures);
                WriteTextureAndAssignName(sb, mat, "map_Disp", "_BumpMap", ".png", textures);
                WriteTextureAndAssignName(sb, mat, "map_Ka", "_EmissionMap", ".png", textures);

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
        return sb.ToString();
    }

    public static string MeshToString(MeshFilter mf, bool preTransform)
    {
        StringBuilder sb = new StringBuilder();
        AppendMesh(sb, mf, preTransform, 0);
        return sb.ToString();
    }

    public static string GameObjectsToString(GameObject[] gos, bool preTransform)
    {
        StringBuilder sb = new StringBuilder();
        int vertexOffset = 0;
        foreach (var go in gos)
        {
            var mfs = go.GetComponentsInChildren<MeshFilter>();
            foreach (var mf in mfs)
            {
                vertexOffset += AppendMesh(sb, mf, preTransform, vertexOffset);
                sb.Append('\n');
            }
        }
        return sb.ToString();
    }

    public static string GameObjectsMaterialToString(GameObject[] gos)
    {
        StringBuilder sb = new StringBuilder();
        HashSet<string> materials = new HashSet<string>();
        HashSet<Texture2D> textures = new HashSet<Texture2D>();
        foreach (var go in gos)
        {
            var mfs = go.GetComponentsInChildren<MeshFilter>();
            WriteMaterial(mfs, sb, materials, textures);
        }
        return sb.ToString();
    }

    public static string MeshFiltersToString(MeshFilter[] mfs, bool preTransform)
    {
        StringBuilder sb = new StringBuilder();
        int vertexOffset = 0;
        foreach (var mf in mfs)
        {
            vertexOffset += AppendMesh(sb, mf, preTransform, vertexOffset);
            sb.Append('\n');
        }
        return sb.ToString();
    }

    public static void MeshToFile(MeshFilter mf, string filename)
    {
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(MeshToString(mf, false));
        }
    }

    public static Texture2D [] GetUniqueTextures(GameObject [] gos)
    {
        HashSet<Texture2D> textures = new HashSet<Texture2D>();
        foreach( var go in gos )
        {
            var mfs = go.GetComponentsInChildren<MeshFilter>();
            GetUniqueTextures(mfs, textures);
        }
        return textures.ToArray();
    }

    public static Texture2D [] GetUniqueTextures(MeshFilter [] mfs, HashSet<Texture2D> textures = null)
    {
        if (textures == null)
            textures = new HashSet<Texture2D>();
        foreach( var mf in mfs )
        {
            var mr = mf.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                foreach( var mat in mr.sharedMaterials)
                {
                    GetTexture(mat, "_MainTex", textures);
                    GetTexture(mat, "_BumpMap", textures);
                    GetTexture(mat, "_EmissionMap", textures);
                }
            }
        }
        return textures.ToArray();
    }
}