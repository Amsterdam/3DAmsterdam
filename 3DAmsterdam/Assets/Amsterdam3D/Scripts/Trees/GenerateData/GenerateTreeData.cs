using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class GenerateTreeData : MonoBehaviour
{
    [SerializeField]
    private GameObjectsGroup treeTypes;

    [SerializeField]
    private TextAsset[] bomenCsvDataFiles;

    public void Start()
    {
        ParseTreeData();
        TraverseTileFiles();
    }

    private void ParseTreeData()
    {
        foreach (var csvData in bomenCsvDataFiles)
        {
            string[] lines = csvData.text.Split(char.Parse(Environment.NewLine));
            foreach (var line in lines)
            {
                ParseTree(line);
            }
        }
    }

    private void ParseTree(string line)
    {
        string[] cell = line.Split(';');

        //OBJECTNUMMER;Soortnaam_NL;Boomnummer;Soortnaam_WTS;Boomtype;Boomhoogte;Plantjaar;Eigenaar;Beheerder;Categorie;SOORT_KORT;SDVIEW;RADIUS;WKT_LNG_LAT;WKT_LAT_LNG;LNG;LAT;
    }

    private void TraverseTileFiles()
	{
        var info = new DirectoryInfo("C:/Users/Sam/Desktop/1x1kmTiles");
        var fileInfo = info.GetFiles();
        foreach (var file in fileInfo)
        {
            Debug.Log(file.Name);
            if(!file.Name.Contains(".manifest")){
                   
			}
        }
    }
}
