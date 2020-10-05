using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine.Networking;

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

	private string[] objLines;
	private string[] mtlLines;

	private string line;
	private string[] linePart;

	private Regex regexWhitespaces = new Regex(@"\s+");

	private int parseLinePointer = 0;

	private GeometryBuffer buffer;
	// Materials
	private List<MaterialData> materialData;

	// Awake so that the Buffer is always instantiated in time.
	void Awake()
	{
		buffer = new GeometryBuffer();
	}

	/// <summary>
	/// Sets the obj string and turns it into an array with every newline
	/// </summary>
	/// <param name="data">obj string</param>
	public void SetGeometryData(string data)
	{
		objLines = data.Split("\n".ToCharArray());
		parseLinePointer = 0;
	}
	/// <summary>
	/// Sets the material string and turns in into an array with every newline
	/// </summary>
	/// <param name="data">obj string</param>
	public void SetMaterialData(string data)
	{
		mtlLines = data.Split("\n".ToCharArray());
		parseLinePointer = 0;
		materialData = new List<MaterialData>();
	}

	/// <summary>
	/// Read the next obj line
	/// </summary>
	/// <returns>How many lines remain to be parsed</returns>
	public int ParseNextObjLines(int maxLines)
	{
		for (int i = 0; i < maxLines; i++)
		{
			if (parseLinePointer < objLines.Length)
			{
				line = objLines[parseLinePointer].Trim();
				linePart = regexWhitespaces.Split(line);
				switch (linePart[0])
				{
					case O:
						//buffer.AddObject(linePart[1].Trim()); We skip object seperation, to reduce object count.
						//Importing large SketchupUp generated OBJ files results in an enormous amount of objects, making WebGL builds explode. 
						break;
					case V:
						buffer.PushVertex(new Vector3(cf(linePart[1]), cf(linePart[2]), cf(linePart[3])));
						break;
					case VT:
						buffer.PushUV(new Vector2(cf(linePart[1]), cf(linePart[2])));
						break;
					case VN:
						buffer.PushNormal(new Vector3(cf(linePart[1]), cf(linePart[2]), cf(linePart[3])));
						break;
					case F:
						var faces = new FaceIndices[linePart.Length - 1];
						GetFaceIndices(faces, linePart);
						if (linePart.Length == 4)
						{
							//tris
							buffer.PushFace(faces[0]);
							buffer.PushFace(faces[1]);
							buffer.PushFace(faces[2]);
						}
						else if (linePart.Length == 5)
						{
							//quad
							buffer.PushFace(faces[0]);
							buffer.PushFace(faces[1]);
							buffer.PushFace(faces[3]);
							buffer.PushFace(faces[3]);
							buffer.PushFace(faces[1]);
							buffer.PushFace(faces[2]);
						}
						else
						{
							//ngons
							Debug.LogWarning("face vertex count :" + (linePart.Length - 1) + " larger than 4. Ngons not supported.");
						}
						break;
					case MTLLIB:
						mtllib = line.Substring(linePart[0].Length + 1).Trim();
						break;
					case USEMTL:
						buffer.AddSubMeshGroup(linePart[1].Trim());
						break;
				}
				parseLinePointer++;
			}
		}
		return objLines.Length - parseLinePointer;
	}

	/// <summary>
	/// Read the next mtl line
	/// </summary>
	/// <returns>How many lines remain to be parsed</returns>
	public int ParseNextMtlLines(int maxLines)
	{
		for (int i = 0; i < maxLines; i++)
		{
			if (parseLinePointer < mtlLines.Length)
			{
				var currentMaterialData = new MaterialData();
				line = mtlLines[parseLinePointer].Trim();

				if (line.IndexOf("#") != -1) line = line.Substring(0, line.IndexOf("#"));
				linePart = regexWhitespaces.Split(line);

				if (linePart[0].Trim() != "")
				{
					switch (linePart[0])
					{
						case NML:
							currentMaterialData = new MaterialData();
							currentMaterialData.Name = linePart[1].Trim();
							materialData.Add(currentMaterialData);
							break;
						case KA:
							currentMaterialData.Ambient = gc(linePart);
							break;
						case KD:
							currentMaterialData.Diffuse = gc(linePart);
							break;
						case KS:
							currentMaterialData.Specular = gc(linePart);
							break;
						case NS:
							currentMaterialData.Shininess = cf(linePart[1]) / 1000;
							break;
						case D:
						case TR:
							currentMaterialData.Alpha = cf(linePart[1]);
							break;
						case MAP_KD:
							currentMaterialData.DiffuseTexPath = linePart[linePart.Length - 1].Trim();
							break;
						case MAP_BUMP:
						case BUMP:
							BumpParameter(currentMaterialData, linePart);
							break;
						case ILLUM:
							currentMaterialData.IllumType = ci(linePart[1]);
							break;
						default:
							Debug.Log("this line was not processed :" + line);
							break;
					}
				}
				parseLinePointer++;
			}
		}

		return mtlLines.Length - parseLinePointer;
	}

	void GetFaceIndices(IList<FaceIndices> targetFacesList, string[] linePart)
	{
		for (int i = 1; i < linePart.Length; i++)
		{
			string[] indices = linePart[i].Trim().Split("/".ToCharArray());
			var faceIndices = new FaceIndices();
			// vertex
			int vertexIndex = ci(indices[0]);
			faceIndices.vertexIndex = vertexIndex - 1;
			// uv
			if (indices.Length > 1 && indices[1] != "")
			{
				int uvIndex = ci(indices[1]);
				faceIndices.vertexUV = uvIndex - 1;
			}
			// normal
			if (indices.Length > 2 && indices[2] != "")
			{
				int normalIndex = ci(indices[2]);
				faceIndices.vertexNormal = normalIndex - 1;
			}
			else
			{
				faceIndices.vertexNormal = -1;
			}
			targetFacesList[i - 1] = faceIndices;
		}
	}

	static Material GetMaterial(MaterialData md, Material sourceMaterial)
	{
		Material newMaterial;

		if (md.IllumType == 2)
		{
			string shaderName = (md.BumpTex != null) ? "Bumped Specular" : "Specular";
			newMaterial = new Material(sourceMaterial);
			newMaterial.SetColor("_SpecColor", md.Specular);
			newMaterial.SetFloat("_Shininess", md.Shininess);
		}
		else
		{
			string shaderName = (md.BumpTex != null) ? "Bumped Diffuse" : "Diffuse";
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
			return float.Parse(v);
		}
		catch (Exception e)
		{
			print(e);
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
			print(e);
			return 0;
		}
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

	public void Build(Material defaultMaterial)
	{
		var materialLibrary = new Dictionary<string, Material>();
		if (!string.IsNullOrEmpty(mtllib) && materialData != null)
		{
			foreach (MaterialData md in materialData)
			{
				if (materialLibrary.ContainsKey(md.Name))
				{
					Debug.LogWarning("Duplicate material found: " + md.Name + ". ignored repeated occurences");
					continue;
				}
				materialLibrary.Add(md.Name, GetMaterial(md, defaultMaterial));
			}
		}

		var gameObjects = new GameObject[buffer.NumberOfObjects];
		if (buffer.NumberOfObjects == 1)
		{
			gameObject.AddComponent(typeof(MeshFilter));
			gameObject.AddComponent(typeof(MeshRenderer));
			gameObjects[0] = gameObject;
		}
		else if (buffer.NumberOfObjects > 1)
		{
			for (int i = 0; i < buffer.NumberOfObjects; i++)
			{
				var go = new GameObject();
				go.transform.parent = gameObject.transform;
				go.AddComponent(typeof(MeshFilter));
				go.AddComponent(typeof(MeshRenderer));
				gameObjects[i] = go;
			}
		}

		buffer.PopulateMeshes(gameObjects, materialLibrary, defaultMaterial);
	}
}