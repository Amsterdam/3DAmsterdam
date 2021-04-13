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

		writer.WriteEndElement(); // COLLADA
		writer.WriteEndDocument();
		writer.Flush();
		//writer.Close(); //Should probably stay open to reuse for reading buffer?
	}


	public void AddObject(List<Vector3RD> clippedVerticesRD, string objectName, Color objectColor)
	{

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