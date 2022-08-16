using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;


using UnityEngine;
using Netherlands3D;
/// <summary>
/// Convert coordinates between Unity, WGS84 and RD(EPSG7415)
/// <!-- accuracy: WGS84 to RD  X <0.01m, Y <0.02m H <0.03m, tested in Amsterdam with PCNapTrans-->
/// <!-- accuracy: RD to WGS84  X <0.01m, Y <0.02m H <0.03m, tested in Amsterdam with PCNapTrans-->
/// </summary>
namespace ConvertCoordinates
{
    /// <summary>
    /// Vector2 width Double values to represent RD-coordinates (X,Y)
    /// </summary>
    [System.Serializable]
    public struct Vector2RD
    {
        public double x;
        public double y;
        
        public Vector2RD(double X, double Y)
        {
            x = X;
            y = Y;
        }

        public bool IsInThousands 
        {
            get 
            {
                Debug.Log($"x:{x} y:{y}");
                return x % 1000 == 0 && y % 1000 == 0;
            }
        }
        
    }

    /// <summary>
    /// Vector3 width Double values to represent RD-coordinates (X,Y,H)
    /// </summary>
    [System.Serializable]
    public struct Vector3RD
    {
        public double x;
        public double y;
        public double z;
        public Vector3RD(double X, double Y, double Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public override string ToString()
        {
            return $"x:{x} y:{y} z:{z}";
        }
    }

    /// <summary>
    /// Vector3 width Double values to represent WGS84-coordinates (Lon,Lat,H)
    /// </summary>
    public struct Vector3WGS
    {
        public double lat;
        public double lon;
        public double h;
        public Vector3WGS(double Lon, double Lat, double H)
        {
            lat = Lat;
            lon = Lon;
            h = H;
        }
    }

    public static class CoordConvert
    {

        private static byte[] RDCorrectionX = Resources.Load<TextAsset>("x2c").bytes;
        private static byte[] RDCorrectionY = Resources.Load<TextAsset>("y2c").bytes;
        private static byte[] RDCorrectionZ = Resources.Load<TextAsset>("nlgeo04").bytes;

        private static Vector3RD output = new Vector3RD();

        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinaat">Vector2 WGS-coordinate</param>
        /// <returns>Vector3 Unity-Coordinate (y=0)</returns>
        public static Vector3 WGS84toUnity(Vector2 coordinaat)
        {
            Vector3 output = new Vector3();
            output = WGS84toUnity(coordinaat.x, coordinaat.y);
           
            return output;
        }
        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinaat">Vector3 WGS-coordinate</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        public static Vector3 WGS84toUnity(Vector3 coordinaat)
        {
            Vector3 output = WGS84toUnity(coordinaat.x, coordinaat.y);
            double hoogteCorrectie = RDCorrection(coordinaat.x, coordinaat.y, "Z", RDCorrectionZ);
            output.y = (float)(coordinaat.z - hoogteCorrectie);
            return output;
        }
        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinaat">Vector3RD WGS-coordinate</param>
        /// <returns>Vector Unity-Coordinate</returns>
        public static Vector3 WGS84toUnity(Vector3WGS coordinaat)
        {
            Vector3 output = WGS84toUnity(coordinaat.lon, coordinaat.lat);
            double hoogteCorrectie = RDCorrection(coordinaat.lon, coordinaat.lat, "Z", RDCorrectionZ);
            output.y = (float)( coordinaat.h - hoogteCorrectie);
            return output;
        }
        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="lon">double lon (east-west)</param>
        /// <param name="lat">double lat (south-north)</param>
        /// <returns>Vector3 Unity-Coordinate at 0-NAP</returns>
        public static Vector3 WGS84toUnity(double lon, double lat)
        {
            Vector3 output = new Vector3();
            if (WGS84IsValid(new Vector3WGS(lon,lat,0)) == false)
            {
                Debug.Log("<color=red>coordinate " + lon + "," + lat + " is not a valid WGS84-coordinate!</color>");
                return output;
            }
            Vector3RD vectorRD = new Vector3RD();
            vectorRD = WGS84toRD(lon, lat);
            vectorRD.z = Config.activeConfiguration.zeroGroundLevelY;
            output = RDtoUnity(vectorRD);
            return output;
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinaat">RD-coordinate</param>
        /// <returns>UnityCoordinate</returns>
        public static Vector3 RDtoUnity(Vector3 coordinaat)
        {
            return RDtoUnity(coordinaat.x, coordinaat.y, coordinaat.z);
        }
        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinaat">Vector3RD RD-Coordinate XYH</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        public static Vector3 RDtoUnity(Vector3RD coordinaat)
        {
            return RDtoUnity(coordinaat.x, coordinaat.y, coordinaat.z);
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinaat">Vector3RD RD-Coordinate XYH</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        public static Vector3 RDtoUnity(Vector2RD coordinaat)
        {
            return RDtoUnity(coordinaat.x, coordinaat.y, 0);
        }


        /// <summary>
        /// Convert RD-coordinate to Unity-coordinate
        /// </summary>
        /// <param name="coordinaat">RD-coordinate XYH</param>
        /// <returns>Unity-Coordinate</returns>
        public static Vector3 RDtoUnity(Vector2 coordinaat)
        {
            Vector3 output = new Vector3();
            //convert to unity
            output = RDtoUnity(coordinaat.x, coordinaat.y,0);
            return output;
        }
        /// <summary>
        /// Convert RD-coordinate to Unity-coordinate
        /// </summary>
        /// <param name="x">RD X-coordinate</param>
        /// <param name="y">RD Y-coordinate</param>
        /// <param name="y">RD eleveation</param>
        /// <returns>Unity-Coordinate</returns>
        private static Vector3 RDtoUnity(double X, double Y, double Z)
        {
            Vector3 output = new Vector3();
            output.x = (float)( X - Config.activeConfiguration.RelativeCenterRD.x);
            output.y = (float)(Z + Config.activeConfiguration.zeroGroundLevelY);
            output.z = (float)(Y - Config.activeConfiguration.RelativeCenterRD.y);
            return output;
        }

        /// <summary>
        /// Converts Unity-Coordinate to WGS84-Coordinate 
        /// </summary>
        /// <param name="coordinaat">Unity-coordinate XHZ</param>
        /// <returns>WGS-coordinate</returns>
        public static Vector3WGS UnitytoWGS84(Vector3 coordinaat)
        {
            Vector3RD vectorRD = UnitytoRD(coordinaat);
            Vector3WGS output = RDtoWGS84(vectorRD.x,vectorRD.y);
            double hoogteCorrectie = RDCorrection(output.lon, output.lat, "Z", RDCorrectionZ);
            output.h = vectorRD.z + hoogteCorrectie;
            return output;
        }
        /// <summary>
        /// Converts Unity-Coordinate to RD-coordinate
        /// </summary>
        /// <param name="coordinaat">Unity-Coordinate</param>
        /// <returns>RD-coordinate</returns>
        public static Vector3RD UnitytoRD(Vector3 coordinaat)
        {
            //Vector3WGS wgs = UnitytoWGS84(coordinaat);
            Vector3RD RD = new Vector3RD();
            RD.x = coordinaat.x + Config.activeConfiguration.RelativeCenterRD.x;
            RD.y = coordinaat.z + Config.activeConfiguration.RelativeCenterRD.y;
            RD.z = coordinaat.y - Config.activeConfiguration.zeroGroundLevelY;
            return RD;
        }


        /// <summary>
        /// Converts RD-coordinate to WGS84-cordinate using the "benaderingsformules" from http://home.solcon.nl/pvanmanen/Download/Transformatieformules.pdf
        /// and X, Y, and Z correctiongrids
        /// </summary>
        /// <param name="x">RD-coordinate X</param>
        /// <param name="y">RD-coordinate Y</param>
        /// <returns>WGS84-coordinate</returns>
        /// 
        //setup coefficients for lattitude-calculation
        private static double[] Kp = new double[] { 0, 2, 0, 2, 0, 2, 1, 4, 2, 4, 1 };
        private static double[] Kq = new double[] { 1, 0, 2, 1, 3, 2, 0, 0, 3, 1, 1 };
        private static double[] Kpq = new double[] { 3235.65389, -32.58297, -0.24750, -0.84978, -0.06550, -0.01709, -0.00738, 0.00530, -0.00039, 0.00033, -0.00012 };
        //setup coefficients for longitude-calculation
        private static double[] Lp = new double[] { 1, 1, 1, 3, 1, 3, 0, 3, 1, 0, 2, 5 };
        private static double[] Lq = new double[] { 0, 1, 2, 0, 3, 1, 1, 2, 4, 2, 0, 0 };
        private static double[] Lpq = new double[] { 5260.52916, 105.94684, 2.45656, -0.81885, 0.05594, -.05607, 0.01199, -0.00256, 0.00128, 0.00022, -0.00022, 0.00026 };
        public static Vector3WGS RDtoWGS84(double x, double y)
        {
            //coordinates of basepoint in RD
            double refRDX = 155000;
            double refRDY = 463000;

            //coordinates of basepoint in WGS84
            double refLon = 5.38720621;
            double refLat = 52.15517440;

            double correctionX = RDCorrection(x,y,"X",RDCorrectionX);
            double correctionY = RDCorrection(x, y, "Y", RDCorrectionY);

            double DeltaX = (x+correctionX - refRDX) * Math.Pow(10, -5);
            double DeltaY = (y+correctionY - refRDY) * Math.Pow(10, -5);

            

            //calculate lattitude
            double Deltalat = 0;
            for (int i = 0; i < Kpq.Length; i++)
            {
                Deltalat += Kpq[i] * Math.Pow(DeltaX, Kp[i]) * Math.Pow(DeltaY, Kq[i]);
            }
            Deltalat = Deltalat / 3600;
            double lat = Deltalat + refLat;

            //calculate longitude
            double Deltalon = 0;
            for (int i = 0; i < Lpq.Length; i++)
            {
                Deltalon += Lpq[i] * Math.Pow(DeltaX, Lp[i]) * Math.Pow(DeltaY, Lq[i]);
            }
            Deltalon = Deltalon / 3600;
            double lon = Deltalon + refLon;

            //output result
            Vector3WGS output = new Vector3WGS();
            output.lon = lon;
            output.lat = lat;
            return output;
        }
        /// <summary>
        /// Converts WGS84-coordinate to RD-coordinate using the "benaderingsformules" from http://home.solcon.nl/pvanmanen/Download/Transformatieformules.pdf
        /// and X, Y, and Z correctiongrids
        /// </summary>
        /// <param name="lon">Longitude (East-West)</param>
        /// <param name="lat">Lattitude (South-North)</param>
        /// <returns>RD-coordinate xyH</returns>
        /// 
        //setup coefficients for X-calculation
        private static double[] Rp = new double[] { 0, 1, 2, 0, 1, 3, 1, 0, 2 };
        private static double[] Rq = new double[] { 1, 1, 1, 3, 0, 1, 3, 2, 3 };
        private static double[] Rpq = new double[] { 190094.945, -11832.228, -114.221, -32.391, -0.705, -2.340, -0.608, -0.008, 0.148 };
        //setup coefficients for Y-calculation
        private static double[] Sp = new double[] { 1, 0, 2, 1, 3, 0, 2, 1, 0, 1 };
        private static double[] Sq = new double[] { 0, 2, 0, 2, 0, 1, 2, 1, 4, 4 };
        private static double[] Spq = new double[] { 309056.544, 3638.893, 73.077, -157.984, 59.788, 0.433, -6.439, -0.032, 0.092, -0.054 };

        public static Vector3RD WGS84toRD(double lon, double lat)
        {
            //coordinates of basepoint in RD
            double refRDX = 155000;
            double refRDY = 463000;

            //coordinates of basepoint in WGS84
            double refLon = 5.38720621;
            double refLat = 52.15517440;

            double DeltaLon = 0.36 * (lon - refLon);
            double DeltaLat = 0.36 * (lat - refLat);

            //calculate X
            double DeltaX = 0;
            for (int i = 0; i < Rpq.Length; i++)
            {
                DeltaX += Rpq[i] * Math.Pow(DeltaLat, Rp[i]) * Math.Pow(DeltaLon, Rq[i]);
            }
            double X = DeltaX + refRDX;

            //calculate Y
            double DeltaY = 0;
            for (int i = 0; i < Spq.Length; i++)
            {
                DeltaY += Spq[i] * Math.Pow(DeltaLat, Sp[i]) * Math.Pow(DeltaLon, Sq[i]);
            }
            double Y = DeltaY + refRDY;

            double correctionX = RDCorrection(X, Y, "X",RDCorrectionX);
            double correctionY = RDCorrection(X,Y, "Y", RDCorrectionY);
            X -= correctionX;
            Y -= correctionY;

            //output result
            output.x = (float)X;
            output.y = (float)Y;
            output.z = 0;
            return output;
        }


        /// <summary>
        /// checks if RD-coordinate is within the defined valid region
        /// </summary>
        /// <param name="coordinaat">RD-coordinate</param>
        /// <returns>true if coordinate is valid</returns>
        public static bool RDIsValid(Vector3RD coordinaat)
        {
            if (coordinaat.x > -7000 && coordinaat.x < 300000)
            {
                if (coordinaat.y > 289000 && coordinaat.y < 629000)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// checks if WGS-coordinate is valid
        /// </summary>
        /// <param name="coordinaat">Vector3 WGS84-coordinate</param>
        /// <returns>True if coordinate is valid</returns>
        public static bool WGS84IsValid(Vector3WGS coordinaat)
        {
            bool isValid = true;
            if (coordinaat.lon < -180) { isValid = false; }
            if (coordinaat.lon > 180) { isValid = false; }
            if (coordinaat.lat < -90) { isValid = false; }
            if (coordinaat.lat > 90) { isValid = false; }
            return isValid;
        }

        /// <summary>
        /// correction for RD-coordinatesystem
        /// </summary>
        /// <param name="x">X-value of coordinate when richting is X or Y, else longitude</param>
        /// <param name="y">Y-value of coordinate when richting is X or Y, else lattitude</param>
        /// <param name="richting">X, Y, or Z</param>
        /// <returns>correction for RD X and Y or Elevationdifference between WGS84  and RD</returns>
        public static Double RDCorrection(double x, double y, string richting, byte[] bytes)
        {
            double waarde = 0;
            //TextAsset txt;

            if (richting == "X")
            {
                //txt = RDCorrectionX;
                waarde = -0.185;    
            }
            else if (richting == "Y")
            {
                //txt = RDCorrectionY;
                waarde = -0.232;
            }
            else
            {
                //DeltaH tussen wGS en NAP
                //txt = RDCorrectionZ;
            }

            
            //byte[] bytes = txt.bytes;

            double Xmin;
            double Xmax;
            double Ymin;
            double Ymax;
            int sizeX;
            int sizeY;

            int datanummer;
            sizeX = BitConverter.ToInt16(bytes, 4);
            sizeY = BitConverter.ToInt16(bytes, 6);
            Xmin = BitConverter.ToDouble(bytes, 8);
            Xmax = BitConverter.ToDouble(bytes, 16);
            Ymin = BitConverter.ToDouble(bytes, 24);
            Ymax = BitConverter.ToDouble(bytes, 32);

            double kolombreedte = (Xmax - Xmin) / sizeX;
            double locatieX = Math.Floor((x - Xmin) / kolombreedte);
            double rijhoogte = (Ymax - Ymin) / sizeY;
            double locatieY = (long)Math.Floor((y - Ymin) / rijhoogte);

            if (locatieX < Xmin || locatieX > Xmax)
            {
                return waarde;
            }
            if (locatieY < Ymin || locatieY > Ymax)
            {
                return waarde;
            }

            datanummer = (int)(locatieY * sizeX + locatieX);

            // do linear interpolation on the grid
            if (locatieX < sizeX && locatieY < sizeY)
            {
                float linksonder = BitConverter.ToSingle(bytes, 56 + (datanummer * 4));
                float rechtsonder = BitConverter.ToSingle(bytes, 56 + ((datanummer+1) * 4));
                float linksboven = BitConverter.ToSingle(bytes, 56 + ((datanummer+ sizeX) * 4));
                float rechtsboven = BitConverter.ToSingle(bytes, 56 + ((datanummer + sizeX+1) * 4));

                double Yafstand = ((y - Ymin) % rijhoogte)/rijhoogte;
                double YgewogenLinks = ((linksboven-linksonder)*Yafstand)+linksonder;
                double YgewogenRechts = ((rechtsboven - rechtsonder) * Yafstand)+rechtsonder;

                double Xafstand = ((x - Xmin) % kolombreedte)/kolombreedte;
                waarde += ((YgewogenRechts - YgewogenLinks) * Xafstand) + YgewogenLinks;
            }
            else
            {
                
                float myFloat = System.BitConverter.ToSingle(bytes, 56 + (datanummer * 4));
                waarde += myFloat;
            }
            //datanummer = 1500;
            
            
            
            
            return waarde;
        }

    }
}
