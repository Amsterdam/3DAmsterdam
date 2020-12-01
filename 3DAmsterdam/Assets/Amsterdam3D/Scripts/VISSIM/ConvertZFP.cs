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
    public Dictionary<int, GameObject[]> vehicleTypes = new Dictionary<int, GameObject[]>();

    private string requiredTemplate = "$VEHICLE:SIMSEC;NO;VEHTYPE;COORDFRONT;COORDREAR;WIDTH";
    public string fileLocationVISSIM = "921929autoluw2030ref005.fzp";

    public List<VissimData> allVissimData = new List<VissimData>();
    public Dictionary<int, List<VissimData>> allVissimDataByVehicleID = new Dictionary<int, List<VissimData>>(); // Vehicle Sorting test, see SortDataByCar() function

    private bool readyToConvert = false;

    public bool finishedLoadingData = false;
    public float frameCounter = 0.0f;
    public float timeBetweenFrames = 0.0f;

    [Header("VISSIM Object Prefabs")]
    [SerializeField] private GameObject[] vissimCarPrefabs = default;
    [SerializeField] private GameObject[] vissimTruckPrefabs = default;
    [SerializeField] private GameObject[] vissimBusPrefabs = default;
    [SerializeField] private GameObject[] vissimTramPrefabs = default;
    [SerializeField] private GameObject[] vissimPedestrianPrefabs = default;
    [SerializeField] private GameObject[] vissimCyclePrefabs = default;
    [SerializeField] private GameObject[] vissimVanPrefabs = default;

    /* VISSIM OBJECT TYPE TEMPLATE
     * 
     * 100 = Car
     * 200 = Truck
     * 300 = Bus
     * 400 = Tram
     * 500 = Pedestrian
     * 600 = Cycle
     * 700 = Van
     * 
     */
    void Start()
    {
        // Based on the VISSIM Object Template
        vehicleTypes.Add(100, vissimCarPrefabs); // Car
        vehicleTypes.Add(200, vissimTruckPrefabs); // Truck
        vehicleTypes.Add(300, vissimBusPrefabs); // Bus
        vehicleTypes.Add(400, vissimTramPrefabs); // Tram
        vehicleTypes.Add(500, vissimPedestrianPrefabs); // Pedestrian
        vehicleTypes.Add(600, vissimCyclePrefabs); // Cycle
        vehicleTypes.Add(700, vissimVanPrefabs); // Van
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            // Fetches Vissim data from the server (WILL BE REPLACED BY UPLOAD FEATURE)
            StartCoroutine(RetrieveVissim(Constants.BASE_DATA_URL + "traffic/" +  fileLocationVISSIM));
        }
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

        //sets  the current VISSIM file start parameters
        frameCounter = allVissimData[0].simsec - timeBetweenFrames; // Some simulations start at a different simsec depending on the population of the simulation. This makes sure that it will always start at the 1st frame
        
        /* // continue with this after the code testing
        foreach(VissimData data in allVissimDataByVehicleID[244])
        {
            Debug.Log(data.simsec); // HIER MEE KAN JE PER VOERTUIG DE 
        }
        */
        
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

        SortDataByCar(data); // currently in test modes, can be removed later or kept for other functions.
    }

    /// <summary>
    /// Sorts all the VISSIM data and groups them together based on the ID of the vehicle. This results in having multiple simsec simulation data available per specific vehicle id.
    /// </summary>
    /// <param name="data"></param>
    public void SortDataByCar(VissimData data)
    {
        if (!allVissimDataByVehicleID.ContainsKey(data.id))
        {
            List<VissimData> temp = new List<VissimData>();
            temp.Add(data);
            allVissimDataByVehicleID.Add(data.id, temp);
        }
        else
        {
            allVissimDataByVehicleID[data.id].Add(data);
        }
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
                // fetches the data
                ReadFileFZP(request.downloadHandler.text);
            }
        }
    }
}
