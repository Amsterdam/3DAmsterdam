using Netherlands3D.JavascriptConnection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;


public class ColladaFile : MonoBehaviour
{
    private XmlTextWriter writer;
    private MemoryStream memoryStream;
    private StringWriter stringWriter;

    public void CreateCollada()
    {
        stringWriter = new StringWriter();
        writer = new XmlTextWriter(stringWriter);
        writer.WriteStartDocument(false);
        writer.WriteStartElement("COLLADA");
        writer.WriteAttributeString("xmlns", "http://www.collada.org/2008/03/COLLADASchema");
        writer.WriteAttributeString("version", "1.5.0");



        writer.WriteEndElement(); // COLLADA
        writer.WriteEndDocument();
        writer.Flush();
        //writer.Close(); //Should probably stay open to reuse for reading buffer?
    }

    public void Save()
    {
#if !UNITY_EDITOR

        var mydocs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        File.WriteAllText(Path.Combine(mydocs, "testColladaFile.dae"), stringWriter.ToString());
        return;
#endif

        if (memoryStream.Length > 0)
        {
            JavascriptMethodCaller.DownloadByteArrayAsFile(memoryStream.ToArray(), memoryStream.ToArray().Length, "testfile.dae"); ;
        }
        else
        {
            Debug.Log("cant write file");
        }

    }
}