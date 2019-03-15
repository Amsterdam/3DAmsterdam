using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleJSON;


public class RenameFiles : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string folderpath = "D://geojson/GeoJSONBAG/";
        string[] files = Directory.GetFiles(folderpath, "*.geojson", SearchOption.TopDirectoryOnly);
        //Debug.Log(files.Length);
        foreach (string file in files)
        {
            string tekst = System.IO.File.ReadAllText(file);
            JSONNode N = JSON.Parse(tekst);
            //string X_waarde = N["features"][0]["properties"]["left"].Value;
            //string Y_waarde = N["features"][0]["properties"]["bottom"].Value;
            if (N["features"].Count==0)
            {
                continue;
            }

            string polytype = N["features"][0]["geometry"]["type"].Value;
            JSONNode grondvlak;
            if (polytype == "MultiPolygon")
            {
                grondvlak = N["features"][0]["geometry"]["coordinates"][0][0];
            }
            else
            {
                grondvlak = N["features"][0]["geometry"]["coordinates"][0];
            }
            double dbl;

            double dX=0;
            double dY=0;
            for (int i = 0; i < grondvlak.Count; i++)
                
            {
                tekst = grondvlak[i][0].Value;
                if (tekst.Length>8)
                {
                    tekst = tekst.Replace(".", ",");
                    dX = double.Parse(tekst);
                }
                

                tekst = grondvlak[i][1].Value;
                if (tekst.Length > 8)
                {
                    tekst = tekst.Replace(".", ",");
                    dY = double.Parse(tekst);
                }

            }

            dbl = Mathf.Floor((float)dX / 500) * 500;
            int X = (int)dbl;
            string X_waarde = X.ToString();

            dbl = Mathf.Floor((float)dY / 500) * 500;
            int Y = (int)dbl;
            string Y_waarde = Y.ToString();

            // zoeken op eerste cordinaat en dan afronden, left en bottom zijn iet altijd correct

            string NieuweFileName = folderpath +"Hernoemd/"+ X_waarde + "_" + Y_waarde + ".geojson";
            //Debug.Log(NieuweFileName);
            try
            {
                System.IO.File.Move(file, NieuweFileName);
            }
            catch (System.Exception)
            {
                Debug.Log("mislukt: "+ file + " opslaan als " + NieuweFileName);
                
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
