using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ConvertCoordinates;

public class ImportDXF
{



    static string[] regels;
    static int regelnummer;
    [MenuItem("Tools/Importeer Maaiveld van DXF")]

    



    private static void ImporteerFBX()
    {
        regelnummer = 0;
        regels = System.IO.File.ReadAllLines(@"D:\git-repos\tile-67320-51836-XX gebieden.dxf");
        List<Vector3> vertices;
        Vector3 Vertex = new Vector3();
        long aantalVertices;
        long aantalFaces;
        List<int> tris;

        while (GotoObject("AcDbPolyFaceMesh"))
        {
            
            aantalVertices = System.Convert.ToInt32(GetValue("71"));
            aantalFaces = System.Convert.ToInt32(GetValue("72"));
            vertices = new List<Vector3>();
            tris = new List<int>();
            Debug.Log(aantalVertices + " vertices");
            Debug.Log(aantalFaces + " faces");
            string laagnaam = GetValue("8");
            Debug.Log("level: " + laagnaam);

            for (long i = 0; i < aantalVertices; i++)
            {
                GotoObject("AcDbPolyFaceMeshVertex");
                double dbl;
                double.TryParse(GetValue("10"), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
                float X = (float)dbl;
                double.TryParse(GetValue("20"), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
                float Y = (float)dbl;
                double.TryParse(GetValue("30"), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
                float Z = (float)dbl;
                
                if (X <100000)
                {
                    break;
                }
                Vertex = CoordConvert.RDtoUnity(new Vector3(X, Y, Z));
                vertices.Add(Vertex);
                
            }
            Debug.Log("vertices ingelezen");
            for (long i = 0; i < aantalFaces; i++)
            {
                GotoObject("AcDbFaceRecord");
                string waarde = GetValue("71");
                waarde = waarde.Replace("-", "");
                int X = int.Parse(waarde)-1;
                tris.Add(X);
                waarde = GetValue("72");
                waarde = waarde.Replace("-", "");
                X = int.Parse(waarde) - 1;
                tris.Add(X);
                waarde = GetValue("73");
                waarde = waarde.Replace("-", "");
                X = int.Parse(waarde) - 1;
                
                tris.Add(X);
            }
            Debug.Log("Faces Ingelezen");


            Mesh msh = new Mesh();
            msh.vertices = vertices.ToArray();
            msh.triangles = tris.ToArray();
            msh.RecalculateNormals();
            msh.RecalculateBounds();
            GameObject Nieuw = new GameObject(laagnaam);
            Nieuw.AddComponent(typeof(MeshRenderer));
            MeshFilter filter = Nieuw.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.mesh = msh;

            Debug.Log("Mesh Gemaakt");
        }
    }

    private static string VindtLaagnaam()
    {
        string laagnaam = "";
        bool gevonden = false;
        while (gevonden == false)
        {
            regelnummer += 1;
            if (regels[regelnummer].Trim() == "8")
            {
                regelnummer += 1;
                gevonden = true;
                laagnaam = regels[regelnummer].Trim();
            }
        }
        return laagnaam;
    }

    private static bool GotoObject( string code)
    {
        bool gevonden = false;
        while (gevonden == false && regelnummer<regels.Length-1)
        {
            regelnummer += 1;
            //Debug.Log(regelnummer + " van " + regels.Length);
            if (regels[regelnummer] == code)
            {
                gevonden = true;
            }           
        }
        return gevonden;
    }

    private static string GetValue(string code)
    {
        string output = "";
        bool gevonden = false;
        while (gevonden == false)
        {
            regelnummer += 1;
            if (regels[regelnummer].Trim() == code)
            {
                regelnummer += 1;
                gevonden = true;
                output = regels[regelnummer].Trim();
            }
        }
        return output;
    }
}
