using ConvertCoordinates;
using Netherlands3D.JavascriptConnection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

public class ColladaFile
{
	private XmlTextWriter writer;
	private StringWriter stringWriter;

	private List<Material> materials;

	private List<Mesh> meshes;
	private List<Vector3RD> vertices;
	private List<Mesh> triangles;

	private Dictionary<string, List<Vector3RD>> objectTriangles;

	public void AddObject(List<Vector3RD> triangleVertices, string objectName, Material objectMaterial)
	{
		if (objectTriangles == null) objectTriangles = new Dictionary<string, List<Vector3RD>>();
		if (materials == null) materials = new List<Material>();

		if (!materials.Contains(objectMaterial))
		{
			materials.Add(objectMaterial);
		}

		if (!objectTriangles.ContainsKey(objectName))
		{
			objectTriangles.Add(objectName, triangleVertices);
		}
		vertices = triangleVertices;
	}

	public void CreateCollada()
	{
		stringWriter = new StringWriter();
		writer = new XmlTextWriter(stringWriter);
		writer.WriteStartDocument(false);
		writer.WriteStartElement("COLLADA");
		writer.WriteAttributeString("xmlns", "http://www.collada.org/2008/03/COLLADASchema");
		writer.WriteAttributeString("version", "1.5.0");

		writer.WriteStartElement("asset");
		writer.WriteStartElement("contributor");
		writer.WriteStartElement("authoring_tool");
		writer.WriteString("3D Amsterdam");
		writer.WriteEndElement(); //end authoring_tool
		writer.WriteEndElement(); //end contributor
		writer.WriteStartElement("created");
		writer.WriteString(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
		writer.WriteEndElement(); //end created
		writer.WriteStartElement("modified");
		writer.WriteString(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
		writer.WriteEndElement(); //end modified
		writer.WriteStartElement("revision");
		writer.WriteString("1");
		writer.WriteEndElement(); //end revision
		writer.WriteStartElement("unit");
		writer.WriteAttributeString("meter", "1");
		writer.WriteAttributeString("name", "meter");
		writer.WriteEndElement(); //end unit
		writer.WriteStartElement("up_axis");
		writer.WriteString("Z_UP");
		writer.WriteEndElement(); //end up_axis
		writer.WriteEndElement(); //end asset

		writer.WriteStartElement("library_effects");
		writer.WriteEndElement();

		//Material Effects
		foreach(Material material in materials)
		{
			Color materialColor = material.GetColor("_BaseColor");

			writer.WriteStartElement("effect");
			writer.WriteAttributeString("id", material.name);
			writer.WriteStartElement("profile_COMMON");
			writer.WriteStartElement("technique");
			writer.WriteAttributeString("sid", "phong1");
			writer.WriteStartElement("phong");

			//Phong properties
			writer.WriteStartElement("emission");
			writer.WriteStartElement("color");
			writer.WriteString("0.0 0.0 0.0 0.0");
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.WriteStartElement("ambient");
			writer.WriteStartElement("color");
			writer.WriteString("0.0 0.0 0.0 0.0");
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.WriteStartElement("diffuse");
			writer.WriteStartElement("color");
			writer.WriteString(materialColor.r + " " + materialColor.g + " " + materialColor.b + " " + materialColor.a);
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.WriteStartElement("specular");
			writer.WriteStartElement("color");
			writer.WriteString("0.0 0.0 0.0 0.0");
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.WriteStartElement("shininess");
			writer.WriteStartElement("float");
			writer.WriteString("5.0");
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.WriteStartElement("reflective");
			writer.WriteStartElement("color");
			writer.WriteString("0.0 0.0 0.0 0.0");
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.WriteStartElement("reflectivity");
			writer.WriteStartElement("float");
			writer.WriteString("0.0");
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.WriteStartElement("transparent");
			writer.WriteStartElement("color");
			writer.WriteString("0.0 0.0 0.0 0.0");
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.WriteStartElement("transparency");
			writer.WriteStartElement("float");
			writer.WriteString((1.0f-materialColor.a).ToString());
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		//Materials
		writer.WriteStartElement("library_materials");
		foreach (Material material in materials)
		{
			writer.WriteStartElement("material");
			writer.WriteAttributeString("id", material.name);
			writer.WriteStartElement("instance_effect");
			writer.WriteAttributeString("url","#"+material.name);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		writer.WriteEndElement();

		if(objectTriangles != null && objectTriangles.Count > 0){ 
			writer.WriteStartElement("library_geometries");

			foreach (var mesh in objectTriangles)
			{
				writer.WriteStartElement("geometry");
				writer.WriteAttributeString("id", mesh.Key);
				writer.WriteAttributeString("name", mesh.Key);
				writer.WriteStartElement("mesh");

				writer.WriteStartElement("source");
				writer.WriteAttributeString("id", mesh.Key + "-Pos");
				writer.WriteStartElement("float_array");
				writer.WriteAttributeString("id", mesh.Key + "_Pos-array");
				writer.WriteAttributeString("count", mesh.Value.Count.ToString());

				//Verts for this mesh
				var meshVertices = mesh.Value;
				for (int i = 0; i < meshVertices.Count; i++)
				{
					var vert = meshVertices[i];
					writer.WriteString(vert.x + " " + vert.y + " " + vert.z);
					if(i < meshVertices.Count - 1){	
						writer.WriteString(" ");
					}
				}

				writer.WriteEndElement(); //end float_array
				writer.WriteStartElement("technique_common");
				writer.WriteStartElement("accessor");
				//start source for vertex positions
				writer.WriteAttributeString("source", "#" + mesh.Key + "_verts-array");
				writer.WriteAttributeString("count", meshVertices.Count.ToString());
				writer.WriteAttributeString("stride", "3");
				writer.WriteStartElement("param");
				writer.WriteAttributeString("name", "X");
				writer.WriteAttributeString("type", "float");
				writer.WriteEndElement();
				writer.WriteStartElement("param");
				writer.WriteAttributeString("name", "Y");
				writer.WriteAttributeString("type", "float");
				writer.WriteEndElement();
				writer.WriteStartElement("param");
				writer.WriteAttributeString("name", "Z");
				writer.WriteAttributeString("type", "float");
				writer.WriteEndElement();
				writer.WriteEndElement(); //end accessor
				writer.WriteEndElement(); //end technique_common
				writer.WriteEndElement(); //end source

				//start source for normals
				writer.WriteStartElement("source");
				writer.WriteAttributeString("id", mesh.Key + "-Normal");
				writer.WriteStartElement("float_array");
				writer.WriteAttributeString("id", mesh.Key + "_Normal-array");
				writer.WriteAttributeString("count", "1"); //just one normal for now untill we have a list
				writer.WriteString("0 0 0");
				writer.WriteEndElement(); //end float_array
				writer.WriteStartElement("technique_common");
				writer.WriteStartElement("accessor");
				//start source for vertex positions
				writer.WriteAttributeString("source", "#" + mesh.Key + "_Normal-array");
				writer.WriteAttributeString("count", "1");
				writer.WriteAttributeString("stride", "3");
				writer.WriteStartElement("param");
				writer.WriteAttributeString("name", "X");
				writer.WriteAttributeString("type", "float");
				writer.WriteEndElement();
				writer.WriteStartElement("param");
				writer.WriteAttributeString("name", "Y");
				writer.WriteAttributeString("type", "float");
				writer.WriteEndElement();
				writer.WriteStartElement("param");
				writer.WriteAttributeString("name", "Z");
				writer.WriteAttributeString("type", "float");
				writer.WriteEndElement();
				writer.WriteEndElement(); //end accessor
				writer.WriteEndElement(); //end technique_common
				writer.WriteEndElement(); //end source

				writer.WriteStartElement("vertices");
				writer.WriteAttributeString("id", mesh.Key + "-Vtx");
				writer.WriteStartElement("input");
				writer.WriteAttributeString("semantic", "POSITION");
				writer.WriteAttributeString("source", "#" + mesh.Key + "-Pos");
				writer.WriteEndElement(); //end input
				writer.WriteEndElement(); //end vertices

				writer.WriteStartElement("polygons");
				writer.WriteAttributeString("count", (meshVertices.Count / 3).ToString());
				writer.WriteAttributeString("material", "WHITE");
				writer.WriteStartElement("input");
				writer.WriteAttributeString("semantic", "VERTEX");
				writer.WriteAttributeString("source", "#" + mesh.Key + "-Vtx");
				writer.WriteAttributeString("offset", "0");
				writer.WriteEndElement();
				writer.WriteStartElement("input");
				writer.WriteAttributeString("semantic", "NORMAL");
				writer.WriteAttributeString("source", "#" + mesh.Key + "-Normal");
				writer.WriteAttributeString("offset", "1");
				writer.WriteEndElement();
				for (int i = 0; i < meshVertices.Count; i+=3)
				{
					writer.WriteStartElement("p"); //start p
					var vert = meshVertices[i];
					writer.WriteString(i.ToString() + " 0 " + (i+1).ToString() + " 0 " + (i + 2).ToString() + " 0"); //triangle
					writer.WriteEndElement(); //end p
				}
				writer.WriteEndElement(); //end polygon
				writer.WriteEndElement(); //end mesh
				writer.WriteEndElement(); //end geometry
			}
			writer.WriteEndElement(); //end library_geometries
		}

		writer.WriteStartElement("library_visual_scenes");
		writer.WriteStartElement("visual_scene");
		writer.WriteAttributeString("id", "DefaultScene");
		if (objectTriangles != null && objectTriangles.Count > 0)
		{
			foreach (var mesh in objectTriangles)
			{
				writer.WriteStartElement("node");
				writer.WriteAttributeString("id", mesh.Key);
				writer.WriteStartElement("translate");
				writer.WriteString(" 0 0 0");
				writer.WriteEndElement(); //end translate
				
				writer.WriteStartElement("scale");
				writer.WriteString(" 1 1 1");
				writer.WriteEndElement(); //end scale

				//reference geometry
				writer.WriteStartElement("instance_geometry");
				writer.WriteAttributeString("url", "#" + mesh.Key);
				writer.WriteStartElement("bind_material");
				writer.WriteStartElement("technique_common");
				writer.WriteStartElement("instance_material");
				writer.WriteAttributeString("symbol", "WHITE");
				writer.WriteAttributeString("target", "#" + mesh.Key);
				writer.WriteEndElement(); //end instance_material
				writer.WriteEndElement(); //end technique_common
				writer.WriteEndElement(); //end bind_material
				writer.WriteEndElement(); //end instance_geometry
				writer.WriteEndElement(); //end node
			}
		}
		writer.WriteEndElement(); //end visual_scene
		writer.WriteEndElement(); //end library_visual_scenes

		//Scene
		writer.WriteStartElement("scene");
		writer.WriteStartElement("instance_visual_scene");
		writer.WriteAttributeString("url", "#DefaultScene");
		writer.WriteEndElement(); // end instance_visual_scene
		writer.WriteEndElement(); // end scene
		writer.WriteEndElement(); // COLLADA
		writer.WriteEndDocument();
		writer.Flush();
		//writer.Close(); //Should probably stay open to reuse for reading buffer?
	}

	public void Save()
	{
#if UNITY_EDITOR
		var mydocs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
		File.WriteAllText(Path.Combine(mydocs, "testColladaFile.dae"), stringWriter.ToString());
		return;
#endif

		if (stringWriter != null)
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(stringWriter.ToString());
			JavascriptMethodCaller.DownloadByteArrayAsFile(byteArray, byteArray.Length, "testfile.dae"); ;
		}
		else
		{
			Debug.Log("cant write file");
		}

	}
}