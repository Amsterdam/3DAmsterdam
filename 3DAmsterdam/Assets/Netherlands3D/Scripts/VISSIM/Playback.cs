using Netherlands3D.Cameras;
using Netherlands3D.TileSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic.VISSIM
{
    public class Playback : MonoBehaviour
    {
        public Dictionary<int, VissimVehicle> vehicles = new Dictionary<int, VissimVehicle>();
        [SerializeField] private ConvertFZP fileConverter = default;
        [SerializeField] private BinaryMeshLayer meshLayer;

        private Vector3 cameraStartPosition;
        private float timeCounter;
        private int loopCounter = 0;
        private int loopCounterFuture = 0;

        // Start is called before the first frame update
        void Start()
        {
            timeCounter = 0.0f;
        }

        // Update is called once per frame
        void Update()
        {
            if (fileConverter.finishedLoadingData)
            {
                SendCommand(fileConverter.allVissimData);
            }
            else
            {
                loopCounter = 0;
                loopCounterFuture = 0;
            }
        }
        /// <summary>
        /// Checks the current simulation time and sends according commands to all vehicles.
        /// </summary>
        /// <param name="dataList"></param>
        public void SendCommand(List<VissimData> dataList)
        {
            if (Time.time > timeCounter)
            {
                timeCounter = Time.time + fileConverter.timeBetweenFrames; // runs the simulation at the imported simspeed
                fileConverter.frameCounter++;

                for (int i = loopCounter; i < dataList.Count; i++)
                {
                    if (fileConverter.frameCounter != dataList[i].simsec)
                    {
                        loopCounter = i;
                        loopCounterFuture = i;
                        break;
                    }
                    else
                    {
                        if (vehicles.ContainsKey(dataList[i].id))
                        {
                            // send vehicle command
                            vehicles[dataList[i].id].vehicleCommandData = dataList[i];
                        }
                        else
                        {
                            // change 0 with i when you have more models
                            GameObject tempObject = Instantiate(fileConverter.vehicleTypes[dataList[i].vehicleType][Random.Range(0, fileConverter.vehicleTypes[dataList[i].vehicleType].Length)], transform.position, new Quaternion(0f, 0f, 0f, 0f));
                            //tempObject.transform.SetParent(this.transform); This used to cause the vehicles to spawn where the button was.

                            VissimVehicle carInstance = tempObject.GetComponent<VissimVehicle>();
                            carInstance.SetMeshLayer(meshLayer);
                            vehicles.Add(dataList[i].id, carInstance);
                            vehicles[dataList[i].id].vehicleCommandData = dataList[i];
                        }
                    }
                }
                // checks the point where the car is heading to
                for (int i = loopCounterFuture; i < dataList.Count; i++)
                {
                    if (fileConverter.frameCounter + fileConverter.timeBetweenFrames != dataList[i].simsec)
                    {
                        break;
                    }
                    else
                    {
                        if (vehicles.ContainsKey(dataList[i].id))
                        {
                            // send vehicle command
                            vehicles[dataList[i].id].futurePosition = dataList[i].coordRear;
                            vehicles[dataList[i].id].MoveAnimation(vehicles[dataList[i].id].vehicleCommandData.coordRear, vehicles[dataList[i].id].futurePosition, fileConverter.timeBetweenFrames);
                        }
                    }
                }

            }


        }

        public void SendCameraToVissim()
        {
             cameraStartPosition = CameraModeChanger.Instance.ActiveCamera.gameObject.transform.position;
             CameraModeChanger.Instance.ActiveCamera.gameObject.transform.position = new Vector3(fileConverter.allVissimData[0].coordRear.x, cameraStartPosition.y ,fileConverter.allVissimData[0].coordRear.z);
        }

        public void SendCameraToStart()
        {
            CameraModeChanger.Instance.ActiveCamera.gameObject.transform.position = cameraStartPosition;
        }
    }
}