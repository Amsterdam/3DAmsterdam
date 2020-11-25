using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ConvertCoordinates;

public class VissimManager : MonoBehaviour
{
    /* FOR THIS VISSIM SIMULATION, USE THE STANDARD TEMPLATE WITH THE FOLLOWING PARAMETERS ONLY.
     * 
     *  $VEHICLE:SIMSEC;NO;VEHTYPE;COORDFRONT;COORDREAR;WIDTH
     * 
     */

    public Dictionary<int, Car> vehicles = new Dictionary<int, Car>();

    public Dictionary<int, string> vehicleTypes = new Dictionary<int, string>() 
    { 
        { 100, "Auto" }, 
        { 200, "Vrachtwagen" }, 
        { 300, "Bus" }, 
        { 400, "Tram" }, 
        { 500, "Voetganger" }, 
        { 600, "Fiets" }, 
        { 700, "Bestelbus" } 
    };

    public List<VissimData> allData = new List<VissimData>();

    [HideInInspector] //[TextArea]
    public string textas;
    public string fileLocationVISSIM = "921-929-autoluw-2030-ref+_005.fzp";

    private string requiredTemplate = "$VEHICLE:SIMSEC;NO;VEHTYPE;COORDFRONT;COORDREAR;WIDTH";
    private bool readyToConvert = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RetrieveVissim(Constants.BASE_DATA_URL + "traffic/" +  fileLocationVISSIM));
    }

    /// <summary>
    /// Reads the vissim file and converts the lines into individual lines.
    /// </summary>
    /// <param name="text"></param>
    public void ReadFileFZP(string text)
    {
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
    }
    /// <summary>
    /// Converts string into VissimData.
    /// </summary>
    /// <param name="dataString"></param>
    public void AddVissimData(string dataString)
    {
        Debug.Log(dataString);
        string[] arr = dataString.Split(';');
        Debug.Log(arr[1]);
        VissimData data = new VissimData(float.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), convertStringToVector(arr[3]), convertStringToVector(arr[4]), float.Parse(arr[5]));
        allData.Add(data);
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
            Debug.Log("SEND");
            if (request.isDone && !request.isHttpError)
            {
                // catches the data
                textas = request.downloadHandler.text;
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
