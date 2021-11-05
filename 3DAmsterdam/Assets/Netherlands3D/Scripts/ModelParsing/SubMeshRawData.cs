using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
namespace Netherlands3D.ModelParsing
{
    public class SubMeshRawData : MonoBehaviour
    {
        BinaryWriter writer;
        FileStream fs;
        BinaryReader bReader;
        
        string filepath;
        string filename;
        public void SetupWriting(string name)
        {
            filename = name.Replace('/', '-');
            filename = filename.Replace(".", "");
            filename = filename.Replace("$", "");
            filepath = Application.persistentDataPath + "/" + filename + ".dat";
            // create the file if it doesnt already exist
            var datafile = File.Exists(filepath) ? File.Open(filepath, FileMode.Append) : File.Open(filepath, FileMode.CreateNew);
            datafile.Close();
            writer = new BinaryWriter(File.Open(filepath, FileMode.Append));
        }
        public void Add(int vertexIndex, int normalIndex, int textureIndex)
        {
            writer.Write(vertexIndex);
            writer.Write(normalIndex);
            writer.Write(textureIndex);
        }
        public void EndWriting()
        {
            writer.Flush();
            writer.Close();
            
        }
        public void SetupReading(string name = "")
        {
            if (name != "")
            {
                filename = name.Replace('/', '-');
                filename = filename.Replace(".", "");
                filename = filename.Replace("$", "");
                filepath = Application.persistentDataPath + "/" + filename + ".dat";
            }
            fs = File.OpenRead(filepath);
            bReader = new BinaryReader(fs);
        }

        public void RemoveData()
        {
            File.Delete(filepath);
        }

        public int numberOfVertices()
        {
            return (int)fs.Length / 12;
        }
        public Vector3Int ReadNext()
        {
            Vector3Int output = new Vector3Int();
            output.x = bReader.ReadInt32();
            output.y = bReader.ReadInt32();
            output.z = bReader.ReadInt32();
            return output;
        }
        public void EndReading()
        {
            bReader.Close();
            fs.Close();

        }
    }
}
