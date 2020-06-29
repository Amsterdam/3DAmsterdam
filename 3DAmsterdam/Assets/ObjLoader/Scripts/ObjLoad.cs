using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine.Networking;

public class ObjLoad : MonoBehaviour
{

	public Shader Shader;

	// OBJ File Tags
	const string O = "o";
	const string G = "g";
	const string V = "v";
	const string VT = "vt";
	const string VN = "vn";
	const string F = "f";
	const string MTL = "mtllib";
	const string UML = "usemtl";

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

	string basepath;
	string mtllib;
	GeometryBuffer buffer;

	// Awake so that the Buffer is always instantiated in time.
	void Awake()
	{
		buffer = new GeometryBuffer();
		Shader = Shader.Find("Standard");
	}

	UnityWebRequest GetTextureLoader(MaterialData m, string texpath)
	{
		char[] separators = { '/', '\\' };
		string[] components = texpath.Split(separators);
		string filename = components[components.Length - 1];
		string ext = Path.GetExtension(filename).ToLower();
		if (ext != ".png" && ext != ".jpg")
		{
			Debug.LogWarning("Unsupported texture format: " + ext);
		}
		var texloader = new UnityWebRequest(basepath + filename);
		Debug.Log("Texture path for material(" + m.Name + ") = " + (basepath + filename));
		return texloader;
	}

	void GetFaceIndicesByOneFaceLine(IList<FaceIndices> faces, string[] p, bool isFaceIndexPlus)
	{
		if (isFaceIndexPlus)
		{
			for (int j = 1; j < p.Length; j++)
			{
				string[] c = p[j].Trim().Split("/".ToCharArray());
				var fi = new FaceIndices();
				// vertex
				int vi = ci(c[0]);
				fi.Vi = vi - 1;
				// uv
				if (c.Length > 1 && c[1] != "")
				{
					int vu = ci(c[1]);
					fi.Vu = vu - 1;
				}
				// normal
				if (c.Length > 2 && c[2] != "")
				{
					int vn = ci(c[2]);
					fi.Vn = vn - 1;
				}
				else
				{
					fi.Vn = -1;
				}
				faces[j - 1] = fi;
			}
		}
		else
		{ // for minus index
			int vertexCount = buffer.Vertices.Count;
			int uvCount = buffer.Uvs.Count;
			for (int j = 1; j < p.Length; j++)
			{
				string[] c = p[j].Trim().Split("/".ToCharArray());
				var fi = new FaceIndices();
				// vertex
				int vi = ci(c[0]);
				fi.Vi = vertexCount + vi;
				// uv
				if (c.Length > 1 && c[1] != "")
				{
					int vu = ci(c[1]);
					fi.Vu = uvCount + vu;
				}
				// normal
				if (c.Length > 2 && c[2] != "")
				{
					int vn = ci(c[2]);
					fi.Vn = vertexCount + vn;
				}
				else
				{
					fi.Vn = -1;
				}
				faces[j - 1] = fi;
			}
		}
	}

	public void SetGeometryData(string data)
	{
		string[] lines = data.Split("\n".ToCharArray());
		var regexWhitespaces = new Regex(@"\s+");
		bool isFirstInGroup = true;
		bool isFaceIndexPlus = true;
		for (int i = 0; i < lines.Length; i++)
		{
			string l = lines[i].Trim();

			if (l.IndexOf("#") != -1)
			{ // comment line
				continue;
			}
			string[] p = regexWhitespaces.Split(l);
			switch (p[0])
			{
				case O:
					buffer.PushObject(p[1].Trim());
					isFirstInGroup = true;
					break;
				case G:
					string groupName = null;
					if (p.Length >= 2)
					{
						groupName = p[1].Trim();
					}
					isFirstInGroup = true;
					buffer.PushGroup(groupName);
					break;
				case V:
					buffer.PushVertex(new Vector3(cf(p[1]), cf(p[2]), cf(p[3])));
					break;
				case VT:
					buffer.PushUV(new Vector2(cf(p[1]), cf(p[2])));
					break;
				case VN:
					buffer.PushNormal(new Vector3(cf(p[1]), cf(p[2]), cf(p[3])));
					break;
				case F:
					var faces = new FaceIndices[p.Length - 1];
					if (isFirstInGroup)
					{
						isFirstInGroup = false;
						string[] c = p[1].Trim().Split("/".ToCharArray());
						isFaceIndexPlus = (ci(c[0]) >= 0);
					}
					GetFaceIndicesByOneFaceLine(faces, p, isFaceIndexPlus);
					if (p.Length == 4)
					{
						buffer.PushFace(faces[0]);
						buffer.PushFace(faces[1]);
						buffer.PushFace(faces[2]);
					}
					else if (p.Length == 5)
					{
						buffer.PushFace(faces[0]);
						buffer.PushFace(faces[1]);
						buffer.PushFace(faces[3]);
						buffer.PushFace(faces[3]);
						buffer.PushFace(faces[1]);
						buffer.PushFace(faces[2]);
					}
					else
					{
						Debug.LogWarning("face vertex count :" + (p.Length - 1) + " larger than 4:");
					}
					break;
				case MTL:
					mtllib = l.Substring(p[0].Length + 1).Trim();
					break;
				case UML:
					buffer.PushMaterialName(p[1].Trim());
					break;
			}
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

	// Materials
	List<MaterialData> materialData;
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

	public void SetMaterialData(string data)
	{
		string[] lines = data.Split("\n".ToCharArray());

		materialData = new List<MaterialData>();
		var currentMaterialData = new MaterialData();
		var regexWhitespaces = new Regex(@"\s+");

		for (int i = 0; i < lines.Length; i++)
		{
			string l = lines[i].Trim();

			if (l.IndexOf("#") != -1) l = l.Substring(0, l.IndexOf("#"));
			string[] p = regexWhitespaces.Split(l);
			if (p[0].Trim() == "") continue;

			switch (p[0])
			{
				case NML:
					currentMaterialData = new MaterialData();
					currentMaterialData.Name = p[1].Trim();
					materialData.Add(currentMaterialData);
					break;
				case KA:
					currentMaterialData.Ambient = gc(p);
					break;
				case KD:
					currentMaterialData.Diffuse = gc(p);
					break;
				case KS:
					currentMaterialData.Specular = gc(p);
					break;
				case NS:
					currentMaterialData.Shininess = cf(p[1]) / 1000;
					break;
				case D:
				case TR:
					currentMaterialData.Alpha = cf(p[1]);
					break;
				case MAP_KD:
					currentMaterialData.DiffuseTexPath = p[p.Length - 1].Trim();
					break;
				case MAP_BUMP:
				case BUMP:
					BumpParameter(currentMaterialData, p);
					break;
				case ILLUM:
					currentMaterialData.IllumType = ci(p[1]);
					break;
				default:
					Debug.Log("this line was not processed :" + l);
					break;
			}
		}
	}

	static Material GetMaterial(MaterialData md)
	{
		Material m;

		if (md.IllumType == 2)
		{
			string shaderName = (md.BumpTex != null) ? "Bumped Specular" : "Specular";
			m = new Material(Shader.Find(shaderName));
			m.SetColor("_SpecColor", md.Specular);
			m.SetFloat("_Shininess", md.Shininess);
		}
		else
		{
			string shaderName = (md.BumpTex != null) ? "Bumped Diffuse" : "Diffuse";
			m = new Material(Shader.Find(shaderName));
		}

		if (md.DiffuseTex != null)
		{
			m.SetTexture("_MainTex", md.DiffuseTex);
		}
		else
		{
			m.SetColor("_Color", md.Diffuse);
		}
		if (md.BumpTex != null) m.SetTexture("_BumpMap", md.BumpTex);

		m.name = md.Name;

		return m;
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
		var materials = new Dictionary<string, Material>();
		if (!string.IsNullOrEmpty(mtllib))
		{
			foreach (MaterialData md in materialData)
			{
				if (materials.ContainsKey(md.Name))
				{
					Debug.LogWarning("Duplicate material found: " + md.Name + ". ignored repeated occurences");
					continue;
				}
				materials.Add(md.Name, GetMaterial(md));
			}
		}
		else
		{
			materials.Add("default", defaultMaterial);
		}

		var ms = new GameObject[buffer.NumberOfObjects];

		if (buffer.NumberOfObjects == 1)
		{
			gameObject.AddComponent(typeof(MeshFilter));
			gameObject.AddComponent(typeof(MeshRenderer));
			ms[0] = gameObject;
		}
		else if (buffer.NumberOfObjects > 1)
		{
			for (int i = 0; i < buffer.NumberOfObjects; i++)
			{
				var go = new GameObject();
				go.transform.parent = gameObject.transform;
				go.AddComponent(typeof(MeshFilter));
				go.AddComponent(typeof(MeshRenderer));
				ms[i] = go;
			}
		}
		buffer.PopulateMeshes(ms, materials);
	}
}