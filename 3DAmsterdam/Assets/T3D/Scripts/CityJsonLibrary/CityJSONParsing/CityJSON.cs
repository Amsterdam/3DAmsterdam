/*
*  Copyright (C) X Gemeente
*              	 X Amsterdam
*				 X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://joinup.ec.europa.eu/software/page/eupl
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/

using System.Collections.Generic;
using System.IO;
using CityJsonLibrary.Coordinates;
using System.Numerics;
using System;
using System.Text;
using System.Globalization;
using SimpleJSON;

namespace Netherlands3D.CityJSON
{
	public class CityJSON
	{
		public JSONNode cityJsonNode;
		public Vector3Double[] vertices;
		private List<Vector2> textureVertices;
		private List<Surfacetexture> Textures;
		private List<CityJsonMaterial> Materials;

		private float LOD = -1;
		private string filterType = "";

		private Vector3Double transformScale;
		private Vector3Double transformOffset;

		public Vector3Double TransformOffset { get => transformOffset; }

		public CityJSON(string file_or_cityjson)
		{
			string filepath = string.Empty;

            if (File.Exists(file_or_cityjson))
            {
				filepath = file_or_cityjson;

				cityJsonNode = StreamParse(file_or_cityjson);

				if (cityJsonNode == null || cityJsonNode["CityObjects"] == null)
				{
					Console.WriteLine($"Failed to parse CityJSON file {file_or_cityjson}");
					return;
				}
			}
            else
            {
				cityJsonNode = JSON.Parse(file_or_cityjson);

				if (cityJsonNode == null || cityJsonNode["CityObjects"] == null)
				{
					Console.WriteLine($"Failed to parse CityJSON {file_or_cityjson}");
					return;
				}
			}
			

			//Get vertices
			Console.WriteLine("\r reading vertices");
			//vertices = new Vector3Double[];
			textureVertices = new List<Vector2>();
			Textures = new List<Surfacetexture>();

			//Optionaly parse transform scale and offset
			transformScale = (cityJsonNode["transform"] != null && cityJsonNode["transform"]["scale"] != null) ? new Vector3Double(
				cityJsonNode["transform"]["scale"][0].AsDouble,
				cityJsonNode["transform"]["scale"][1].AsDouble,
				cityJsonNode["transform"]["scale"][2].AsDouble
			) : new Vector3Double(1, 1, 1);

			transformOffset = (cityJsonNode["transform"] != null && cityJsonNode["transform"]["translate"] != null) ? new Vector3Double(
				   cityJsonNode["transform"]["translate"][0].AsDouble,
				   cityJsonNode["transform"]["translate"][1].AsDouble,
				   cityJsonNode["transform"]["translate"][2].AsDouble
			) : new Vector3Double(0, 0, 0);

			//now load all the vertices with the scaler and offset applied
			
			vertices = new Vector3Double[cityJsonNode["vertices"].Count];
			int counter = 0;
			foreach (JSONNode node in cityJsonNode["vertices"])
			{
				var vertCoordinates = new Vector3Double(
						node[0].AsDouble * transformScale.X + transformOffset.X,
						node[1].AsDouble * transformScale.Y + transformOffset.Y,
						node[2].AsDouble * transformScale.Z + transformOffset.Z
				);
				vertices[counter++] = vertCoordinates;
			}
			//get textureVertices
			foreach (JSONNode node in cityJsonNode["appearance"]["vertices-texture"])
			{
				textureVertices.Add(new Vector2(node[0].AsFloat, node[1].AsFloat));
			}
			foreach (JSONNode node in cityJsonNode["appearance"]["textures"])
			{
				Surfacetexture texture = new Surfacetexture();
				texture.path = filepath + node["image"];
				texture.wrapmode = node["wrapMode"];
				Textures.Add(texture);
			}
		}

		public int CityObjectCount()
        {
			return cityJsonNode["CityObjects"].Count;
        }

		public CityObject LoadCityObjectByID(int id, float lod)
        {
			this.LOD = lod;
			this.filterType = "";
			string key = cityJsonNode["CityObjects"].getKeyAtIndex(id);
			
			CityObject cityObject = ReadCityObject(cityJsonNode["CityObjects"][id], filterType);
			if (cityObject != null)
			{
				cityObject.keyName = key;
			}
			return cityObject;
		}

		public void ClearCityObject(string key)
        {
			cityJsonNode["CityObjects"][key] = null;
        }

		public CityObject LoadCityObjectByKey(string key)
        {
			CityObject cityObject = ReadCityObject(cityJsonNode["CityObjects"][key], filterType);
			cityObject.keyName = key;
			return cityObject;
		}

		public List<CityObject> LoadCityObjects(float lod, string filterType = "")
		{
			List<CityObject> cityObjects = new List<CityObject>();
			this.LOD = lod;
			this.filterType = filterType;
			
			//Traverse the cityobjects as a key value pair, so we can read the unique key name and use it as a default identifier
			int counter = 0;
			
			foreach (KeyValuePair<string, JSONNode> kvp in (JSONObject)cityJsonNode["CityObjects"])
			{
				Console.WriteLine("\r" + counter++);
				CityObject cityObject = ReadCityObject(kvp.Value, filterType);
				if (cityObject != null)
				{
					cityObject.keyName = kvp.Key;
					cityObjects.Add(cityObject);
				}
			}

			return cityObjects;
		}

		private CityObject ReadCityObject(JSONNode node, string filter = "")
		{
			CityObject cityObject = new CityObject();

			//Read filer object type
			var type = node["type"];
			cityObject.cityObjectType = type.Value;
			if (filter != "" && (type == null || type != filter))
			{
				return null;
			}

			//read attributes
			List<Semantics> semantics = ReadSemantics(node["attributes"]);
			cityObject.semantics = semantics;

			//read surface geometry ( if we did not filter LOD, or the LOD matches )
			List<Surface> surfaces = new List<Surface>();
			foreach (JSONNode geometrynode in node["geometry"])
			{
				if (LOD == -1 || geometrynode["lod"].AsFloat == LOD)
				{
					if (geometrynode["type"] == "Solid")
					{
						JSONNode exteriorshell = geometrynode["boundaries"][0];
						foreach (JSONNode surfacenode in exteriorshell)
						{
							surfaces.Add(ReadSurfaceVectors(surfacenode));
						}
						JSONNode interiorshell = geometrynode[0][1];
						if (interiorshell != null)
						{
							foreach (JSONNode surfacenode in interiorshell)
							{
								surfaces.Add(ReadSurfaceVectors(surfacenode));
							}
						}
					}
					if (geometrynode["type"] == "MultiSurface")
					{
						foreach (JSONNode surfacenode in geometrynode["boundaries"])
						{
							surfaces.Add(ReadSurfaceVectors(surfacenode));
						}
					}

					//read textureValues
					JSONNode texturenode = geometrynode["texture"];
					if (texturenode != null)
					{
						int counter = 0;
						JSONNode valuesNode = texturenode[0]["values"];
						if (geometrynode["type"] == "Solid")
						{
							JSONNode exteriorshell = valuesNode[0];
							foreach (JSONNode surfacenode in exteriorshell)
							{
								surfaces[counter] = AddSurfaceUVs(surfacenode, surfaces[counter]);
								counter++;
							}
							JSONNode interiorshell = valuesNode[1];
							if (interiorshell != null)
							{
								foreach (JSONNode surfacenode in interiorshell)
								{
									surfaces[counter] = AddSurfaceUVs(surfacenode, surfaces[counter]);
									counter++;
								}
							}
						}
						else if (geometrynode["type"] == "MultiSurface")
						{
							foreach (JSONNode surfacenode in valuesNode)
							{
								surfaces[counter] = AddSurfaceUVs(surfacenode, surfaces[counter]);
								counter++;
							}
						}
					}


					//read SurfaceAttributes
					JSONNode semanticsnode = geometrynode["semantics"];
					if (semanticsnode != null)
					{
						if (geometrynode["type"] == "Solid")
						{
							for (int i = 0; i < semanticsnode["values"][0].Count; i++)
							{
								int semanticsID = semanticsnode["values"][0][i].AsInt;
								surfaces[i].semantics = ReadSemantics(semanticsnode["surfaces"][semanticsID]);
							}
						}
						if (geometrynode["type"] == "MultiSurface")
						{
							for (int i = 0; i < semanticsnode["values"].Count; i++)
							{
								int semanticsID = semanticsnode["values"][i].AsInt;
								surfaces[i].semantics = ReadSemantics(semanticsnode["surfaces"][semanticsID]);
							}
						}

					}

					// read MaterialValues
					JSONNode materialnode = geometrynode["material"];
				}
			}

			cityObject.surfaces = surfaces;

			//process children
			JSONNode childrenNode = node["children"];
			if (childrenNode != null)
			{
				List<CityObject> children = new List<CityObject>();
				for (int i = 0; i < childrenNode.Count; i++)
				{
					string childname = childrenNode[i];
					JSONNode childnode = cityJsonNode["CityObjects"][childname];
					CityObject child = ReadCityObject(childnode);
					child.keyName = childname;

					if (child != null)
					{
						children.Add(child);
					}
				}
				cityObject.children = children;
			}

			return cityObject;
		}

		private List<Surface> ReadSolid(JSONNode geometrynode)
		{
			JSONNode boundariesNode = geometrynode["boundaries"];
			List<Surface> result = new List<Surface>();

			foreach (JSONNode node in boundariesNode[0])
			{
				Surface surf = new Surface();
				foreach (JSONNode vertexnode in node[0])
				{
					surf.outerRing.Add(vertices[vertexnode.AsInt]);
				}
				result.Add(surf);
			}
			JSONNode semanticsnode = geometrynode["semantics"];
			JSONNode ValuesNode = semanticsnode["values"][0];
			for (int i = 0; i < ValuesNode.Count; i++)
			{
				result[i].SurfaceType = geometrynode["semantics"]["surfaces"][ValuesNode[i].AsInt]["type"];
				result[i].semantics = ReadSemantics(geometrynode["semantics"]["surfaces"][ValuesNode[i].AsInt]);
			}

			if (geometrynode["texture"] != null)
			{
				Surfacetexture surfacematerial = null;
				int counter = 0;
				foreach (JSONNode node in geometrynode["texture"][0][0])
				{
					List<Vector2> indices = new List<Vector2>();
					for (int i = 0; i < node[0][0].Count; i++)
					{
						JSONNode item = node[0][0][i];

						if (surfacematerial is null)
						{
							surfacematerial = Textures[item.AsInt];
							result[i].surfacetexture = surfacematerial;
						}
						else
						{
							indices.Add(textureVertices[item.AsInt]);
						}

					}
					indices.Reverse();
					result[counter].outerringUVs = indices;
					counter++;
				}
			}
			return result;
		}

		private List<Surface> ReadMultiSurface(JSONNode geometrynode)
		{
			JSONNode boundariesNode = geometrynode["boundaries"];
			List<Surface> result = new List<Surface>();
			foreach (JSONNode node in boundariesNode)
			{
				Surface surf = new Surface();
				foreach (JSONNode vertexnode in node[0])
				{
					surf.outerRing.Add(vertices[vertexnode.AsInt]);

				}
				for (int i = 1; i < node.Count; i++)
				{
					List<Vector3Double> innerRing = new List<Vector3Double>();
					foreach (JSONNode vertexnode in node[i])
					{
						innerRing.Add(vertices[vertexnode.AsInt]);

					}
					surf.innerRings.Add(innerRing);
				}
				result.Add(surf);
			}
			//add semantics
			JSONNode semanticsnode = geometrynode["semantics"];
			JSONNode ValuesNode = semanticsnode["values"];
			for (int i = 0; i < ValuesNode.Count; i++)
			{
				string surfacetype = geometrynode["semantics"]["surfaces"][ValuesNode[i].AsInt]["type"];
				result[i].SurfaceType = geometrynode["semantics"]["surfaces"][ValuesNode[i].AsInt]["type"];
				result[i].semantics = ReadSemantics(geometrynode["semantics"]["surfaces"][ValuesNode[i].AsInt]);
			}

			return result;
		}

		private Surface AddSurfaceUVs(JSONNode UVValueNode, Surface surf)
		{
			List<Vector2> UVs = new List<Vector2>();

			foreach (JSONNode vectornode in UVValueNode[0])
			{
				if (surf.TextureNumber == -1)
				{
					surf.TextureNumber = vectornode.AsInt;
					surf.surfacetexture = Textures[surf.TextureNumber];
				}
				else
				{
					UVs.Add(textureVertices[vectornode.AsInt]);
				}
			}
			UVs.Reverse();
			surf.outerringUVs = UVs;

			//inner rings
			for (int i = 1; i < UVValueNode.Count; i++)
			{
				UVs = new List<Vector2>();
				int counter = 0;
				foreach (JSONNode vectornode in UVValueNode[i])
				{
					if (counter > 0)
					{
						UVs.Add(textureVertices[vectornode.AsInt]);
					}
					counter++;
				}
				UVs.Reverse();
				surf.innerringUVs.Add(UVs);
			}
			return surf;
		}
		private Surface ReadSurfaceVectors(JSONNode surfacenode)
		{
			Surface surf = new Surface();
			//read exteriorRing
			List<Vector3Double> verts = new List<Vector3Double>();
			foreach (JSONNode vectornode in surfacenode[0])
			{
				verts.Add(vertices[vectornode.AsInt]);
			}
			surf.outerRing = verts;
			for (int i = 1; i < surfacenode.Count; i++)
			{
				verts = new List<Vector3Double>();
				foreach (JSONNode vectornode in surfacenode[i])
				{
					verts.Add(vertices[vectornode.AsInt]);
				}
				surf.innerRings.Add(verts);
			}

			return surf;
		}

		private List<Semantics> ReadSemantics(JSONNode semanticsNode)
		{
			List<Semantics> result = new List<Semantics>();

			if (semanticsNode!=null)
			{
				foreach (KeyValuePair<string, JSONNode> kvp in semanticsNode)
				{
					result.Add(new Semantics(kvp.Key, kvp.Value));
				}
			}

			return result;
		}

		public JSONNode StreamParse(string fileName)
		{
			using (StreamReader streamReader = File.OpenText(fileName))
			{
				Stack<JSONNode> stack = new Stack<JSONNode>();
				JSONNode ctx = null;
				int i = 0;
				long bytelength = streamReader.BaseStream.Length;
				StringBuilder Token = new StringBuilder();
				string TokenName = "";
				bool QuoteMode = false;
				bool TokenIsQuoted = false;
				char str;
				while (!streamReader.EndOfStream)
				{
					str = (char)streamReader.Read();
					i++;
					if (i > 100000000)
					{
						i = 0;
						string percentage = ((int)(100 * streamReader.BaseStream.Position / bytelength)).ToString();
						Console.Write($"\rreading file ... { percentage}%");
					}
					switch (str)
					{
						case '{':
							if (QuoteMode)
							{
								Token.Append(str);
								break;
							}
							stack.Push(new JSONObject());
							if (ctx != null)
							{
								ctx.Add(TokenName, stack.Peek());
							}
							TokenName = "";
							Token.Length = 0;
							ctx = stack.Peek();
							break;

						case '[':
							if (QuoteMode)
							{
								Token.Append(str);
								break;
							}

							stack.Push(new JSONArray());
							if (ctx != null)
							{
								ctx.Add(TokenName, stack.Peek());
							}
							TokenName = "";
							Token.Length = 0;
							ctx = stack.Peek();
							break;

						case '}':
						case ']':
							if (QuoteMode)
							{

								Token.Append(str);
								break;
							}
							if (stack.Count == 0)
								throw new Exception("JSON Parse: Too many closing brackets");

							stack.Pop();
							if (Token.Length > 0 || TokenIsQuoted)
							{
								ParseElement(ctx, Token.ToString(), TokenName, TokenIsQuoted);
								TokenIsQuoted = false;
							}
							TokenName = "";
							Token.Length = 0;
							if (stack.Count > 0)
								ctx = stack.Peek();
							break;

						case ':':
							if (QuoteMode)
							{
								Token.Append(str);
								break;
							}
							TokenName = Token.ToString();
							Token.Length = 0;
							TokenIsQuoted = false;
							break;

						case '"':
							QuoteMode ^= true;
							TokenIsQuoted |= QuoteMode;
							break;

						case ',':
							if (QuoteMode)
							{
								Token.Append(str);
								break;
							}
							if (Token.Length > 0 || TokenIsQuoted)
							{
								ParseElement(ctx, Token.ToString(), TokenName, TokenIsQuoted);
								TokenIsQuoted = false;
							}
							TokenName = "";
							Token.Length = 0;
							TokenIsQuoted = false;
							break;

						case '\r':
						case '\n':
							break;

						case ' ':
						case '\t':
							if (QuoteMode)
								Token.Append(str);
							break;

						case '\\':
							++i;
							if (QuoteMode)
							{
								char C = str;
								switch (C)
								{
									case 't':
										Token.Append('\t');
										break;
									case 'r':
										Token.Append('\r');
										break;
									case 'n':
										Token.Append('\n');
										break;
									case 'b':
										Token.Append('\b');
										break;
									case 'f':
										Token.Append('\f');
										break;
									case 'u':
										{ //(char)streamReader.Read()
											string s = $"{(char)streamReader.Read()}{(char)streamReader.Read()}{(char)streamReader.Read()}{(char)streamReader.Read()}";//aJSON.Substring(i + 1, 4);
											Token.Append((char)int.Parse(
												s,
												System.Globalization.NumberStyles.AllowHexSpecifier));

											break;
										}
									default:
										Token.Append(C);
										break;
								}
							}
							break;

						default:
							Token.Append(str);
							break;
					}
					++i;
				}
				if (QuoteMode)
				{
					throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
				}
				return ctx;
			}
		}

		static void ParseElement(JSONNode ctx, string token, string tokenName, bool quoted)
		{
			if (quoted)
			{
				ctx.Add(tokenName, token);
				return;
			}
			string tmp = token.ToLower();
			if (tmp == "false" || tmp == "true")
				ctx.Add(tokenName, tmp == "true");
			else if (tmp == "null")
				ctx.Add(tokenName, null);
			else
			{
				double val;
				if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
					ctx.Add(tokenName, val);
				else
					ctx.Add(tokenName, token);
			}
		}
	}
	public class CityObject
	{
		public List<Semantics> semantics;
		public List<Surface> surfaces;
		public List<CityObject> children;
		public string cityObjectType;
		public string keyName = "";

		public CityObject()
		{
			semantics = new List<Semantics>();
			surfaces = new List<Surface>();
			children = new List<CityObject>();
		}
	}

	public class Surface
	{
		public List<Vector3Double> outerRing;
		public List<List<Vector3Double>> innerRings;
		public List<Vector2> outerringUVs;
		public List<List<Vector2>> innerringUVs;
		public string SurfaceType;
		public List<Semantics> semantics;
		public Surfacetexture surfacetexture;
		public int TextureNumber;

		public Surface()
		{
			TextureNumber = -1;
			semantics = new List<Semantics>();
			outerRing = new List<Vector3Double>();
			innerRings = new List<List<Vector3Double>>();
			outerringUVs = new List<Vector2>();
			innerringUVs = new List<List<Vector2>>();
		}
	}

	public class Surfacetexture
	{
		public string path;
		public string wrapmode;
	}

	[System.Serializable]
	public class Semantics
	{
		public string name;
		public string value;
		public Semantics(string Name, string Value)
		{
			name = Name;
			value = Value;
		}
	}

	public class CityJsonMaterial
	{
		public string name;
		public float r;
		public float g;
		public float b;
		public float a;
	}
}
