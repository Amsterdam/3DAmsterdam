using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Linq;
using System;

public class ObjExporter
{
    public struct MeshMaterials
    {
        public Mesh m;
        public string[] mats;
    }

    static int AppendMesh(StringBuilder sb, MeshFilter mf, Matrix4x4 preTransform, int vertexOffset)
    {
        Mesh m = mf.sharedMesh;
        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

        sb.Append("g ").Append(mf.name).Append("\n");
        foreach (Vector3 v in m.vertices)
        {
            Matrix4x4 finalMat = preTransform * mf.transform.localToWorldMatrix;
            Vector3 vt = finalMat.MultiplyPoint(v);
            sb.Append($"v {vt.x} {vt.y} {vt.z}\n");
        }
        sb.Append("\n");
        var normals = m.normals;
        if (normals == null || normals.Length == 0)
            normals = new Vector3[m.vertexCount];
        foreach (Vector3 n in normals)
        {
            Matrix4x4 finalMat = preTransform * mf.transform.localToWorldMatrix;
            Vector3 nt = finalMat.MultiplyVector(n);
            sb.Append($"vn {nt.x} {nt.y} {nt.z}\n");
        }
        sb.Append("\n");
        var uvs = m.uv;
        if (uvs == null || uvs.Length == 0)
            uvs = new Vector2[m.vertexCount];
        foreach (Vector2 uv in uvs)
        {
            sb.Append($"vt {uv.x} {uv.y}\n");
        }
        var vo = vertexOffset;
        int count = Mathf.Min(m.subMeshCount, mats.Length);
        for (int material = 0; material < count; material++)
        {
         //   sb.Append("\n");

            var matName = MtlExporter.GetMatName(mats[material]);

            sb.Append("usemtl ").Append(matName).Append("\n");
            sb.Append("usemap ").Append(matName).Append("\n");

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

    static Mesh FinishMesh(ref List<Vector3> vertices, ref List<Vector3> normals, ref List<Vector2> uvs, ref List<List<int>> indices)
    {
        Mesh m = new Mesh();
        m.SetVertices(vertices);
        m.SetNormals(normals);
        m.SetUVs(0, uvs);
        int k = 0;
        foreach (var l in indices)
        {
            m.SetIndices(l.ToArray(), MeshTopology.Triangles, k++);
        }
        vertices.Clear();
        normals.Clear();
        uvs.Clear();
        indices.Clear();
        return m;
    }

    public static List<MeshMaterials> ObjToMesh(string objData)
    {
        if (string.IsNullOrEmpty(objData))
            return null;

        List<MeshMaterials> meshMats = new List<MeshMaterials>();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<List<int>> indices = new List<List<int>>();
        List<string> mats = new List<string>();

        using (StringReader sr = new StringReader(objData))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length > 0)
                {
                    char t = line[0];
                    string[] el = line.Split( new char[] { ' ' }, StringSplitOptions.None);
                    Vector3 v = Vector3.zero;
                    switch (el[0])
                    {
                        case "g":
                            if (vertices.Count != 0)
                            {
                                Mesh m = FinishMesh(ref vertices, ref normals, ref uvs, ref indices);
                                MeshMaterials mm = new MeshMaterials();
                                mm.m = m;
                                mm.mats = mats.ToArray();
                                meshMats.Add(mm);
                                mats.Clear();
                            }
                            break;

                        case "v":
                            v.x = Convert.ToSingle(el[1]);
                            v.y = Convert.ToSingle(el[2]);
                            v.z = Convert.ToSingle(el[3]);
                            vertices.Add(v);
                            break;

                        case "vn":
                            v.x = Convert.ToSingle(el[1]);
                            v.y = Convert.ToSingle(el[2]);
                            v.z = Convert.ToSingle(el[3]);
                            normals.Add(v);
                            break;

                        case "vt":
                            v.x = Convert.ToSingle(el[1]);
                            v.y = Convert.ToSingle(el[2]);
                            uvs.Add(v);
                            break;

                        case "f":
                            indices.Last().Add(Convert.ToInt32(el[1])-1);
                            indices.Last().Add(Convert.ToInt32(el[2])-1);
                            indices.Last().Add(Convert.ToInt32(el[3])-1);
                            break;

                        case "usemtl":
                            mats.Add(el[1]);
                            indices.Add(new List<int>());
                            break;
                    }
                }
            }
        }

        if (vertices.Count != 0)
        {
            Mesh m = FinishMesh(ref vertices, ref normals, ref uvs, ref indices);
            MeshMaterials mm = new MeshMaterials();
            mm.m = m;
            mm.mats = mats.ToArray();
            meshMats.Add(mm);
        }

        return meshMats;
    }

    public static string WriteObjToString(string mtlLib, MeshFilter mf, Matrix4x4 preTransform)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("mtllib ").Append(mtlLib).Append("\n");
        AppendMesh(sb, mf, preTransform, 0);
        return sb.ToString();
    }

    public static string WriteObjToString(string mtlLib, MeshFilter[] mfs, Matrix4x4 preTransform)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("mtllib ").Append(mtlLib).Append("\n");
        int vertexOffset = 0;
        HashSet<int> objs = new HashSet<int>();
        foreach (var mf in mfs)
        {
            if (objs.Contains(mf.GetInstanceID()))
                continue;
            objs.Add(mf.GetInstanceID());
            vertexOffset += AppendMesh(sb, mf, preTransform, vertexOffset);
            sb.Append('\n');
        }
        return sb.ToString();
    }

    public static void WriteObjToFile(MeshFilter mf, string filename)
    {
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(WriteObjToString(Path.GetFileNameWithoutExtension(filename) + ".mtl", mf, Matrix4x4.identity));
        }
    }
}