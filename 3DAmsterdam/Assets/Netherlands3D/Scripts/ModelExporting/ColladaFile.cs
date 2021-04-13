using ConvertCoordinates;
using Netherlands3D.JavascriptConnection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;


public class ColladaFile : MonoBehaviour
{
	private XmlTextWriter writer;
	private StringWriter stringWriter;

	private List<Material> materials;
	private List<Mesh> meshes;

	public void CreateCollada()
	{
		stringWriter = new StringWriter();
		writer = new XmlTextWriter(stringWriter);
		writer.WriteStartDocument(false);
		writer.WriteStartElement("COLLADA");
		writer.WriteAttributeString("xmlns", "http://www.collada.org/2008/03/COLLADASchema");
		writer.WriteAttributeString("version", "1.5.0");

		writer.WriteStartElement("asset");
		writer.WriteStartElement("created");
		writer.WriteString("Now");
		writer.WriteEndElement();
		writer.WriteStartElement("modified");
		writer.WriteString("Now");
		writer.WriteEndElement();
		writer.WriteStartElement("revision");
		writer.WriteString("1");
		writer.WriteEndElement();
		writer.WriteEndElement();

		writer.WriteStartElement("library_effects");
		writer.WriteStartElement("effect");
		writer.WriteAttributeString("xmlns", "http://www.collada.org/2008/03/COLLADASchema");
		writer.WriteEndElement();

		//Material Effects
		foreach(Material material in materials)
		{
			Color materialColor = material.GetColor("_MainColor");

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
			writer.WriteString(materialColor.a.ToString());
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

		writer.WriteStartElement("library_geometries");
		foreach (Mesh mesh in meshes)
		{
			writer.WriteStartElement("geometry");
			writer.WriteAttributeString("id", mesh.name);
			writer.WriteAttributeString("name", mesh.name);
			writer.WriteStartElement("mesh");

			writer.WriteStartElement("source");
			writer.WriteAttributeString("id", mesh.name + "_verts");
			writer.WriteStartElement("float_array");
			writer.WriteAttributeString("id", mesh.name + "_verts-array");
			writer.WriteAttributeString("count", mesh.vertexCount.ToString());

			//Verts for this mesh
			for (int i = 0; i < mesh.vertexCount; i++)
			{
				var vert = mesh.vertices[i];
				writer.WriteString(vert.x + " " + vert.y + " " + vert.z);
				if(i < mesh.vertexCount -1){	
					writer.WriteString(" ");
				}
			}

			writer.WriteStartElement("technique_common");
			writer.WriteStartElement("accessor");
			writer.WriteAttributeString("source", "#" + mesh.name + "_verts-array");
			writer.WriteAttributeString("count", mesh.vertexCount.ToString());
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
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();

		}
		writer.WriteEndElement();

		writer.WriteEndElement();

		writer.WriteEndElement(); // COLLADA
		writer.WriteEndDocument();
		writer.Flush();
		//writer.Close(); //Should probably stay open to reuse for reading buffer?
	}


	public void AddObject(List<Vector3RD> clippedVerticesRD, string objectName, Material objectMaterial)
	{
		if (materials == null) materials = new List<Material>();
		materials.Add(objectMaterial);


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