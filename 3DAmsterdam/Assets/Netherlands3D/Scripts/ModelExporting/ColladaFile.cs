using Netherlands3D.Core;
using Netherlands3D.JavascriptConnection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using UnityEngine;

/// <summary>
/// Please refer to the official collada 1.4 documentation this Collada XML generation was based on.
/// https://www.khronos.org/files/collada_spec_1_4.pdf 
/// We chose the 1.4 version because this is the version officialy supported by the Sketchup version most of our users own.
/// Files exported with this script have been successfully tested imported in Sketchup Pro 2019, and Blender 2.92
/// </summary>
public class ColladaFile
{
	[DllImport("__Internal")]
	private static extern void DownloadFile(byte[] array, int byteLength, string fileName);

	private XmlTextWriter writer;
	private StringWriter stringWriter;
	private List<Material> materials;
	private Dictionary<string, List<Vector3RD>> objectsTriangles;

	private bool generateGeoHeader = false;
	private Vector3WGS geoCoordinates = default;

	public void AddObject(List<Vector3RD> triangleVertices, string objectName, Material objectMaterial)
	{
		if (objectsTriangles == null) objectsTriangles = new Dictionary<string, List<Vector3RD>>();
		if (materials == null) materials = new List<Material>();

		if (!materials.Contains(objectMaterial))
		{
			materials.Add(objectMaterial);
		}

		if (!objectsTriangles.ContainsKey(objectName))
		{
			objectsTriangles.Add(objectName, triangleVertices);
		}
		else{
			//existing object with same name adds their triangles to the existing object
			objectsTriangles[objectName].AddRange(triangleVertices);
		}
	}

	/// <summary>
	/// Write the XML output based on our list of materials and objects+triangles
	/// </summary>
	public void CreateCollada(bool addGeoLocation = false, Vector3WGS geolocationWGS = default)
	{
		if (addGeoLocation)
		{
			generateGeoHeader = true;
			geoCoordinates = geolocationWGS;
		}

		stringWriter = new StringWriter();
		writer = new XmlTextWriter(stringWriter);
		WriteDocumentHeader();
		WriteAssetHeader();

		//Library for material Effects
		writer.WriteStartElement("library_effects");	
		foreach (Material material in materials)
		{

			WriteMaterialEffects(material);
		}
		writer.WriteEndElement();

		//Library for materials 
		writer.WriteStartElement("library_materials");
		foreach (Material material in materials)
		{
			WriteMaterial(material);
		}
		writer.WriteEndElement();

		//Library for geometries
		if (objectsTriangles != null && objectsTriangles.Count > 0)
		{
			writer.WriteStartElement("library_geometries");

			foreach (var mesh in objectsTriangles)
			{
				WriteGeometry(mesh);
			}
			writer.WriteEndElement(); //end library_geometries
		}

		//Library for scenes
		writer.WriteStartElement("library_visual_scenes");
		writer.WriteStartElement("visual_scene");
		writer.WriteAttributeString("id", "DefaultScene");

		if (generateGeoHeader)
		{
			//geolocation (officialy only supported from collada 1.5.1)
			writer.WriteStartElement("asset");
			writer.WriteStartElement("coverage");
			writer.WriteStartElement("geographic_location");
			writer.WriteStartElement("longitude");
			writer.WriteString(geoCoordinates.lon.ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement(); //end longitude
			writer.WriteStartElement("latitude");
			writer.WriteString(geoCoordinates.lat.ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement(); //end latitude
			writer.WriteStartElement("altitude");
			writer.WriteAttributeString("mode", "relativeToGround");
			writer.WriteString(geoCoordinates.h.ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement(); //end altitude
			writer.WriteEndElement(); //end geographic_location
			writer.WriteEndElement(); //end coverage
			writer.WriteEndElement(); //end asset
		}

		if (objectsTriangles != null && objectsTriangles.Count > 0)
		{
			foreach (var mesh in objectsTriangles)
			{
				//Create nodes that reference instanced geometry
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
				writer.WriteAttributeString("url", "#" + mesh.Key + "-geometry");
				writer.WriteStartElement("bind_material");
				writer.WriteStartElement("technique_common");
				writer.WriteStartElement("instance_material");
				writer.WriteAttributeString("symbol", mesh.Key + "-symbol");
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

		//Close document
		writer.WriteEndElement(); // COLLADA
		writer.WriteEndDocument();
		writer.Flush();
		writer.Close();
	}

	/// <summary>
	/// Writes geometry node we can reference in our model nodes
	/// </summary>
	/// <param name="meshNameAndVerts">Key value pair with name,and a list of verts</param>
	/// <param name="writeVertexNormals">Optionaly write out a list of vert normals (matches the length of verts, so adds a lot of data)</param>
	private void WriteGeometry(KeyValuePair<string, List<Vector3RD>> meshNameAndVerts, bool writeVertexNormals = false)
	{
		writer.WriteStartElement("geometry");
		writer.WriteAttributeString("id", meshNameAndVerts.Key + "-geometry");
		writer.WriteAttributeString("name", meshNameAndVerts.Key);
		writer.WriteStartElement("mesh");

		writer.WriteStartElement("source");
		writer.WriteAttributeString("id", meshNameAndVerts.Key + "-Pos");
		writer.WriteStartElement("float_array");
		writer.WriteAttributeString("id", meshNameAndVerts.Key + "_Pos-array");
		writer.WriteAttributeString("count", meshNameAndVerts.Value.Count.ToString());

		//Verts for this mesh
		var meshVertices = meshNameAndVerts.Value;
		for (int i = 0; i < meshVertices.Count; i++)
		{
			var vert = meshVertices[i];
			writer.WriteString(vert.x + " " + vert.y + " " + vert.z);
			if (i < meshVertices.Count - 1)
			{
				writer.WriteString(" ");
			}
		}

		writer.WriteEndElement(); //end float_array
		writer.WriteStartElement("technique_common");
		writer.WriteStartElement("accessor");
		writer.WriteAttributeString("count", "4");
		writer.WriteAttributeString("source", "#" + meshNameAndVerts.Key + "_verts-array");
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

		if (writeVertexNormals)
		{
			//start source for normals
			writer.WriteStartElement("source");
			writer.WriteAttributeString("id", meshNameAndVerts.Key + "-Normal");
			writer.WriteStartElement("float_array");
			writer.WriteAttributeString("id", meshNameAndVerts.Key + "_Normal-array");
			writer.WriteAttributeString("count", meshVertices.Count.ToString());

			//normals
			for (int i = 0; i < meshVertices.Count; i++)
			{
				var vert = meshVertices[i];
				writer.WriteString("0 0 0"); //Zero untill we have a list of normals
				if (i < meshVertices.Count - 1)
				{
					writer.WriteString(" ");
				}
			}
			writer.WriteEndElement(); //end float_array
			writer.WriteStartElement("technique_common");
			writer.WriteStartElement("accessor");
			//start source for vertex positions
			writer.WriteAttributeString("count", "4");
			writer.WriteAttributeString("source", "#" + meshNameAndVerts.Key + "_Normal-array");
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
		}

		writer.WriteStartElement("vertices");
		writer.WriteAttributeString("id", meshNameAndVerts.Key + "-Vtx");
		writer.WriteStartElement("input"); //start input vert positions
		writer.WriteAttributeString("semantic", "POSITION");
		writer.WriteAttributeString("source", "#" + meshNameAndVerts.Key + "-Pos");
		writer.WriteEndElement(); //end input vert position

		if (writeVertexNormals)
		{
			writer.WriteStartElement("input"); //start input normal positions
			writer.WriteAttributeString("semantic", "NORMAL");
			writer.WriteAttributeString("source", "#" + meshNameAndVerts.Key + "-Normal");
			writer.WriteEndElement(); //end input normal positions
		}
		writer.WriteEndElement(); //end vertices

		//Now write our list of triangles (long list of all vertex indices (3 indices per triangle))
		writer.WriteStartElement("triangles");
		writer.WriteAttributeString("count", (meshVertices.Count / 3).ToString());
		writer.WriteAttributeString("material", meshNameAndVerts.Key + "-symbol");
		writer.WriteStartElement("input");
		writer.WriteAttributeString("semantic", "VERTEX");
		writer.WriteAttributeString("source", "#" + meshNameAndVerts.Key + "-Vtx");
		writer.WriteAttributeString("offset", "0");
		writer.WriteEndElement();
		writer.WriteStartElement("p"); //start p
		for (int i = 0; i < meshVertices.Count; i += 3)
		{
			var vert = meshVertices[i];
			// reverse winding order to comply with collade-specification (CCW)
			writer.WriteString(i.ToString() + " " + (i + 2).ToString() + " " + (i + 1).ToString() + " ");
		}
		writer.WriteEndElement(); //end p
		writer.WriteEndElement(); //end triangles

		writer.WriteEndElement(); //end mesh
		writer.WriteEndElement(); //end geometry
	}

	/// <summary>
	/// Writes a collada material referencing an instance effects (using material name)
	/// </summary>
	/// <param name="material"></param>
	private void WriteMaterial(Material material)
	{
		writer.WriteStartElement("material");
		writer.WriteAttributeString("id", material.name.Split(' ')[0]);
		writer.WriteStartElement("instance_effect");
		writer.WriteAttributeString("url", "#" + material.name.Split(' ')[0] + "-effect");
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	/// <summary>
	/// Creates a material effect node we can reference in our material nodes
	/// </summary>
	/// <param name="material">The source material to pluck the shading information from</param>
	private void WriteMaterialEffects(Material material)
	{
		Color materialColor = material.GetColor("_BaseColor");
		writer.WriteStartElement("effect");
		writer.WriteAttributeString("id", material.name.Split(' ')[0] + "-effect");
		writer.WriteStartElement("profile_COMMON");
		writer.WriteStartElement("technique");
		writer.WriteAttributeString("sid", "common");
		writer.WriteStartElement("lambert");

		//Material properties (lots of extra fields possible here. please refer to https://www.khronos.org/files/collada_spec_1_4.pdf for details)
		writer.WriteStartElement("emission");
		writer.WriteStartElement("color");
		writer.WriteString("0.0 0.0 0.0 1.0");
		writer.WriteEndElement();
		writer.WriteEndElement();

		writer.WriteStartElement("diffuse");
		writer.WriteStartElement("color");
		writer.WriteString(materialColor.r + " " + materialColor.g + " " + materialColor.b + " " + materialColor.a);
		writer.WriteEndElement();
		writer.WriteEndElement();

		writer.WriteStartElement("index_of_refraction");
		writer.WriteStartElement("float");
		writer.WriteAttributeString("sid", "ior");
		writer.WriteString("1.45");
		writer.WriteEndElement();
		writer.WriteEndElement();

		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void WriteDocumentHeader()
	{
		writer.WriteStartDocument(false);
		writer.WriteStartElement("COLLADA");
		writer.WriteAttributeString("xmlns", "http://www.collada.org/2008/03/COLLADASchema");
		writer.WriteAttributeString("version", "1.4.1");
	}

	private void WriteAssetHeader()
	{
		writer.WriteStartElement("asset");
		writer.WriteStartElement("contributor");
		writer.WriteStartElement("authoring_tool");
		writer.WriteString("3D Amsterdam");
		writer.WriteEndElement(); //end authoring_tool
		writer.WriteEndElement(); //end contributor
		writer.WriteStartElement("created");
		writer.WriteString(DateTime.Now.ToString());
		writer.WriteEndElement(); //end created
		writer.WriteStartElement("modified");
		writer.WriteString(DateTime.Now.ToString());
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
	}

	public void Save(string filename = "")
	{
#if UNITY_EDITOR
		var mydocs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
		var filePath = Path.Combine(mydocs, (filename != "") ? filename : "ColladaExport.dae");
		Debug.Log(filePath);
		File.WriteAllText(filePath, stringWriter.ToString());
		stringWriter = null;
		writer = null;
		return;
#endif

		if (stringWriter != null)
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(stringWriter.ToString());
			stringWriter = null;
			writer = null;
			DownloadFile(byteArray, byteArray.Length, (filename!="") ? filename : "ColladaExport.dae");
		}
		else
		{
			Debug.Log("cant write file");
		}

	}
}