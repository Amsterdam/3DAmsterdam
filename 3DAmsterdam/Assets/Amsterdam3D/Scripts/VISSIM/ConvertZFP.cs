using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ConvertCoordinates;

public class ConvertZFP : MonoBehaviour
{
    /* FOR THIS VISSIM SIMULATION, USE THE STANDARD TEMPLATE WITH THE FOLLOWING PARAMETERS ONLY.
     * 
     *  $VEHICLE:SIMSEC;NO;VEHTYPE;COORDFRONT;COORDREAR;WIDTH
     * 
     */

    [SerializeField] private GameObject[] vissimCarPrefab = default;

    public Dictionary<int, GameObject[]> vehicleTypes = new Dictionary<int, GameObject[]>() 
    { 
        //{ 100, "Auto" }, 
        //{ 200, "Vrachtwagen" }, 
        //{ 300, "Bus" }, 
        //{ 400, "Tram" }, 
        //{ 500, "Voetganger" }, 
        //{ 600, "Fiets" }, 
        //{ 700, "Bestelbus" } 
    };
    
    private string requiredTemplate = "$VEHICLE:SIMSEC;NO;VEHTYPE;COORDFRONT;COORDREAR;WIDTH";
    public string fileLocationVISSIM = "921929autoluw2030ref005.fzp";

    public List<VissimData> allVissimData = new List<VissimData>();

    private bool readyToConvert = false;

    public bool finishedLoadingData = false;
    public float frameCounter = 0.0f;
    public float timeBetweenFrames = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RetrieveVissim(Constants.BASE_DATA_URL + "traffic/" +  fileLocationVISSIM));
        vehicleTypes.Add(100, vissimCarPrefab);
    }

    /// <summary>
    /// Reads the vissim file and converts the lines into individual lines.
    /// </summary>
    /// <param name="text"></param>
    public void ReadFileFZP(string text)
    {
        finishedLoadingData = false;
        string[] lines = text.Split(System.Environment.NewLine.ToCharArray());
        foreach (string line in lines)
        {
            // do something with each line
            if (readyToConvert && line != "")
            {
                AddVissimData(line);
            }
            if (line == requiredTemplate) 
            {
                readyToConvert = true;
            }
        }
        readyToConvert = false;
        finishedLoadingData = true;

        //sets  the current VISSIm file start parameters
        //timeBetweenFrames = allVissimData[1].simsec - allVissimData[0].simsec; // This calculates the resoluation, since its always constant we use the 2nd one and the 1st and find the difference.
        frameCounter = allVissimData[0].simsec - timeBetweenFrames; // Some simulations start at a different simsec depending on the population of the simulation. This makes sure that it will always start at the 1st frame
    }
    /// <summary>
    /// Converts string into VissimData.
    /// </summary>
    /// <param name="dataString"></param>
    public void AddVissimData(string dataString)
    {
        string[] arr = dataString.Split(';');
        float simsec = float.Parse(arr[0].Replace(".", ","));
        VissimData data = new VissimData(simsec, int.Parse(arr[1]), int.Parse(arr[2]), convertStringToVector(arr[3]), convertStringToVector(arr[4]), float.Parse(arr[5]));
        allVissimData.Add(data);
    }

    /// <summary>
    /// Converts the VISSIM RD coordinate string into a Vector3.
    /// </summary>
    /// <param name="stringVector"></param>
    /// <returns></returns>
    /// 
    public Vector3 convertStringToVector(string stringVector)
    {
        //0 value is X
        //1 value is Y
        //2 value is Z
        stringVector = stringVector.Replace(".", ","); // Transforms decimal from US standard which uses a Period to European with a Comma

        string[] splitString = stringVector.Split(' '); // Splits the string into individual vectors
        double x = double.Parse(splitString[0]);
        double y = double.Parse(splitString[1]);
        double z = double.Parse(splitString[2]);
        Vector3RD rdVector = new Vector3RD(x, y, z); // Creates the Double Vector
        //ConvertCoordinates.CoordConvert.WGS84toUnity
        //ConvertCoordinates.CoordConvert.RDtoUnity
        Vector3 convertedCoordinates = ConvertCoordinates.CoordConvert.RDtoUnity(rdVector); 
        // Y Coordinates will be calculated by the vehicle to connect with the Map (Maaiveld).

        return convertedCoordinates;
    }

    /// <summary>
    /// Retrieves vissim data through a webrequest
    /// </summary>
    /// <param name="apiUrl"></param>
    /// <returns></returns>
    public IEnumerator RetrieveVissim(string apiUrl)
    {
        // send http request
        Debug.Log(apiUrl);
        var request = UnityWebRequest.Get(apiUrl);
        {
            yield return request.SendWebRequest();
            if (request.isDone && !request.isHttpError)
            {
                // catches the data
                ReadFileFZP(request.downloadHandler.text);
            }
        }
    }
}

/*

 $VEHICLE:SIMSEC;NO;VEHTYPE;COORDFRONT;COORDREAR;WIDTH
600.1;60;100;117846.354 487305.272 0.000;117851.123 487308.912 0.000;2.00
600.1;61;100;118362.231 487351.433 0.000;118368.039 487352.935 0.000;2.00
600.1;62;100;118369.410 487353.164 0.000;118375.153 487354.902 0.000;2.00
600.1;63;100;118377.358 487355.150 0.000;118382.877 487357.503 0.000;2.00


*/
