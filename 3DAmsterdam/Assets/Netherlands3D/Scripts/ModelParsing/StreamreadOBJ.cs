using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using System.IO;
using System.Text;
using UnityEngine.Rendering;
namespace Netherlands3D.ModelParsing
{
	public class StreamreadOBJ : MonoBehaviour
	{
		public int maxLinesPerFrame = 500;

		// OBJ File Tags
		const char COMMENT = '#';
		const char O = 'o';
		const char SPACE = ' ';

		const string Freeform = "vp";
		const string V = "v";
		const string VT = "vt";
		const string VN = "vn";
		const string F = "f";
		const string MTLLIB = "mtllib";
		const string USEMTL = "usemtl";

		enum objTag
		{
			Object,
			Vertex,
			VertexTexture,
			VertexNormal,
			Face,
			MtlLib,
			UseMTL,
			Other,
			LineEnd
		}

		private const char faceSplitChar = '/';
		private const char lineSplitChar = '\r';
		private const char linePartSplitChar = ' ';

		[SerializeField]
		private GeometryBuffer buffer;

		private bool splitNestedObjects = false;
		private bool ignoreObjectsOutsideOfBounds = false;
		private Vector2RD bottomLeftBounds;
		private Vector2RD topRightBounds;
		private bool RDCoordinates = false;
		private bool flipFaceDirection = false;
		private bool flipYZ = false;
		private bool weldVertices = false;
		private int maxSubMeshes = 0;

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



		StreamReader streamReader;
		public bool isFinished = false;
		public bool succes = true;

		private List<GeometryBuffer.FaceIndices> faces = new List<GeometryBuffer.FaceIndices>();

		private List<Vector3> vertices = new List<Vector3>();
		private List<Vector3> normals = new List<Vector3>();
		private List<int> indices = new List<int>();
		private Dictionary<string, Submesh> submeshes = new Dictionary<string, Submesh>();
		private Submesh activeSubmesh = new Submesh();
		void AddSubMesh(string submeshName)
		{
			if (activeSubmesh.name == submeshName)
			{
				return;
			}
			if (activeSubmesh.name != null)
			{
				if (activeSubmesh.vertices.Count > 0)
				{
					if (submeshes.ContainsKey(activeSubmesh.name))
					{
						submeshes[activeSubmesh.name] = activeSubmesh;

					}
				}
				else
                {
                    if (submeshes.ContainsKey(activeSubmesh.name))
                    {
						submeshes.Remove(activeSubmesh.name);
                    }
                }

			}
			if (submeshes.ContainsKey(submeshName))
			{
				activeSubmesh = submeshes[submeshName];

			}
			else
			{
				activeSubmesh = new Submesh();
				activeSubmesh.name = submeshName;
				activeSubmesh.startIndex = indices.Count;
				activeSubmesh.vertices = new Dictionary<int, Vertex>();
				activeSubmesh.indices = new List<int>();
				submeshes.Add(submeshName, activeSubmesh);
			}
		}



		public void ReadOBJ(string filename)
		{
			faces.Capacity = 4;



			if (!File.Exists(Application.persistentDataPath + "/" + filename))
			{
				Debug.Log("can't find file");
				succes = false;
				isFinished = true;
				return;
			}

			StartCoroutine(StreamReadFile(filename));
		}

		IEnumerator StreamReadFile(string filename)
		{
			//setup first submesh;
			AddSubMesh("default");
			int lineCount = 0;

			int characterCount = 0;
			bool lineRead = true;
			FileStream fileStream = new FileStream(Application.persistentDataPath + "/" + filename, FileMode.Open, FileAccess.Read);

			streamReader = new StreamReader(fileStream, System.Text.Encoding.UTF8);

			while (lineRead)
			{

				if (lineCount == maxLinesPerFrame)
				{
					lineCount = 0;
					yield return null;
				}
				if (streamReader.Peek() == -1)
				{
					lineRead = false;
					continue;
				}
				lineCount++;
				ReadLine();


			}

			streamReader.Close();
			fileStream.Close();

			File.Delete(Application.persistentDataPath + "/" + filename);

			isFinished = true;
			
			Debug.Log(lineCount);
		}
		private void ReadLine()
		{
			objTag tag = FindOBJTag();
			switch (tag)
			{
				case objTag.Object:
					//buffer.AddObject(ReadString());
					break;
				case objTag.Vertex:
					ReadVertex();
					break;
				case objTag.VertexTexture:
					//ReadVertexTexture();
					break;
				case objTag.VertexNormal:
					ReadNormal();
					break;
				case objTag.Face:
					ReadFaceLine();
					break;
				case objTag.MtlLib:
					ReadString();
					break;
				case objTag.UseMTL:
					AddSubMesh(ReadString());
					break;
				case objTag.Other:
					ReadString();
					break;
				default:
					break;
			}
		}

		objTag FindOBJTag()
		{
			char readChar;

			while (true)
			{
				if (NextChar(out readChar))
				{
					readChar = char.ToLower(readChar);
					if (readChar == COMMENT)
					{// found a commentLine
						return objTag.Other;
					}
					else if (readChar == SPACE)
					{// start with a space, just keep reading

					}
					else if (readChar == '\n')
					{// start with a line-end, continue on next line;

					}
					else if (readChar == '\r')
					{// start with a line-end, continue on next line;

					}
					else if (readChar == 'v')
					{// could be v, vt or vn. 
						if (NextChar(out readChar))
						{
							readChar = char.ToLower(readChar);
							if (readChar == ' ')
							{// found v
								return objTag.Vertex;
							}
							else if (readChar == 'n')
							{   // probably found vn. check if it is followed by a space
								if (NextChar(out readChar))
								{
									readChar = char.ToLower(readChar);
									if (readChar == ' ')
									{ // did find vn
										return objTag.VertexNormal;
									}
									else
									{// found something else after all
										return objTag.Other;
									}
								}
							}
							else if (readChar == 't')
							{// probably found vt. check if it is followed bij a space
								if (NextChar(out readChar))
								{
									readChar = char.ToLower(readChar);
									if (readChar == ' ')
									{ // did find vt
										return objTag.VertexTexture;
									}
									else
									{// found something else after all
										return objTag.Other;
									}
								}

							}
							else
							{ // found someting else
								return objTag.Other;
							}
						}
						else { return objTag.Other; }
					}
					else if (readChar == 'o')
					{// possibly found object, check if it is followed by a space
						if (NextChar(out readChar))
						{
							if (readChar == ' ')
							{// found object
								return objTag.Object;
							}
							else
							{// found someting else
								return objTag.Other;
							}
						}
						else { return objTag.Other; }
					}
					else if (readChar == 'f')
					{//possibly found a face, check if it is followed by a space
						if (NextChar(out readChar))
						{
							if (readChar == ' ')
							{// found a face
								return objTag.Face;
							}
							else
							{// it was something else
								return objTag.Other;
							}
						}
						else { return objTag.Other; };
					}
					else if (readChar == 'u')
					{// could have found usemtl
						if (NextChar(out readChar))
						{
							readChar = char.ToLower(readChar);
							if (readChar == 's')
							{
								readChar = char.ToLower(readChar);
								if (NextChar(out readChar))
								{
									readChar = char.ToLower(readChar);
									if (readChar == 'e')
									{
										if (NextChar(out readChar))
										{
											readChar = char.ToLower(readChar);
											if (readChar == 'm')
											{
												if (NextChar(out readChar))
												{
													readChar = char.ToLower(readChar);
													if (readChar == 't')
													{
														if (NextChar(out readChar))
														{
															readChar = char.ToLower(readChar);
															if (readChar == 'l')
															{
																if (NextChar(out readChar))
																{
																	readChar = char.ToLower(readChar);
																	if (readChar == ' ')
																	{// found usemtl
																		return objTag.UseMTL;
																	}
																	else { return objTag.Other; }
																}
																else { return objTag.Other; };
															}
															else { return objTag.Other; }
														}
														else { return objTag.Other; };
													}
													else { return objTag.Other; }
												}
												else { return objTag.Other; };
											}
											else { return objTag.Other; }
										}
										else { return objTag.Other; };
									}
									else { return objTag.Other; }
								}
								else { return objTag.Other; };
							}
							else { return objTag.Other; }
						}
						else { return objTag.Other; };
					}
					else if (readChar == 'm')
					{
						if (NextChar(out readChar))
						{
							readChar = char.ToLower(readChar);
							if (readChar == 't')
							{
								if (NextChar(out readChar))
								{
									readChar = char.ToLower(readChar);
									if (readChar == 'l')
									{
										if (NextChar(out readChar))
										{
											readChar = char.ToLower(readChar);
											if (readChar == 'l')
											{
												if (NextChar(out readChar))
												{
													readChar = char.ToLower(readChar);
													if (readChar == 'i')
													{
														if (NextChar(out readChar))
														{
															readChar = char.ToLower(readChar);
															if (readChar == 'b')
															{
																if (NextChar(out readChar))
																{
																	if (readChar == ' ')
																	{ // found mtllib
																		return objTag.MtlLib;
																	}
																	else { return objTag.Other; }
																}
																else { return objTag.Other; };
															}
															else { return objTag.Other; }
														}
														else { return objTag.Other; };
													}
													else { return objTag.Other; }
												}
												else { return objTag.Other; };
											}
											else { return objTag.Other; }
										}
										else { return objTag.Other; };
									}
									else { return objTag.Other; }
								}
								else { return objTag.Other; };
							}
							else { return objTag.Other; }
						}
						else { return objTag.Other; };
					}
					else
					{// found something else
						return objTag.Other;
					}

				}
				else
				{
					// reached the end of the file
					return objTag.Other;
				}


			}
			return objTag.Other;
		}

		string ReadString()
		{
			char readChar;
			StringBuilder sb = new StringBuilder();
			while (true)
			{
				if (NextChar(out readChar))
				{
					if (readChar == lineSplitChar)
					{
						return sb.ToString();
					}
					else
					{
						sb.Append(readChar);
					}
				}
				else { return ""; }
			}
		}

		void ReadVertex()
		{

			float x = ReadFloat();
			float y = ReadFloat();
			float z = ReadFloat();

			if (x != float.NaN && y != float.NaN & z != float.NaN)
			{
				if (FlipYZ)
				{
					vertices.Add(new Vector3(x, z, y));
				}
				else
				{
					vertices.Add(new Vector3(x, y, -z));
				}
			}
		}
		void ReadVertexTexture()
		{

			float x = ReadFloat();
			float y = ReadFloat();
			if (x != float.NaN && y != float.NaN)
			{
				buffer.PushUV(new Vector2(x, y));
			}
		}

		void ReadFaceLine()
		{
			faces.Clear();
			bool keepGoing = true;
			char lastChar;
			GeometryBuffer.FaceIndices faceIndex;
			while (keepGoing)
			{
				GeometryBuffer.FaceIndices faceindex = new GeometryBuffer.FaceIndices();
				if (ReadSingleFace(out faceindex, out lastChar))
				{//succesfully read a face
					faces.Add(faceindex);
					if (lastChar == lineSplitChar)
					{ // reached the end of the line
						keepGoing = false;
					}
				}
				else
				{
					keepGoing = false;
				}

			}
			/// process faces
			if (faces.Count == 3)
			{
				Vector3 triangleNormal = CalculateNormal(vertices[faces[0].vertexIndex], vertices[faces[1].vertexIndex], vertices[faces[2].vertexIndex]);
				SaveVertexToSubmesh(faces[0], triangleNormal);
				SaveVertexToSubmesh(faces[1], triangleNormal);
				SaveVertexToSubmesh(faces[2], triangleNormal);
				

				
			}
			else if (faces.Count == 4)
			{
				Vector3 triangleNormal = CalculateNormal(vertices[faces[0].vertexIndex], vertices[faces[1].vertexIndex], vertices[faces[3].vertexIndex]);
				SaveVertexToSubmesh(faces[0], triangleNormal);
				SaveVertexToSubmesh(faces[1], triangleNormal);
				SaveVertexToSubmesh(faces[3], triangleNormal);
				SaveVertexToSubmesh(faces[3], triangleNormal);
				SaveVertexToSubmesh(faces[1], triangleNormal);
				SaveVertexToSubmesh(faces[2], triangleNormal);
			}
			else
			{
				Debug.Log(faces.Count + " vertices in a face");
			}
		}
		void SaveVertexToSubmesh(GeometryBuffer.FaceIndices v1, Vector3 fallbackNormal)
        {

			Vertex vert1 = new Vertex();
			Vector3 normal = new Vector3();
			vert1.coordinate = vertices[v1.vertexIndex];
            if (v1.vertexNormal == -10)
            {
				normal = fallbackNormal;
            }
			else
            {
				normal = normals[v1.vertexNormal];
            }

			Vertex vertex;
            if (activeSubmesh.vertices.ContainsKey(v1.vertexIndex))
            {
				vertex = activeSubmesh.vertices[v1.vertexIndex];
            }
			else
            {
				vertex = new Vertex();
				vertex.coordinate = vertices[v1.vertexIndex];
				vertex.normals = new Dictionary<Vector3, int>();
				activeSubmesh.vertices.Add(v1.vertexIndex, vertex);
            }

			
			int newIndex;
            if (vertex.normals.ContainsKey(normal))
            {
				newIndex = vertex.normals[normal];

            }
			else
            {
				activeSubmesh.vertexCount++;
				newIndex = activeSubmesh.indexCount+1;
				activeSubmesh.indexCount++;
				vertex.normals.Add(normal, newIndex);
            }

			activeSubmesh.indices.Add(newIndex);
			activeSubmesh.vertices[v1.vertexIndex] = vertex;
			
			
           

		}

		bool ReadSingleFace(out GeometryBuffer.FaceIndices faceIndex, out char readChar)
		{
			GeometryBuffer.FaceIndices face = new GeometryBuffer.FaceIndices();
			face.vertexNormal = -10;
			int number;
			char lastChar;

			if (readInt(out number, out lastChar))
			{
                if (number<0)
                {
					number += vertices.Count;
                }
				face.vertexIndex = number - 1; //subtract 1 because objindexes start with 1
				if (lastChar == '/') // vertex is followed by texture and / or normal;
				{

					if (readInt(out number, out lastChar))

					{// succesfully read vertexUV
						face.vertexUV = number - 1;
					}
					if (readInt(out number, out lastChar))
                        if (number<0)
                        {
							number += normals.Count;
                        }
					{// succesfully read Normal
						face.vertexNormal = number - 1;
					}

				}
			}
			else
			{// couldn't read a valid integer
				faceIndex = face;
				readChar = lastChar;
				return false;
			}
			faceIndex = face;
			readChar = lastChar;
			return true;
		}

		bool readInt(out int number, out char lastChar)
		{// return if succesfully found a bool
			char readChar = ' ';
			bool numberFound = false;
			number = 0;
			int sign = 1;
			bool keepGoing = true;
			while (keepGoing)
			{
				if (NextChar(out readChar))
				{
					if (char.IsDigit(readChar))
					{
						numberFound = true;
						number = (number * 10) + (int)char.GetNumericValue(readChar);
					}
					else if (readChar == '-')
					{
						sign = -1;
					}
					else
					{
						keepGoing = false;

					}
				}
				else
				{
					keepGoing = false;
				}
			}
			number = number * sign;
			lastChar = readChar;
			return numberFound;
		}
		void ReadNormal()
		{

			float x = ReadFloat();
			float y = ReadFloat();
			float z = ReadFloat();
			if (x != float.NaN && y != float.NaN & z != float.NaN)
			{
				if (FlipYZ)
				{
					normals.Add(new Vector3(x, z, y));
				}
				else
				{
					normals.Add(new Vector3(x, y, -z));
				}
			}
		}

		float ReadFloat()
		{
			char readChar;
			bool numberFound = false;
			int number = 0;
			int decimalPlaces = 0;
			bool isDecimal = false;
			int sign = 1;
			bool keepGoing = true;
			while (keepGoing)
			{
				if (NextChar(out readChar))
				{
					switch (readChar)
					{
						case ' ':
							//found a space
							if (number > 0)
							{ // if we start with a space we continue
								keepGoing = false;
							}
							else
							{// if we end with a space we are finished
								keepGoing = false;
							}
							break;
						case lineSplitChar:
							// end of the line, end of the floatvalue
							keepGoing = false;
							break;
						case '.':
							// found a decimalpoint
							isDecimal = true;
							break;
						case '-':
							// found a negative-sign
							sign = -1;
							break;
						default:
							// no space or endof the line
							if (char.IsDigit(readChar))
							{
								numberFound = true;
								number = (number * 10) + (int)char.GetNumericValue(readChar);
								if (isDecimal)
								{
									decimalPlaces++;
								}
							}
							else
							{// found something else, so we stop
								keepGoing = false;
							}
							break;
					}
				}
				else { keepGoing = false; }
			}
			if (numberFound)
			{
				float value = sign * number / (Mathf.Pow(10, decimalPlaces));
				return value;
			}
			else
			{ // no number found, so we return NAN;
				return float.NaN;
			}

		}


		private bool NextChar(out char character)
		{
			if (streamReader.Peek() > -1)
			{
				character = (char)streamReader.Read();
				return true;
			}
			character = 'e';
			return false;
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

		public GameObject Build(Material defaultMaterial)
		{

			// remove de indices en vertices-lists because wo no longer need them
			vertices = new List<Vector3>();
			indices = new List<int>();

			//finish up the last submeshdata
			submeshes[activeSubmesh.name] = activeSubmesh;


            // set up materialList
            Material[] materials = new Material[submeshes.Count];
			
            Mesh mesh = new Mesh();
            // analyze all the vertices
            int vertexcount = 0;
            int maxVerticesPerSubmesh = 0;
            foreach (var submesh in submeshes)
            {
                int meshvertices = submesh.Value.vertexCount;
                vertexcount += meshvertices;
                if (maxVerticesPerSubmesh < meshvertices)
                {
                    maxVerticesPerSubmesh = meshvertices;
                }
            }
            // collect all the vertices and indices
            List<Vector3> defvertices = new List<Vector3>();
			List<Vector3> defNormals = new List<Vector3>();
            defvertices.Capacity = vertexcount;
			defNormals.Capacity = vertexcount;
            List<int> defIndices = new List<int>();
            defIndices.Capacity = defvertices.Count;

			
            foreach (var kvp in submeshes)
            {
				Submesh submesh = kvp.Value;
				submesh.startVertex = defvertices.Count;
                foreach (var vertex in submesh.vertices)
                {
                    foreach (var normal in vertex.Value.normals)
                    {
						defvertices.Add(vertex.Value.coordinate);
						defNormals.Add(normal.Key);
                    }
                }
                for (int i = 0; i < submesh.indices.Count; i++)
                {
					defIndices.Add(submesh.indices[i]);
                }
                submesh.vertices.Clear();

            }
            // set all the vertices
            mesh.SetVertices(defvertices);
            defvertices = new List<Vector3>(); ;
            IndexFormat indexFormat = IndexFormat.UInt32;
            //if (maxVerticesPerSubmesh < 65535)
            //{
            //    indexFormat = IndexFormat.UInt16;
            //}
            // set all the indices;
            mesh.SetIndexBufferParams(defIndices.Count, indexFormat);
            mesh.SetIndexBufferData(defIndices, 0, 0, defIndices.Count);
            defIndices = new List<int>();
            // set up the submeshes;
            mesh.subMeshCount = submeshes.Count;

			int startvertex = 0;
            int indexcounter = 0;
            int submeshIndex = 0;
            foreach (var submesh in submeshes)
            {
                SubMeshDescriptor smd = new SubMeshDescriptor();
                smd.indexStart = indexcounter;
                smd.indexCount = submesh.Value.indices.Count;
                smd.topology = MeshTopology.Triangles;
                smd.baseVertex = startvertex;
				startvertex += submesh.Value.vertexCount;
				smd.vertexCount = submesh.Value.vertexCount;
                mesh.SetSubMesh(submeshIndex, smd);

                materials[submeshIndex] = new Material(defaultMaterial);
                materials[submeshIndex].name = submesh.Key;

                submeshIndex++;
                indexcounter += submesh.Value.indices.Count;
                submesh.Value.indices.Clear();
            }
            submeshes.Clear();


			//set up all the materials;
			mesh.RecalculateNormals();
            GameObject go = new GameObject();
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.materials = materials;
            mf.mesh = mesh;

            return go;


		}

		private Vector3 CalculateNormal(Vector3 v1,Vector3 v2, Vector3 v3)
        {
			var dir = Vector3.Cross(v2 - v1, v3 - v1);
			var norm = Vector3.Normalize(dir);
			return norm;
		}
	}
	

	
}