using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
namespace Netherlands3D.ModelParsing
{
    public class intList : MonoBehaviour
    {
        BinaryWriter writer;
        FileStream fs;
        BinaryReader bReader;
        string filepath;
        public void SetupWriting(string name)
        {
            filepath = Application.persistentDataPath + "/" + name + ".dat";
            writer = new BinaryWriter(File.Open(filepath, FileMode.OpenOrCreate));
        }
        public void Add(int vertexIndex)
        {
            writer.Write(vertexIndex);

        }
        public void EndWriting()
        {
            writer.Close();
        }
        public void SetupReading(string name = "")
        {
            if (name != "")
            {
                filepath = Application.persistentDataPath + "/" + name + ".dat";
            }
            fs = File.OpenRead(filepath);
            bReader = new BinaryReader(fs);
        }

        public int numberOfVertices()
        {
            return (int)fs.Length / 4;
        }
        public int ReadNext()
        {
            
            int output = bReader.ReadInt32();

            return output;
        }
        public void EndReading()
        {
            bReader.Close();
            fs.Close();

        }
        public void RemoveData()
        {
            File.Delete(filepath);
        }
    }
}

