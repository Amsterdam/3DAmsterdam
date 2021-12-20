using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine.Networking;
using Netherlands3D.Core;

public class ObjLoad : MonoBehaviour
{
    // OBJ File Tags
    const string O = "o";
    const string V = "v";
    const string VT = "vt";
    const string VN = "vn";
    const string F = "f";
    const string MTLLIB = "mtllib";
    const string USEMTL = "usemtl";

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

    private bool splitNestedObjects = false;
    private bool ignoreObjectsOutsideOfBounds = false;
    private Vector2RD bottomLeftBounds;
    private Vector2RD topRightBounds;
    private bool RDCoordinates = false;
    private bool flipFaceDirection = false;
    private bool flipYZ = false;
    private bool weldVertices = false;
    private int maxSubMeshes = 0;
    private bool tracing = false;
    private bool enableMeshRenderer = true;

    /// <summary>
    /// Disabled is the default. Otherwise SketchUp models would have a loooot of submodels (we cant use batching for rendering in WebGL, so this is bad for performance)
    /// </summary>
    public bool SplitNestedObjects { get => splitNestedObjects; set => splitNestedObjects = value; }
    public bool ObjectUsesRDCoordinates { get => RDCoordinates; set => RDCoordinates = value; }
    public bool IgnoreObjectsOutsideOfBounds { get => ignoreObjectsOutsideOfBounds; set => ignoreObjectsOutsideOfBounds = value; }
    public bool FlipYZ { get => flipYZ; set => flipYZ = value; }
    public int MaxSubMeshes { get => maxSubMeshes; set => maxSubMeshes = value; }
    public bool FlipFaceDirection { get => flipFaceDirection; set => flipFaceDirection = value; }
    public bool WeldVertices { get => weldVertices; set => weldVertices = value; }
    public bool EnableMeshRenderer { get => enableMeshRenderer; set => enableMeshRenderer = value; }
    public Vector2RD BottomLeftBounds { get => bottomLeftBounds; set => bottomLeftBounds = value; }
    public Vector2RD TopRightBounds { get => topRightBounds; set => topRightBounds = value; }
    public GeometryBuffer Buffer { get => buffer; set => buffer = value; }

    private StringReader stringReader;

    //	/// <summary>
    //	/// Sets the obj string and turns it into an array with every newline
    //	/// </summary>
    //	/// <param name="data">obj string</param>
    public void SetGeometryData(ref string data)
    {
        //		buffer = new GeometryBuffer();
        //		stringReader = new StringReader(data);

        //		//Count newlines
        //		totalDataLines = 0;
        //		foreach (var character in data)
        //		{
        //			if (character == lineSplitChar) totalDataLines++;
        //		}

        //		parseLinePointer = 0;
    }
    //	/// <summary>
    //	/// Sets the material string and turns in into an array with every newline
    //	/// </summary>
    //	/// <param name="data">obj string</param>
    public void SetMaterialData(ref string data)
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
    }

    //	/// <summary>
    //	/// Read the next obj line
    //	/// </summary>
    //	/// <returns>How many lines remain to be parsed</returns>
    public int ParseNextObjLines(int maxLines)
    {
        return -1;
        //		int currentLine = 0;
        //		while (parseLinePointer < totalDataLines && currentLine < maxLines)
        //		{
        //			parseLinePointer++;
        //			currentLine++;
        //			line = stringReader.ReadLine();
        //			if (line != null)
        //			{
        //				if (!ParseObjLine(ref line))
        //				{
        //					return -1;
        //				}
        //			}
        //			else
        //			{
        //				//No lines remain
        //				return -1;
        //			}
        //		}

        //		return totalDataLines - parseLinePointer;
    }

    //	/// <summary>
    //	/// Parses a single objline
    //	/// </summary>
    //	/// <param name="objline">the obj line</param>
    //	/// <returns>Returns false on failure</returns>
    //	private bool ParseObjLine(ref string objline)
    //	{
    //		linePart = objline.Trim().Split(linePartSplitChar);
    //		switch (linePart[0])
    //		{
    //			case O:
    //				if (SplitNestedObjects) buffer.AddObject(linePart[1].Trim());
    //				break;
    //			case MTLLIB:
    //				mtllib = line.Substring(linePart[0].Length + 1).Trim();
    //				break;
    //			case USEMTL:
    //				if (MaxSubMeshes == 0 || buffer.currentObjectData.SubMeshGroups.Count < MaxSubMeshes)
    //					buffer.AddSubMeshGroup(linePart[1].Trim());
    //				break;
    //			case V:
    //				if (buffer.Vertices.Count == 0)
    //				{
    //					if (CoordConvert.RDIsValid(new Vector3RD(cd(linePart[1]), -cd(linePart[3]), cd(linePart[2]))))
    //					{
    //						flipYZ = false;
    //						ObjectUsesRDCoordinates = true;
    //					}
    //					else if (CoordConvert.RDIsValid(new Vector3RD(cd(linePart[1]), cd(linePart[2]), cd(linePart[3]))))
    //					{
    //						flipYZ = true;
    //						ObjectUsesRDCoordinates = true;
    //					}
    //				}
    //				if (ObjectUsesRDCoordinates)
    //				{
    //					if (flipYZ)
    //					{
    //						buffer.PushVertex(CoordConvert.RDtoUnity(new Vector3RD(cd(linePart[1]), cd(linePart[2]), cd(linePart[3]))));
    //					}
    //					else
    //					{
    //						buffer.PushVertex(CoordConvert.RDtoUnity(new Vector3RD(cd(linePart[1]), -cd(linePart[3]), cd(linePart[2]))));
    //					}
    //				}
    //				else
    //				{
    //					if (flipYZ)
    //					{
    //						buffer.PushVertex(new Vector3(cf(linePart[1]), cf(linePart[3]), cf(linePart[2])));
    //					}
    //					else
    //					{
    //						buffer.PushVertex(new Vector3(cf(linePart[1]), cf(linePart[2]), -cf(linePart[3])));
    //					}
    //				}
    //				break;
    //			case VT:
    //				buffer.PushUV(new Vector2(cf(linePart[1]), cf(linePart[2])));
    //				break;
    //			case VN:
    //				buffer.PushNormal(new Vector3(cf(linePart[1]), cf(linePart[2]), -cf(linePart[3])));
    //				break;
    //			case F:
    //				var faces = new FaceIndices[linePart.Length - 1];
    //				GetFaceIndices(faces, linePart);
    //				if (linePart.Length == 4)
    //				{
    //					//tris
    //					buffer.PushFace(faces[0]);
    //					buffer.PushFace(faces[2]);
    //					buffer.PushFace(faces[1]);
    //				}
    //				else if (linePart.Length == 5)
    //				{
    //					//quad
    //					buffer.PushFace(faces[0]);
    //					buffer.PushFace(faces[1]);
    //					buffer.PushFace(faces[3]);
    //					buffer.PushFace(faces[3]);
    //					buffer.PushFace(faces[1]);
    //					buffer.PushFace(faces[2]);
    //				}
    //				else
    //				{
    //					Debug.Log("face vertex count :" + (linePart.Length - 1) + " larger than 4. Ngons not supported.");
    //					return false; //Return failure. Not triangulated.
    //				}
    //				break;
    //		}
    //		return true;
    //	}

    //	/// <summary>
    //	/// Read the next mtl line
    //	/// </summary>
    //	/// <returns>How many lines remain to be parsed</returns>
    public int ParseNextMtlLines(int maxLines)
    {
        return -1;
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

    //	void GetFaceIndices(IList<FaceIndices> targetFacesList, string[] linePart)
    //	{
    //		string[] indices;
    //		for (int i = 1; i < linePart.Length; i++)
    //		{
    //			indices = linePart[i].Trim().Split(faceSplitChar);
    //			var faceIndices = new FaceIndices();
    //			// vertex
    //			int vertexIndex = ci(indices[0]);
    //			faceIndices.vertexIndex = vertexIndex - 1;
    //			// uv
    //			if (indices.Length > 1 && indices[1] != "")
    //			{
    //				int uvIndex = ci(indices[1]);
    //				faceIndices.vertexUV = uvIndex - 1;
    //			}
    //			// normal
    //			if (indices.Length > 2 && indices[2] != "")
    //			{
    //				int normalIndex = ci(indices[2]);
    //				faceIndices.vertexNormal = normalIndex - 1;
    //			}
    //			else
    //			{
    //				faceIndices.vertexNormal = -1;
    //			}
    //			targetFacesList[i - 1] = faceIndices;
    //		}
    //	}

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

    //	static double cd(string v)
    //	{
    //		try
    //		{
    //			return double.Parse(v, System.Globalization.CultureInfo.InvariantCulture);
    //		}
    //		catch (Exception e)
    //		{
    //			print(e + " -> " + v);
    //			return 0;
    //		}
    //	}

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

    //	class MaterialData
    //	{
    //		public string Name;
    //		public Color Ambient;
    //		public Color Diffuse;
    //		public Color Specular;
    //		public float Shininess;
    //		public float Alpha;
    //		public int IllumType;
    //		public string DiffuseTexPath;
    //		public string BumpTexPath;
    //		public Texture2D DiffuseTex;
    //		public Texture2D BumpTex;
    //	}

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

    //public void Build(Material defaultMaterial)
    //{
    //    //		//Close our stringreader
    //    //		//stringReader.Close();		

    //    //		var materialLibrary = new Dictionary<string, Material>();
    //    //		if (!string.IsNullOrEmpty(mtllib) && materialDataSlots != null)
    //    //		{
    //    //			foreach (MaterialData md in materialDataSlots)
    //    //			{
    //    //				if (materialLibrary.ContainsKey(md.Name))
    //    //				{
    //    //					Debug.LogWarning("Duplicate material found: " + md.Name + ". ignored repeated occurences");
    //    //					continue;
    //    //				}
    //    //				materialLibrary.Add(md.Name, GetMaterial(md, defaultMaterial));
    //    //			}
    //    //		}

    //    //		var gameObjects = new GameObject[buffer.NumberOfObjects];
    //    //		if (buffer.NumberOfObjects == 1 && !IgnoreObjectsOutsideOfBounds)
    //    //		{
    //    //			//Single gameobject, single mesh
    //    //			Debug.Log("Single mesh OBJ. Putting renderer on root object.");
    //    //			gameObject.AddComponent<MeshFilter>();
    //    //			gameObject.AddComponent<MeshRenderer>().enabled = EnableMeshRenderer;
    //    //			gameObjects[0] = gameObject;
    //    //		}
    //    //		else if (buffer.NumberOfObjects > 1)
    //    //		{
    //    //			for (int i = 0; i < buffer.NumberOfObjects; i++)
    //    //			{
    //    //				//Multi object with nested children
    //    //				var childGameObject = new GameObject();
    //    //				childGameObject.transform.parent = gameObject.transform;
    //    //				childGameObject.AddComponent<MeshFilter>();
    //    //				childGameObject.AddComponent<MeshRenderer>().enabled = EnableMeshRenderer;
    //    //				gameObjects[i] = childGameObject;
    //    //			}
    //    //		}

    //    //		if(tracing) buffer.Trace();
    //    //		buffer.flipTriangleDirection = flipFaceDirection;
    //    //		//buffer.PopulateMeshes(gameObjects, materialLibrary, defaultMaterial, this);

    //    //		// weld vertices if required
    //    //		if (weldVertices)
    //    //		{
    //    //			string meshname = "";
    //    //			WeldMeshVertices vertexWelder = this.gameObject.AddComponent<WeldMeshVertices>();
    //    //			foreach (var listGameObject in gameObjects)
    //    //			{
    //    //				if (!listGameObject) continue;

    //    //				var meshFilter = listGameObject.GetComponent<MeshFilter>();
    //    //				if (!meshFilter) continue;

    //    //				meshname = meshFilter.sharedMesh.name;
    //    //				Mesh newMeshWithCombinedVerts = vertexWelder.WeldVertices(listGameObject.GetComponent<MeshFilter>().sharedMesh);
    //    //				newMeshWithCombinedVerts.name = meshname;
    //    //				// destroy the old mesh;
    //    //				Destroy(listGameObject.GetComponent<MeshFilter>().sharedMesh);
    //    //				listGameObject.GetComponent<MeshFilter>().sharedMesh = newMeshWithCombinedVerts;
    //    //			}
    //    //			Destroy(vertexWelder);
    //    //		}
    //}

}
class MaterialData
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