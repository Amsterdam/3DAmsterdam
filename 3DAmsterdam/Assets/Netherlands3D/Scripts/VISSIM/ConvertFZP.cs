using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.Core;
using System.Globalization;
using UnityEngine.Rendering;

namespace Netherlands3D.Traffic.VISSIM
{

    public class ConvertFZP : MonoBehaviour
    {
        /* FOR THIS VISSIM SIMULATION, USE THE STANDARD TEMPLATE WITH THE FOLLOWING PARAMETERS ONLY.
         * 
         *  $VEHICLE:SIMSEC;NO;VEHTYPE;COORDFRONT;COORDREAR;WIDTH
         * 
         */
        public List<int> missingVissimTypes = new List<int>();

        public Dictionary<int, GameObject[]> vehicleTypes = new Dictionary<int, GameObject[]>();

        private string requiredTemplate = "$VEHICLE:SIMSEC;NO;VEHTYPE;COORDFRONT;COORDREAR;WIDTH";
        public string fileLocationVISSIM = "921929autoluw2030ref005.fzp";

        public List<VissimData> allVissimData = new List<VissimData>();
        public Dictionary<int, List<VissimData>> allVissimDataByVehicleID = new Dictionary<int, List<VissimData>>(); // Vehicle Sorting test, see SortDataByCar() function
        private bool readyToConvert = false;

        public bool finishedLoadingData = false;
        public float frameCounter = 0.0f;
        public float timeBetweenFrames = 0.0f;


        [Header("VISSIM Scriptable Objects")]
        [SerializeField] private VissimType vissimCarPrefabs = default;
        [SerializeField] private VissimType vissimTruckPrefabs = default;
        [SerializeField] private VissimType vissimBusPrefabs = default;
        [SerializeField] private VissimType vissimTramPrefabs = default;
        [SerializeField] private VissimType vissimPedestrianPrefabs = default;
        [SerializeField] private VissimType vissimCyclePrefabs = default;
        [SerializeField] private VissimType vissimVanPrefabs = default;

        private VissimConfiguration vissimConfiguration = default;

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
            vissimConfiguration = GetComponent<VissimConfiguration>();

            // Based on the VISSIM Object Template
            LoadDefaultVissimData();
        }

        /// <summary>
        /// Reads the vissim file and converts the lines into individual lines.
        /// </summary>
        /// <param name="text"></param>
        public void ReadFileFZP(string text)
        {
            finishedLoadingData = false;
            string[] lines = text.Split((System.Environment.NewLine + "\n" + "\r").ToCharArray());
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

            // automatically calculates the time between the frames.
            foreach (VissimData data in allVissimData)
            {
                if (data.simsec != allVissimData[0].simsec)
                {
                    timeBetweenFrames = data.simsec - allVissimData[0].simsec;
                    break; // after calculating the correct framerate of the simulation, exit the loop.
                }
            }

            // checks if there are missing Vissim types
            if (missingVissimTypes.Count > 0)
            {
                vissimConfiguration.OpenInterface(missingVissimTypes); // opens missing visism interface
            }
            else
            {
                StartVissim(); // starts animation
            }

            //sets  the current VISSIM file start parameters
            frameCounter = allVissimData[0].simsec - timeBetweenFrames; // Some simulations start at a different simsec depending on the population of the simulation. This makes sure that it will always start at the 1st frame

        }


        /// <summary>
        /// Converts string into VissimData.
        /// </summary>
        /// <param name="dataString"></param>
        public void AddVissimData(string dataString)
        {
            string[] arr = dataString.Split(';');
            float simsec = float.Parse(arr[0], CultureInfo.InvariantCulture);
            int vissimTypeID = int.Parse(arr[2]);

            if (!vehicleTypes.ContainsKey(vissimTypeID) && !missingVissimTypes.Contains(vissimTypeID))
            {
                missingVissimTypes.Add(vissimTypeID);
            }

            VissimData data = new VissimData(simsec, int.Parse(arr[1]), vissimTypeID, convertStringToVector(arr[3]), convertStringToVector(arr[4]), float.Parse(arr[5]));
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
            //stringVector = stringVector.Replace(".", ","); // Transforms decimal from US standard which uses a Period to European with a Comma

            string[] splitString = stringVector.Split(' '); // Splits the string into individual vectors
            double x = double.Parse(splitString[0], CultureInfo.InvariantCulture);
            double y = double.Parse(splitString[1], CultureInfo.InvariantCulture);
            double z = double.Parse(splitString[2], CultureInfo.InvariantCulture);
            Vector3RD rdVector = new Vector3RD(x, y, z); // Creates the Double Vector
            Vector3 convertedCoordinates = CoordConvert.RDtoUnity(rdVector);
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
            var request = UnityWebRequest.Get(apiUrl);
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError)
                {
                    // fetches the data
                    ReadFileFZP(request.downloadHandler.text);
                }
            }
        }
        /// <summary>
        /// Resets Vissim Data and replaces it with the default data
        /// </summary>
        public void LoadDefaultVissimData()
        {
            missingVissimTypes.Clear(); // removes all missing vehicles
            vehicleTypes.Clear(); // removes all assigned vehicles

            // Loads in all default vehicles Based on the VISSIM Object Template
            vehicleTypes.Add(100, vissimCarPrefabs.vissimTypeAssets); // Car
            vehicleTypes.Add(200, vissimTruckPrefabs.vissimTypeAssets); // Truck
            vehicleTypes.Add(300, vissimBusPrefabs.vissimTypeAssets); // Bus
            vehicleTypes.Add(400, vissimTramPrefabs.vissimTypeAssets); // Tram
            vehicleTypes.Add(500, vissimPedestrianPrefabs.vissimTypeAssets); // Pedestrian
            vehicleTypes.Add(600, vissimCyclePrefabs.vissimTypeAssets); // Cycle
            vehicleTypes.Add(700, vissimVanPrefabs.vissimTypeAssets); // Van
        }

        /// <summary>
        /// Starts the vissim simulation
        /// </summary>
        public void StartVissim()
        {
            finishedLoadingData = true;
        }
    }
}