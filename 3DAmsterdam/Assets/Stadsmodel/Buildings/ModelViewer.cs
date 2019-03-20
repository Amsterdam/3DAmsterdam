using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;


public class ModelViewer : MonoBehaviour
{
    public string ModelURL = "https://acc.3d.amsterdam.nl/webmap/gebouwen/models/{id}";
    private List<modeldata> CustomModels = new List<modeldata>();
    private int volgnummer = 0;
    private bool downloadgereed = true;

    public struct modeldata
    {
        public string modelnaam;
        public double lon;
        public double lat;
        public string BAGid;
        public double NAPHoogte;
    }

    // Start is called before the first frame update
    void Start()
    {
        /// lijst met custom modellen inlezen en opslaan in de list CustomModels
        TextAsset modellijst = new TextAsset();
        modellijst = Resources.Load<TextAsset>("3Dmodellen/gebouwencoordinatenlijst");
        string tekst = modellijst.text;
        string[] linesInFile = tekst.Split('\n');
        for (int i = 3; i < linesInFile.Length; i++)
        {
            string[] regeldelen = linesInFile[i].Split(';');
            modeldata gegevens = new modeldata();
            gegevens.modelnaam = regeldelen[0];
            double dbl;
            double.TryParse(regeldelen[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
            gegevens.lat = dbl;
            double.TryParse(regeldelen[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
            gegevens.lon = dbl;
            double.TryParse(regeldelen[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
            gegevens.NAPHoogte = dbl;
            gegevens.BAGid = regeldelen[4];
            CustomModels.Add(gegevens);
            
        }
    }

    private void Update()
    {
        if (volgnummer< CustomModels.Count && downloadgereed)
        {
            downloadgereed = false;
            volgnummer++;
            LoadModel(CustomModels[volgnummer]);
        }
    }
    // Update is called once per frame
    bool LoadModel(modeldata m)
    {
        
        bool isaanwezig = false;
 

 
        return isaanwezig;
    }
    
}
