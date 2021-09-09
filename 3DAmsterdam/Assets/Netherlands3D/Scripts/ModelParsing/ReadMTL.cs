using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
namespace Netherlands3D.ModelParsing
{
    public class ReadMTL : MonoBehaviour
    {
        // MTL File Tags
        const string NML = "newmtl";
        const string NS = "Ns"; // Shininess
        const string KA = "Ka"; // Ambient component (not supported)
        const string KD = "Kd"; // Diffuse component
        const string KS = "Ks"; // Specular component
        const string D = "d";   // Transparency (not supported)
        const string TR = "Tr"; // Same as 'd'
        const string ILLUM = "illum"; // Illumination model. 1 - diffuse, 2 - specular
        const string MAP_KA = "map_Ka"; // Ambient texture
        const string MAP_KD = "map_Kd"; // Diffuse texture
        const string MAP_KS = "map_Ks"; // Specular texture
        const string MAP_KE = "map_Ke"; // Emissive texture
        const string MAP_BUMP = "map_bump"; // Bump map texture
        const string BUMP = "bump"; // Bump map texture

        string mtllib;

        private int totalDataLines = 0;

        private string line;
        private string[] linePart;

        private const char faceSplitChar = '/';
        private const char lineSplitChar = '\n';
        private const char linePartSplitChar = ' ';

        private MaterialData targetMaterialData;
        private int parseLinePointer = 0;

        [SerializeField]
        private GeometryBuffer buffer;
        // Materials
        private List<MaterialData> materialDataSlots;

        private StringReader stringReader;

        public bool isBusy=false;

        public List<MaterialData> GetMaterialData()
        {
            if (materialDataSlots==null)
            {
                materialDataSlots = new List<MaterialData>();
            }
            return materialDataSlots;
        }

        //	/// <summary>
        //	/// Sets the material string and turns in into an array with every newline
        //	/// </summary>
        //	/// <param name="data">obj string</param>
        public void StartMTLParse(ref string data)
        {
            stringReader = new StringReader(data);

            //Count newlines
            totalDataLines = 0;
            foreach (var character in data)
            {
                if (character == lineSplitChar) totalDataLines++;
            }

            parseLinePointer = 0;
            materialDataSlots = new List<MaterialData>();
            StartCoroutine(ParseMTLFile());
        }

        private IEnumerator ParseMTLFile()
        {
            isBusy = true;
            int maxLinesPerFrame = 1000;
            int remainingLinesToParse = ParseNextMtlLines(1);
            int totalLines = remainingLinesToParse;

            //loadingObjScreen.ShowMessage("Materialen worden geladen...");
            while (remainingLinesToParse > 0)
            {
                remainingLinesToParse = ParseNextMtlLines(maxLinesPerFrame);
                float percentage = 1.0f - (remainingLinesToParse / totalLines);
                //loadingObjScreen.ProgressBar.Percentage(percentage / 100.0f); //Show first percent
                yield return null;
            }

            isBusy = false;
        }

        public int ParseNextMtlLines(int maxLines)
        {
            
            int currentLine = 0;
            while (parseLinePointer < totalDataLines && currentLine < maxLines)
            {
                parseLinePointer++;
                currentLine++;
                line = stringReader.ReadLine();
                if (line != null)
                {
                    ParseMtlLine(ref line);
                }
                else
                {
                    //No lines remain
                    return -1;
                }
            }
            return totalDataLines - parseLinePointer;
        }

        private void ParseMtlLine(ref string mtlLine)
        {
            if (mtlLine.IndexOf("#") != -1) mtlLine = line.Substring(0, mtlLine.IndexOf("#"));
            linePart = mtlLine.Trim().Split(linePartSplitChar);

            if (linePart[0].Trim() != "")
            {
                switch (linePart[0])
                {
                    case NML:
                        targetMaterialData = new MaterialData();
                        targetMaterialData.Name = linePart[1].Trim();
                        materialDataSlots.Add(targetMaterialData);
                        break;
                    case KA:
                        targetMaterialData.Ambient = gc(linePart);
                        break;
                    case KD:
                        targetMaterialData.Diffuse = gc(linePart);
                        break;
                    case KS:
                        targetMaterialData.Specular = gc(linePart);
                        break;
                    case NS:
                        targetMaterialData.Shininess = cf(linePart[1]) / 1000;
                        break;
                    case D:
                    case TR:
                        targetMaterialData.Alpha = cf(linePart[1]);
                        break;
                    case MAP_KD:
                        targetMaterialData.DiffuseTexPath = linePart[linePart.Length - 1].Trim();
                        break;
                    case MAP_BUMP:
                    case BUMP:
                        BumpParameter(targetMaterialData, linePart);
                        break;
                    case ILLUM:
                        targetMaterialData.IllumType = ci(linePart[1]);
                        break;
                        /*default:
                            Debug.Log("this line was not processed :" + line); //Skip logging for the sake of WebGL performance
                            break;*/
                }
            }
        }
        static Material GetMaterial(MaterialData md, Material sourceMaterial)
        {
            Material newMaterial;

            if (md.IllumType == 2)
            {
                newMaterial = new Material(sourceMaterial);
                newMaterial.SetFloat("_EmissionColor", md.Shininess);
            }
            else
            {
                newMaterial = new Material(sourceMaterial);
            }

            if (md.DiffuseTex != null)
            {
                newMaterial.SetTexture("_MainTex", md.DiffuseTex);
            }
            else
            {
                newMaterial.SetColor("_BaseColor", md.Diffuse);
            }
            if (md.BumpTex != null) newMaterial.SetTexture("_BumpMap", md.BumpTex);

            newMaterial.name = md.Name;

            return newMaterial;
        }

        class BumpParamDef
        {
            public string OptionName;
            public string ValueType;
            public int ValueNumMin;
            public int ValueNumMax;
            public BumpParamDef(string name, string type, int numMin, int numMax)
            {
                OptionName = name;
                ValueType = type;
                ValueNumMin = numMin;
                ValueNumMax = numMax;
            }
        }

        static float cf(string v)
        {
            try
            {
                return float.Parse(v, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                print(e + " -> " + v);
                return 0;
            }
        }
        static int ci(string v)
        {
            try
            {
                return int.Parse(v);
            }
            catch (Exception e)
            {
                print(e + " -> " + v);
                return 0;
            }
        }
        static void BumpParameter(MaterialData m, string[] p)
        {
            var regexNumber = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");

            var bumpParams = new Dictionary<String, BumpParamDef>();
            bumpParams.Add("bm", new BumpParamDef("bm", "string", 1, 1));
            bumpParams.Add("clamp", new BumpParamDef("clamp", "string", 1, 1));
            bumpParams.Add("blendu", new BumpParamDef("blendu", "string", 1, 1));
            bumpParams.Add("blendv", new BumpParamDef("blendv", "string", 1, 1));
            bumpParams.Add("imfchan", new BumpParamDef("imfchan", "string", 1, 1));
            bumpParams.Add("mm", new BumpParamDef("mm", "string", 1, 1));
            bumpParams.Add("o", new BumpParamDef("o", "number", 1, 3));
            bumpParams.Add("s", new BumpParamDef("s", "number", 1, 3));
            bumpParams.Add("t", new BumpParamDef("t", "number", 1, 3));
            bumpParams.Add("texres", new BumpParamDef("texres", "string", 1, 1));
            int pos = 1;
            string filename = null;
            while (pos < p.Length)
            {
                if (!p[pos].StartsWith("-"))
                {
                    filename = p[pos];
                    pos++;
                    continue;
                }
                // option processing
                string optionName = p[pos].Substring(1);
                pos++;
                if (!bumpParams.ContainsKey(optionName))
                {
                    continue;
                }
                BumpParamDef def = bumpParams[optionName];
                var args = new ArrayList();
                int i = 0;
                bool isOptionNotEnough = false;
                for (; i < def.ValueNumMin; i++, pos++)
                {
                    if (pos >= p.Length)
                    {
                        isOptionNotEnough = true;
                        break;
                    }
                    if (def.ValueType == "number")
                    {
                        Match match = regexNumber.Match(p[pos]);
                        if (!match.Success)
                        {
                            isOptionNotEnough = true;
                            break;
                        }
                    }
                    args.Add(p[pos]);
                }
                if (isOptionNotEnough)
                {
                    Debug.Log("Bump variable value not enough for option:" + optionName + " of material:" + m.Name);
                    continue;
                }
                for (; i < def.ValueNumMax && pos < p.Length; i++, pos++)
                {
                    if (def.ValueType == "number")
                    {
                        Match match = regexNumber.Match(p[pos]);
                        if (!match.Success)
                        {
                            break;
                        }
                    }
                    args.Add(p[pos]);
                }
                // TODO: some processing of options
                Debug.Log("Found option: " + optionName + " of material: " + m.Name + " args: " + String.Concat(args.ToArray()));
            }
            if (filename != null)
            {
                m.BumpTexPath = filename;
            }
        }

        static Color gc(IList<string> p)
        {
            return new Color(cf(p[1]), cf(p[2]), cf(p[3]));
        }

        public class MaterialData
        {
            public string Name;
            public Color Ambient;
            public Color Diffuse;
            public Color Specular;
            public float Shininess;
            public float Alpha;
            public int IllumType;
            public string DiffuseTexPath;
            public string BumpTexPath;
            public Texture2D DiffuseTex;
            public Texture2D BumpTex;
        }
    }
   
}