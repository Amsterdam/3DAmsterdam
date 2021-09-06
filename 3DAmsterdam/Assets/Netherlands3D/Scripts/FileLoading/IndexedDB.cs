/* Copyright(C)  X Gemeente
                 X Amsterdam
                 X Economic Services Departments
Licensed under the EUPL, Version 1.2 or later (the "License");
You may not use this work except in compliance with the License. You may obtain a copy of the License at:
https://joinup.ec.europa.eu/software/page/eupl
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied. See the License for the specific language governing permissions and limitations under the License.
*/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using Netherlands3D.ModelParsing;
using Netherlands3D.Traffic.VISSIM;
using Netherlands3D.Interface;

public class IndexedDB : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void SyncFilesFromIndexedDB();
    [DllImport("__Internal")]
    private static extern void SyncFilesToIndexedDB();
    [DllImport("__Internal")]
    private static extern void SendPersistentDataPath(string str);

    public CsvFilePanel csvLoader;

    public List<string> filenames = new List<string>();
    public int numberOfFilesToLoad = 0;
    private int fileCount = 0;

    public void Start()
    {
        #if !UNITY_EDITOR && UNITY_WEBGL
        SendPersistentDataPath(Application.persistentDataPath);
        #endif
    }

    // Called from javascript, the total number of files that are being loaded.
    public void FileCount(int count)
    {
        numberOfFilesToLoad = count;
        fileCount = 0;
        filenames = new List<string>();
        Debug.Log("expecting " + count + " files");
        LoadingScreen.Instance.ShowMessage($"{numberOfFilesToLoad} {((numberOfFilesToLoad>1) ? "bestanden worden" : "bestand wordt")} ingeladen..");
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
    public void LoadFileError(string name)
    {
        fileCount++;
        LoadingScreen.Instance.Hide();
        Debug.Log("unable to load " + name);
    }


    // runs while javascript is busy saving files to indexedDB.
    IEnumerator WaitForFilesToBeLoaded()
    {
        while (fileCount<numberOfFilesToLoad)
        {
            yield return null;
        }
        numberOfFilesToLoad = 0;
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
        ProcessAllFiles();
    }

    void ProcessAllFiles()
    {
        //Figure out the filetypes so we know which function to start
        string extention = Path.GetExtension(filenames[0]);
        Debug.Log("first file-extention = " +extention);
        if (extention == ".obj" || extention == ".mtl")
        {
            GetComponent<ObjStringLoader>().LoadOBJFromIndexedDB(filenames, ClearDatabase);
        }
        else if (extention == ".csv")
        {
            csvLoader.LoadCsvFromFile(filenames[0], ClearDatabase);
        }
        else if (extention == ".fzp")
        {
            GetComponent<VissimStringLoader>().LoadVissimFromFile(filenames[0], ClearDatabase);
        }
    }

    public void ClearDatabase(bool succes)
    {
        filenames.Clear();
        if (succes)
        {
            SyncFilesToIndexedDB();
        }
    }

}
