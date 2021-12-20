using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;
using System.IO;
using System.Text;
using UnityEngine.Rendering;
using Netherlands3D.Interface;

namespace Netherlands3D.ModelParsing
{
	public class StreamreadOBJ : MonoBehaviour
	{
		public LoadingScreen loadingObjScreen;
		public int maxLinesPerFrame = 25000;
		public GameObject createdGameObject;
		public List<ReadMTL.MaterialData> materialDataSlots;
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
		[SerializeField]
		//private GeometryBuffer buffer;

		private bool splitNestedObjects = false;
		private bool ignoreObjectsOutsideOfBounds = false;
		private Vector2RD bottomLeftBounds;
		private Vector2RD topRightBounds;
		private bool RDCoordinates = false;
		private bool flipFaceDirection = false; // set to true if windingOrder in obj-file is not the standard CounterClockWise
		private bool flipYZ = false; // set to true if Y and Z axes in the obj have been flipped
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
		//public GeometryBuffer Buffer { get => buffer; set => buffer = value; }

		StreamReader streamReader;
		public bool isFinished = false;
		public bool succes = true;

		private List<GeometryBuffer.FaceIndices> faces = new List<GeometryBuffer.FaceIndices>();

		private Vector3List vertices = new Vector3List();
		private Vector3List normals = new Vector3List();
		private List<int> indices = new List<int>();
		private Dictionary<string, Submesh> submeshes = new Dictionary<string, Submesh>();
		private Submesh activeSubmesh = new Submesh();
		private bool hasNormals = true;

		void AddSubMesh(string submeshName)
		{
			if (activeSubmesh.name == submeshName)
			{
				return;
			}
			
			if (activeSubmesh.name != null)
			{
				activeSubmesh.rawData.EndWriting();
				if (activeSubmesh.vertexCount > 0)
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
				activeSubmesh.rawData.SetupWriting(submeshName);

			}
			else
			{
				activeSubmesh = new Submesh();
				activeSubmesh.rawData = new SubMeshRawData();
				activeSubmesh.rawData.SetupWriting(submeshName);
				activeSubmesh.name = submeshName;
				activeSubmesh.startIndex = indices.Count;
				//activeSubmesh.vertices = new Dictionary<Vector3, Vertex>(10000);
				//activeSubmesh.indices = new List<int>(10000);
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
			vertices.SetupWriting("vertices");
			normals.SetupWriting("normals");
			//setup first submesh;
			AddSubMesh("default");
			int lineCount = 0;

			int characterCount = 0;
			bool lineRead = true;
			int totalLinesCount = 0;
			FileStream fileStream = new FileStream(Application.persistentDataPath + "/" + filename, FileMode.Open, FileAccess.Read);

			streamReader = new StreamReader(fileStream, System.Text.Encoding.UTF8);

			while (lineRead)
			{

				if (lineCount == maxLinesPerFrame)
				{
					lineCount = 0;
					loadingObjScreen.ProgressBar.SetMessage(totalLinesCount + " regels ingelezen");
					yield return null;
				}
				if (streamReader.Peek() == -1)
				{
					lineRead = false;
					continue;
				}
				lineCount++;
				totalLinesCount++;
				ReadLine();
			}

			streamReader.Close();
			fileStream.Close();
			vertices.EndWriting();
			normals.EndWriting();
			File.Delete(Application.persistentDataPath + "/" + filename);

			isFinished = true;
			
			Debug.Log(totalLinesCount);
		}
		private void ReadLine()
		{
			objTag tag = FindOBJTag();
			switch (tag)
			{
				case objTag.Object:
					ReadString();
					//buffer.AddObject(ReadString());
					break;
				case objTag.Vertex:
					ReadVertex();
					break;
				case objTag.VertexTexture:
					ReadString();
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
					if (readChar == '\r')
					{
						return sb.ToString();
					}
                    else if (readChar == '\n')
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
                if (vertices.Count()==0)
                {// this is the first vertex, check if it is in rd-coordinates
					CheckForRD(x, y, z);
                }
                if (ObjectUsesRDCoordinates)
                {
					Vector3 coord;
					if (flipYZ)
					{
					coord = CoordConvert.RDtoUnity(new Vector3(x,z,y));
					}
					else
                    {
						coord = CoordConvert.RDtoUnity(new Vector3(x, y, z));
					}
					vertices.Add(coord.x, coord.y, coord.z);
					return;
                }

				if (FlipYZ)
				{
					vertices.Add(x, z, y);
					
				}
				else
				{
					vertices.Add(x, y, -z);
				}
			}
		}
		void CheckForRD(float x, float y, float z)
        {
            if (CoordConvert.RDIsValid(new Vector3RD(x, z, y)))
            {
				ObjectUsesRDCoordinates = true;
				FlipYZ = true;
			}
            else if (CoordConvert.RDIsValid(new Vector3RD(x, y, z)))
            {
				ObjectUsesRDCoordinates = true;
				FlipYZ = false;
			}
            
        }

		void ReadVertexTexture()
		{
			float x = ReadFloat();
			float y = ReadFloat();
			if (x != float.NaN && y != float.NaN)
			{
				//buffer.PushUV(new Vector2(x, y));
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
					if (lastChar == '\r')
					{ // reached the end of the line
						keepGoing = false;
					}
                    if (lastChar == '\n')
					{// reached the end of the line
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
                //Vector3 triangleNormal = CalculateNormal(vertices[faces[0].vertexIndex], vertices[faces[1].vertexIndex], vertices[faces[2].vertexIndex]);
                if (!FlipFaceDirection)
                {
					SaveVertexToSubmesh(faces[0]);
					SaveVertexToSubmesh(faces[2]);
					SaveVertexToSubmesh(faces[1]);
				}
                else
                {
				SaveVertexToSubmesh(faces[0]);
				SaveVertexToSubmesh(faces[1]);
				SaveVertexToSubmesh(faces[2]);
                }
				
				

				
			}
			else if (faces.Count == 4)
			{
                //Vector3 triangleNormal = CalculateNormal(vertices[faces[0].vertexIndex], vertices[faces[1].vertexIndex], vertices[faces[3].vertexIndex]);
                if (!FlipFaceDirection)
                {
					SaveVertexToSubmesh(faces[0]);
					SaveVertexToSubmesh(faces[1]);
					SaveVertexToSubmesh(faces[3]);
					SaveVertexToSubmesh(faces[3]);
					SaveVertexToSubmesh(faces[1]);
					SaveVertexToSubmesh(faces[2]);
				}
                else
                {
					SaveVertexToSubmesh(faces[0]);
					SaveVertexToSubmesh(faces[1]);
					SaveVertexToSubmesh(faces[2]);
					SaveVertexToSubmesh(faces[0]);
					SaveVertexToSubmesh(faces[2]);
					SaveVertexToSubmesh(faces[3]);
				}
				
			}
			else
			{
				Debug.Log(faces.Count + " vertices in a face");
			}
		}
		void SaveVertexToSubmesh(GeometryBuffer.FaceIndices v1)
        {
			activeSubmesh.rawData.Add(v1.vertexIndex, v1.vertexNormal, v1.vertexUV);
			activeSubmesh.vertexCount++;
			return;
		}

		bool ReadSingleFace(out GeometryBuffer.FaceIndices faceIndex, out char readChar)
		{
			GeometryBuffer.FaceIndices face = new GeometryBuffer.FaceIndices();
			face.vertexNormal = -1;
			int number;
			char lastChar;

			if (ReadInt(out number, out lastChar))
			{
                if (number<0)
                {
					number += vertices.Count();
                }
				face.vertexIndex = number - 1; //subtract 1 because objindexes start with 1
				if (lastChar == '/') // vertex is followed by texture and / or normal;
				{

					if (ReadInt(out number, out lastChar))

					{// succesfully read vertexUV
						face.vertexUV = number - 1;
					}
					if (ReadInt(out number, out lastChar))
					{ 
                        if (number<0)
                        {
							number += normals.Count();
                        }
					// succesfully read Normal
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

		bool ReadInt(out int number, out char lastChar)
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
					else if (readChar == ' ' && numberFound == false)
                    {// found a space before the start of the number

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
            if (flipFaceDirection)
            {
				x = -x;
				y = -y;
				z = -z;
            }
			if (x != float.NaN && y != float.NaN & z != float.NaN)
			{
				if (FlipYZ)
				{
					normals.Add(x, z, y);
				}
				else
				{
					normals.Add(x, y, -z);
				}
			}
		}

		float ReadFloat()
		{
			char readChar;
			bool numberFound = false;
			long number = 0;
			int decimalPlaces = 0;
			bool isDecimal = false;
			int sign = 1;
			bool hasExponent = false;
			int exponentSign = 1;
			int exponent = 0;
			bool keepGoing = true;
			while (keepGoing)
			{
				if (NextChar(out readChar))
				{
					switch (readChar)
					{
						case '\0':
							//found a null-value
							if (numberFound)
							{ // if we start with a space we continue
								keepGoing = false;
							}
							break;
						case ' ':
							//found a space
							if (numberFound)
							{ // if we start with a space we continue
								keepGoing = false;
							}
							break;
						case '\r':
							// end of the line, end of the floatvalue
							keepGoing = false;
							break;
						case '\n':
							// end of the line, end of the floatvalue
							keepGoing = false;
							break;
						case '.':
							// found a decimalpoint
							isDecimal = true;
							break;
						case 'e':
                            if (numberFound)
                            {
								hasExponent = true;
							}
							break;
						case '-':
							// found a negative-sign
							if (hasExponent)
							{
								exponentSign = -1;
							}
							else
							{
								sign = -1;
							}
							break;
						default:
							// no space or endof the line
							if (char.IsDigit(readChar))
							{
								numberFound = true;
                                if (!hasExponent)
                                {

                                
								number = (number * 10) + (int)char.GetNumericValue(readChar);
								if (isDecimal)
								{
									decimalPlaces++;
								}
								}
								else
                                {
									exponent = (exponent * 10) + (int)char.GetNumericValue(readChar);
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
                if (hasExponent)
                {
					value *= Mathf.Pow(10, (exponentSign * exponent));
                }
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


		static Material GetMaterial(ReadMTL.MaterialData md, Material sourceMaterial)
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


		public void CreateGameObject(Material defaultMaterial)
        {
			isFinished = false;
			StartCoroutine(Build(defaultMaterial));
        }

		public IEnumerator Build(Material defaultMaterial)
		{
			
			// remove de indices en vertices-lists because wo no longer need them
			vertices.EndWriting();
			vertices.SetupReading();
			//vertices.RemoveData();
			normals.EndWriting();
			normals.SetupReading();
			if(normals.Count()==0)
            {
				hasNormals = false;
            }
			//finish up the last submeshdata
			activeSubmesh.rawData.EndWriting();
			submeshes[activeSubmesh.name] = activeSubmesh;


            // set up materialList
            Material[] materials = new Material[submeshes.Count];
			
            // analyze all the vertices
            int vertexcount = 0;
			int indexcount = 0;
            int maxVerticesPerSubmesh = 0;
			List<string> keyList = new List<string>(submeshes.Keys);
			Submesh submesh;
			Vector3Int originalIndex;
			intList finalIndices = new intList();
			finalIndices.SetupWriting("finalIndices");
			Vector3List finalVertices = new Vector3List();
			finalVertices.SetupWriting("finalVertices");
			Vector3List finalNormals = new Vector3List();
			finalNormals.SetupWriting("finalNormals");
			for (int i = 0; i < keyList.Count; i++)
			{
				loadingObjScreen.ShowMessage("Model wordt samengesteld voor materiaal " + (i+1) + " van " +keyList.Count);
				yield return null;
				submesh = submeshes[keyList[i]];
				submesh.startVertex = vertexcount;
				Debug.Log(submesh.name);
				submesh.startIndex = indexcount;
				submesh.rawData.SetupReading();
				int numberOfIndices = submesh.rawData.numberOfVertices();
				Dictionary<Vector3Int, int> indexMap = new Dictionary<Vector3Int, int>(numberOfIndices);
				int subMeshIndexcount = 0;
                for (int j = 0; j < numberOfIndices; j++)
                {
                    if (j%2000==1)
                    {
						float percentage = (float)j / (float)numberOfIndices;
						loadingObjScreen.ProgressBar.Percentage(percentage);
						loadingObjScreen.ProgressBar.SetMessage(Mathf.RoundToInt(percentage * 100).ToString() + "%");
						yield return null;
						

                    }
					int index;
					originalIndex = submesh.rawData.ReadNext();
                    if (indexMap.ContainsKey(originalIndex))
                    {// use the already created vertexIndex
						index = indexMap[originalIndex];
                    }
					else
                    {// create an new vertex
						vertexcount++;
						Vector3 vertex = vertices.ReadItem(originalIndex.x);
						finalVertices.Add(vertex.x, vertex.y, vertex.z);
                        if (hasNormals)
                        {
							Vector3 normal = normals.ReadItem(originalIndex.y);
							finalNormals.Add(normal.x, normal.y, normal.z);
                        }
						
						index = indexMap.Count;
						indexMap.Add(originalIndex, index);
                    }
					finalIndices.Add(index);
					subMeshIndexcount++;
                }

				submesh.rawData.EndReading();
				submesh.rawData.RemoveData();
				submesh.vertexCount = vertexcount - submesh.startVertex;
				submesh.indexCount = subMeshIndexcount;
				indexcount += submesh.indexCount;
				submeshes[keyList[i]]=submesh;
			}
			vertices.EndReading();
			vertices.RemoveData();
			normals.EndReading();
			normals.RemoveData();
			finalIndices.EndWriting();
			finalIndices.SetupReading();
			finalVertices.EndWriting();
			finalVertices.SetupReading();
			finalNormals.EndWriting();
			




			// create an array of all the finalVertices

			Mesh mesh = new Mesh();
			int finalVertexCount = finalVertices.Count();
			Vector3[] defVertices = new Vector3[finalVertexCount];
			loadingObjScreen.ShowMessage("Materialen worden samengevoegd stap 1 van 3");
			for (int i = 0; i < finalVertexCount; i++)
            {
                if (i%10000==1)
                {
					float percentage = (float)i / (float)finalVertexCount;
					loadingObjScreen.ProgressBar.Percentage(percentage);
					loadingObjScreen.ProgressBar.SetMessage(Mathf.RoundToInt(percentage * 100).ToString() + "%");
					yield return null;
				}
				defVertices[i] = finalVertices.ReadItem(i);
            }
			finalVertices.EndReading();
			finalVertices.RemoveData();
			mesh.vertices = defVertices;

			if (hasNormals)
            {
				loadingObjScreen.ShowMessage("Materialen worden samengevoegd stap 2 van 3");
				finalNormals.SetupReading();
				Vector3[] defNormals = new Vector3[finalVertexCount];
				for (int i = 0; i < finalVertexCount; i++)
				{
					if (i % 10000 == 1)
					{
						float percentage = (float)i / (float)finalVertexCount;
						loadingObjScreen.ProgressBar.Percentage(percentage);
						loadingObjScreen.ProgressBar.SetMessage(Mathf.RoundToInt(percentage * 100).ToString() + "%");
						yield return null;
					}
					defNormals[i] = finalNormals.ReadItem(i);
				}
				mesh.normals = defNormals;
			finalNormals.EndReading();
			}
			

			finalNormals.RemoveData();

			int indexCount = finalIndices.numberOfVertices();
			int[] defIndices = new int[indexCount];
			loadingObjScreen.ShowMessage("Materialen worden samengevoegd stap 3 van 3");
			for (int i = 0; i < indexCount; i++)
			{
				if (i % 10000 == 1)
				{
					float percentage = (float)i / (float)indexCount;
					loadingObjScreen.ProgressBar.Percentage(percentage);
					loadingObjScreen.ProgressBar.SetMessage(Mathf.RoundToInt(percentage * 100).ToString() + "%");
					yield return null;
				}
				defIndices[i] = finalIndices.ReadNext();
			}

			finalIndices.EndReading();
			finalIndices.RemoveData();

			mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);
			mesh.SetIndexBufferData(defIndices, 0, 0, indexCount);

			mesh.subMeshCount = submeshes.Count;

			int submeshIndex = 0;
            foreach (var sm in submeshes)
            {

                SubMeshDescriptor smd = new SubMeshDescriptor();
                smd.indexStart = sm.Value.startIndex;
                smd.indexCount = sm.Value.indexCount;
                smd.topology = MeshTopology.Triangles;
                smd.baseVertex = sm.Value.startVertex;
                smd.vertexCount = sm.Value.vertexCount;
                mesh.SetSubMesh(submeshIndex, smd);
				Material mat=null;
                for (int i = 0; i < materialDataSlots.Count; i++)
                {
                    if (materialDataSlots[i].Name == sm.Value.name)
                    {
						mat = GetMaterial(materialDataSlots[i], defaultMaterial);

					}
                }
                if (mat==null)
                {
					mat = new Material(defaultMaterial);
                }
				materials[submeshIndex] = mat;
				mat = null;
                materials[submeshIndex].name = sm.Key;

                submeshIndex++;
            }
			if (!hasNormals)
			{
				mesh.RecalculateNormals();
			}
            //else
            //{
            //    mesh.RecalculateNormals();
            //}
            createdGameObject = new GameObject();
            MeshFilter mf = createdGameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = createdGameObject.AddComponent<MeshRenderer>();
            mr.materials = materials;
            mf.mesh = mesh;

			loadingObjScreen.Hide();
			isFinished = true;
        }

		private Vector3 CalculateNormal(Vector3 v1,Vector3 v2, Vector3 v3)
        {
			var dir = Vector3.Cross(v2 - v1, v3 - v1);
			var norm = Vector3.Normalize(dir);
			return norm;
		}
	}
}