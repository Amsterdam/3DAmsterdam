using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;


using UnityEngine;
/// <summary>
/// Convert coordinates between Unity, WGS84 and RD(EPSG7415)
/// <!-- accuracy: WGS84 to RD  X <0.01m, Y <0.02m H <0.03m, tested in Amsterdam with PCNapTrans-->
/// <!-- accuracy: RD to WGS84  X <0.01m, Y <0.02m H <0.03m, tested in Amsterdam with PCNapTrans-->
/// </summary>
namespace ConvertCoordinates
{
    /// <summary>
    /// Vector3 width Double values to represent RD-coordinates (X,Y,H)
    /// </summary>
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
        //referenceWGS84 and referenceRD are connected, both represent the coordinate in their coordinatesystem corresponding to UnityCoordinates 0,0
        //referenceWGS84 is used for conversions to and from Unity-coordinates
        //referenceRD can be used to check de desired origin, and to set referenceWGS84
        private static Vector3WGS referenceWGS84 = new Vector3WGS(4.892504, 52.373043,0);
        private static Vector3RD referenceRD = new Vector3RD(121311.223, 487355.782,-42.962);

        //scalefactors for converting WGS84 to Unity
        private static double unitsPerDegreeX = 67800;  //approximation of distance between longitudinal degrees in meters at reference-lattitude
        private static double unitsPerDegreeY = 111000; //approximation of distance between lattitudinal degrees in meters

        /// <summary>
        /// set ReferenceWGS84 to given value, 
        /// sets referenceRD to corresponding value, 
        /// sets unitsperdegreeX based on unitsperdegreeY and referenceWGS.y
        /// </summary>
        public static Vector3WGS ReferenceWGS84
        {
            get
            {
                return referenceWGS84;
            }
            set
            {
                //set value
                referenceWGS84 = value;
                // update unitsPerDegreeX
                unitsPerDegreeX = Math.Cos(referenceWGS84.lat * Math.PI / 180) * unitsPerDegreeY;
                // update referenceRD
                Vector3RD refRD = WGS84toRD(referenceWGS84.lon, referenceWGS84.lat);
                // get elevation 
                double hoogte = RDCorrection(referenceWGS84.lon,referenceWGS84.lat,"Z");
                referenceRD = new Vector3RD(refRD.x,refRD.y,-hoogte);
                
            }
        }
       
        public static Vector3RD ReferenceRD
        {
            get
            {
                return referenceRD;
            }
            set
            {
                //set value
                referenceRD = value;
                //update referenceWGS84
                Vector3WGS refWGS = RDtoWGS84(referenceRD.x, referenceRD.y);
                double hoogte = RDCorrection(refWGS.lon, refWGS.lat, "Z");
                referenceWGS84 = new Vector3WGS(refWGS.lon, refWGS.lat,hoogte);
                //update unitsperdegreeX
                unitsPerDegreeX = Math.Cos(referenceWGS84.lon * Math.PI / 180) * unitsPerDegreeY;
            }
        }

        /// <summary>
        /// get: return value
        /// set: set value, recalcalute UnitsPerDegreeY
        /// </summary>
        public static double UnitsPerDegreeX
        {
            get
            {
                return unitsPerDegreeX;
            }
            set
            {
                //set value
                unitsPerDegreeX = value;
                //update unitsPerDegreeY
                unitsPerDegreeY = unitsPerDegreeX / Math.Cos(referenceWGS84.lon * Math.PI / 180);
            }
        }
        /// <summary>
        /// get: return value
        /// set: set value, recalcalute UnitsPerDegreeX
        /// </summary>
        public static double UnitsPerDegreeY
        {
            get
            {
                return unitsPerDegreeY;
            }
            set
            {
                //set value
                unitsPerDegreeY = value;
                //update unitsPerDegreeX
                unitsPerDegreeX = Math.Cos(referenceWGS84.lon * Math.PI / 180) * unitsPerDegreeY;
            }
        }

        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinaat">Vector2 WGS-coordinate</param>
        /// <returns>Vector3 Unity-Coordinate (y=0)</returns>
        public static Vector3 WGS84toUnity(Vector2 coordinaat)
        {
            Vector3 output = new Vector3();
            output = WGS84toUnity(coordinaat.x, coordinaat.y);
            output.y = (float)(0-referenceWGS84.h);
            return output;
        }
        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinaat">Vector3 WGS-coordinate</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        public static Vector3 WGS84toUnity(Vector3 coordinaat)
        {
            Vector3 output = new Vector3();
            output = WGS84toUnity(coordinaat.x, coordinaat.y);
            output.y = (float)(coordinaat.y-referenceWGS84.h);
            return output;
        }
        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinaat">Vector3RD WGS-coordinate</param>
        /// <returns>Vector Unity-Coordinate</returns>
        public static Vector3 WGS84toUnity(Vector3WGS coordinaat)
        {
            Vector3 output = new Vector3();
            output = WGS84toUnity(coordinaat.lon, coordinaat.lat);
            output.y = (float)(coordinaat.h- referenceWGS84.h);
            return output;
        }
        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="lon">double lon (east-west)</param>
        /// <param name="lat">double lat (south-north)</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        public static Vector3 WGS84toUnity(double lon, double lat)
        {
            Vector3 output = new Vector3();
            if (WGS84IsValid(new Vector3WGS(lon,lat,0)) == false)
            {
                Debug.Log("<color=red>coordinate " + lon + "," + lat + " is not a valid WGS84-coordinate!</color>");
                return output;
            }

            output.y = (float)(0-referenceWGS84.h);
            output.x = (float)((lon - referenceWGS84.lon) * unitsPerDegreeX);
            output.z = (float)((lat - referenceWGS84.lat) * unitsPerDegreeY);
            return output;
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinaat">RD-coordinate</param>
        /// <returns>UnityCoordinate</returns>
        public static Vector3 RDtoUnity(Vector3 coordinaat)
        {
            Vector3 output = new Vector3();
            //convert to unity
            output = RDtoUnity(coordinaat.x, coordinaat.y,coordinaat.z);
            //insert elevation
            //output.y = coordinaat.y;
            return output;
        }
        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinaat">Vector3RD RD-Coordinate XYH</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        public static Vector3 RDtoUnity(Vector3RD coordinaat)
        {
            Vector3 output = new Vector3();
            //convert to unity
            output = RDtoUnity(coordinaat.x, coordinaat.y,coordinaat.z);
            //insert elevation
            output.y = (float)(coordinaat.z-referenceRD.z);
            return output;
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
            //if (RDIsValid(new Vector3((float)X, (float)Y)) == false) //check if RD-coordinate is valid
            //{
            //    Debug.Log("<color=red>coordinaat " + X + "," + X + " is geen geldig RD-coordinaat!</color>");
            //    return output;
            //}
            //convert to WGS84
            Vector3WGS wgs = RDtoWGS84(X, Y);
            //convert to Unity
            output = WGS84toUnity(wgs.lon, wgs.lat);
            double hoogte = RDCorrection(wgs.lon, wgs.lat, "Z");
            output.y = (float)(Z - referenceRD.z);
            return output;
        }

        /// <summary>
        /// Converts Unity-Coordinate to WGS84-Coordinate 
        /// </summary>
        /// <param name="coordinaat">Unity-coordinate XHZ</param>
        /// <returns>WGS-coordinate</returns>
        public static Vector3WGS UnitytoWGS84(Vector3 coordinaat)
        {
            Vector3WGS output = new Vector3WGS();
            output.h = coordinaat.y - referenceWGS84.h; ;
            output.lon = (float)((coordinaat.x / UnitsPerDegreeX) + ReferenceWGS84.lon);
            output.lat = (float)((coordinaat.z / UnitsPerDegreeY) + ReferenceWGS84.lat);
            return output;
        }
        /// <summary>
        /// Converts Unity-Coordinate to RD-coordinate
        /// </summary>
        /// <param name="coordinaat">Unity-Coordinate</param>
        /// <returns>RD-coordinate</returns>
        public static Vector3RD UnitytoRD(Vector3 coordinaat)
        {
            Vector3WGS wgs = UnitytoWGS84(coordinaat);
            Vector3RD RD = WGS84toRD(wgs.lon, wgs.lat);
            RD.z = wgs.h - RDCorrection(wgs.lon, wgs.lat, "Z");
            RD.z = RD.z - ReferenceWGS84.h;
            return RD;
        }


        /// <summary>
        /// Converts RD-coordinate to WGS84-cordinate using the "benaderingsformules" from http://home.solcon.nl/pvanmanen/Download/Transformatieformules.pdf
        /// and X, Y, and Z correctiongrids
        /// </summary>
        /// <param name="x">RD-coordinate X</param>
        /// <param name="y">RD-coordinate Y</param>
        /// <returns>WGS84-coordinate</returns>
        public static Vector3WGS RDtoWGS84(double x, double y)
        {
            //coordinates of basepoint in RD
            double refRDX = 155000;
            double refRDY = 463000;

            //coordinates of basepoint in WGS84
            double refLon = 5.38720621;
            double refLat = 52.15517440;

            double correctionX = RDCorrection(x,y,"X");
            double correctionY = RDCorrection(x, y, "Y");

            double DeltaX = (x+correctionX - refRDX) * Math.Pow(10, -5);
            double DeltaY = (y+correctionY - refRDY) * Math.Pow(10, -5);

            //setup coefficients for lattitude-calculation
            double[] Kp = new double[] { 0, 2, 0, 2, 0, 2, 1, 4, 2, 4, 1 };
            double[] Kq = new double[] { 1, 0, 2, 1, 3, 2, 0, 0, 3, 1, 1 };
            double[] Kpq = new double[] { 3235.65389, -32.58297, -0.24750, -0.84978, -0.06550, -0.01709, -0.00738, 0.00530, -0.00039, 0.00033, -0.00012 };

            //calculate lattitude
            double Deltalat = 0;
            for (int i = 0; i < Kpq.Length; i++)
            {
                Deltalat += Kpq[i] * Math.Pow(DeltaX, Kp[i]) * Math.Pow(DeltaY, Kq[i]);
            }
            Deltalat = Deltalat / 3600;
            double lat = Deltalat + refLat;

            //setup coefficients for longitude-calculation
            double[] Lp = new double[] { 1, 1, 1, 3, 1, 3, 0, 3, 1, 0, 2, 5 };
            double[] Lq = new double[] { 0, 1, 2, 0, 3, 1, 1, 2, 4, 2, 0, 0 };
            double[] Lpq = new double[] { 5260.52916, 105.94684, 2.45656, -0.81885, 0.05594, -.05607, 0.01199, -0.00256, 0.00128, 0.00022, -0.00022, 0.00026 };

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

            

            //setup coefficients for X-calculation
            double[] Rp = new double[] { 0, 1, 2, 0, 1, 3, 1, 0, 2 };
            double[] Rq = new double[] { 1, 1, 1, 3, 0, 1, 3, 2, 3 };
            double[] Rpq = new double[] { 190094.945, -11832.228, -114.221, -32.391, -0.705, -2.340, -0.608, -0.008, 0.148 };

            //calculate X
            double DeltaX = 0;
            for (int i = 0; i < Rpq.Length; i++)
            {
                DeltaX += Rpq[i] * Math.Pow(DeltaLat, Rp[i]) * Math.Pow(DeltaLon, Rq[i]);
            }
            double X = DeltaX + refRDX;

            //setup coefficients for Y-calculation
            double[] Sp = new double[] { 1, 0, 2, 1, 3, 0, 2, 1, 0, 1 };
            double[] Sq = new double[] { 0, 2, 0, 2, 0, 1, 2, 1, 4, 4 };
            double[] Spq = new double[] { 309056.544, 3638.893, 73.077, -157.984, 59.788, 0.433, -6.439, -0.032, 0.092, -0.054 };

            //calculate Y
            double DeltaY = 0;
            for (int i = 0; i < Spq.Length; i++)
            {
                DeltaY += Spq[i] * Math.Pow(DeltaLat, Sp[i]) * Math.Pow(DeltaLon, Sq[i]);
            }
            double Y = DeltaY + refRDY;

            double correctionX = RDCorrection(X, Y, "X");
            double correctionY = RDCorrection(X,Y, "Y");
            X -= correctionX;
            Y -= correctionY;

            //output result
            Vector3RD output = new Vector3RD((float)X, (float)Y,0);
            return output;
        }


        /// <summary>
        /// checks if RD-coordinate is within the defined valid region
        /// </summary>
        /// <param name="coordinaat">RD-coordinate</param>
        /// <returns>true if coordinate is valid</returns>
        public static bool RDIsValid(Vector3RD coordinaat)
        {
            bool IsValid = true;
            double[,] Zonegrens = new double[,] { { 141000, 629000 }, { 100000, 600000 }, { 80000, 500000 }, { -7000, 392000 }, { -7000, 336000 }, { 101000, 336000 }, { 161000, 289000 }, { 219000, 289000 }, { 300000, 451000 }, { 300000, 614000 }, { 259000, 629000 } };
            double[] hoeken = new double[Zonegrens.GetLength(0)];
            //calculate angles from coordinate to Zonegrens-points
            for (int i = 0; i < Zonegrens.GetLength(0); i++)
            {
                hoeken[i] = Math.Atan((Zonegrens[i, 1] - coordinaat.z) / (Zonegrens[i, 0] - coordinaat.x));
            }
            double TotalAngle = 0;
            //add angle-difference between points
            TotalAngle += hoeken[0] - hoeken[hoeken.Length - 1];
            for (int i = 1; i < hoeken.Length; i++)
            {
                TotalAngle += hoeken[i] - hoeken[i - 1];
            }
            //if TotalAngle is zero, coordinate is outside Zonegrens
            // else Totalangle is 360 degrees (2*pi) and coordinate is inside Zonegrens
            if (Math.Abs(TotalAngle) < 0.1)
            {
                IsValid = false;
            }

            return IsValid;
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
        public static Double RDCorrection(double x, double y, string richting)
        {
            double waarde = 0;
            TextAsset txt = new TextAsset();

            if (richting == "X")
            {
                txt = Resources.Load<TextAsset>("x2c");
                waarde = -0.185;    
            }
            else if (richting == "Y")
            {
                txt = Resources.Load<TextAsset>("y2c");
                waarde = -0.232;
            }
            else
            {
                //DeltaH tussen wGS en NAP
                txt = Resources.Load<TextAsset>("nlgeo04");
            }

            
            byte[] bytes = txt.bytes;

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

        /// <summary>
        /// calculate the necessary rotation for objects in rd-coordinates to sit nicely in the Unity-CoordinateSystem
        /// </summary>
        /// <param name="locatie">RD-coordinate</param>
        /// <returns>rotationAngle for RD-object (clockwise) in degrees around unity Y-axis</returns>
        public static double RDRotation(Vector3RD locatie)
        {
            double hoek = 0;

            Vector3WGS origin = RDtoWGS84(locatie.x,locatie.y);
            Vector3WGS punt2 = RDtoWGS84(locatie.x+100,locatie.y);

            double deltaX = (punt2.lon - origin.lon) * unitsPerDegreeX;
            double deltaY = (punt2.lat - origin.lat) * unitsPerDegreeY;
            double hoekRad = Math.Tan(deltaY / deltaX);
            hoek = -1*(hoekRad * 180 / Math.PI);

            return hoek;
        }
    }
}
