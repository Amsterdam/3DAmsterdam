using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using Netherlands3D.ModelParsing;




public class IndexedDB : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void SyncFilesFromIndexedDB();
    [DllImport("__Internal")]
    private static extern void SyncFilesToIndexedDB();
    [DllImport("__Internal")]
    private static extern void SendPersistentDataPath(string str);

    public Text urlstring;
    public List<string> filenames = new List<string>();
    public int numberOfFIlesToLoad = 0;
    private int fileCount = 0;
    public void Start()
    {
        SendPersistentDataPath(Application.persistentDataPath);
       // System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
    }

   // called from javascript, the total number of files that are being loaded
    public void FileCount(int count)
    {
        numberOfFIlesToLoad = count;
        fileCount = 0;
        filenames = new List<string>();
        Debug.Log("expecting " + count + " files");
        StartCoroutine(WaitForFilesToBeLoaded());
    }
    
    //called from javascript
    public void LoadFile(string filename)
    {
        filenames.Add(filename);
        fileCount++;
        Debug.Log("received: "+filename);        
    }

    // called from javascript
    public void loadFileError(string name)
    {
        fileCount++;
        Debug.Log("unable to load " + name);
    }


    // runs while javascript is busy saving files to indexedDB.
    IEnumerator WaitForFilesToBeLoaded()
    {
        while (fileCount<numberOfFIlesToLoad)
        {
            yield return null;
        }
        numberOfFIlesToLoad = 0;
        fileCount = 0;
        ProcessFiles();
    }

    
    public void ProcessFiles()
    {
        // start js-function to update the contents of application.persistentdatapath to match the contents of indexedDB.
        SyncFilesFromIndexedDB();
    }

    public void IndexedDBUpdated() // called from SyncFilesFromIndexedDB
    {
       // GetComponent<ObjStringLoader>().LoadOBJFromIndexedDB(filenames);
       // filenames.Clear();
        processAllFiles();
    }

    void processAllFiles()
    {
        ReadOBJ();
        //for (int i = 0; i < filenames.Count; i++)
        //{
        //    ProcessFile(filenames[i]);
        //}
        filenames.Clear();
        //SyncFilesToIndexedDB();
    }

    void ReadOBJ()
    {

        GetComponent<ObjStringLoader>().LoadOBJFromIndexedDB(filenames);
    }

    void ProcessFile(string filename)
    { 
        if (File.Exists(Application.persistentDataPath + "/"+filename))
        {
            Debug.Log("file found");
            streamreadfile(Application.persistentDataPath + "/"+filename);
            
            //BinaryFormatter bf = new BinaryFormatter();
            //string text = File.ReadAllText(Application.persistentDataPath + "/MySharedData.txt");
            //Debug.Log(text);
        }
        else
        {
            Debug.Log(Application.persistentDataPath + "/" + filename + " not found");
        }
    }
    void streamreadfile(string path)
    {
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        char[] foundChar = new char[1];
        int lineEndCounter = 0;
        long size=0;
        using (StreamReader streamReader = new StreamReader(fileStream,System.Text.Encoding.UTF8))
        {
           
            while (streamReader.Peek() >= 0)
            {
                size++;
                char character = (char)streamReader.Read();
                if (character == '\r')
                {
                    lineEndCounter++;
                }
                
            }
        }
        Debug.Log(lineEndCounter + " lines");
        Debug.Log("filesize =" + size + "characters");
       // File.Delete(path);
       
    }
}