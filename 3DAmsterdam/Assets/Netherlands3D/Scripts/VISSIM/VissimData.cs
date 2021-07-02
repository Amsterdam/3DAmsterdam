using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic.VISSIM
{
    [System.Serializable]
    public class VissimData
    {
        public float simsec;
        public int id;
        public int vehicleType;
        public Vector3 coordFront;
        public Vector3 coordRear;
        float width;

        public VissimData(float dataSimsec, int dataId, int dataVehicleType, Vector3 dataCoordFront, Vector3 dataCoordRear, float dataWidth)
        {
            simsec = dataSimsec;
            id = dataId;
            vehicleType = dataVehicleType;
            coordFront = dataCoordFront;
            coordRear = dataCoordRear;
            width = dataWidth;
        }
    }
}