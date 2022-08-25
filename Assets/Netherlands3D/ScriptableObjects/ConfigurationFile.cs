using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Netherlands3D
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ConfigurationFile", order = 0)]
    [System.Serializable]
    public class ConfigurationFile : ScriptableObject
    {
        public enum TmsTileNumberingType
        {
            GoogleAndOSM, // (0 to 2zoom-1, 0 to 2zoom-1) for the range(-180, +85.0511) - (+180, -85.0511)
            TMS, //'Tile Map Service' (0 to 2zoom-1, 2zoom-1 to 0) for the range(-180, +85.0511) - (+180, -85.0511). (That is, the same as the previous with the Y value flipped.)
            QuadTrees //Used by Microsoft
        }

        [Header("Bounding Box coordinates")]
        public Vector2RD RelativeCenterRD;
        public Vector2RD BottomLeftRD;
        public Vector2RD TopRightRD;

        public float zeroGroundLevelY = 43.0f;

        //wordt niet meer gebruikt
        public string webserverRootPath = "";
        public string buildingsMetaDataPath = "";

        private Regex regex_url = new Regex(@"https:\/\/t3d-.", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private string _T3DSandboxEnvironment = null;
        private string _T3DAzureFunctionURL = null;

        [HideInInspector]
        public string T3DAzureWebroot
        {
            get
            {
                if (string.IsNullOrEmpty(_T3DSandboxEnvironment))
                {
                    var url = Application.absoluteURL;
                    _T3DSandboxEnvironment = "https://t3dstorage.z6.web.core.windows.net/"; //default

                    if (string.IsNullOrEmpty(url)) return _T3DSandboxEnvironment;
                    if (url.StartsWith("http://localhost")) return _T3DSandboxEnvironment;
                    if (url.StartsWith("https://t3dstorage.")) return _T3DSandboxEnvironment;

                    MatchCollection matches = regex_url.Matches(url);
                    if (matches.Count == 0) throw new Exception("Kan omgeving niet detecteren");

                    var urlstart = matches[0].Value;
                    _T3DSandboxEnvironment = $"{urlstart}-cdn.azureedge.net/";
                }

                return _T3DSandboxEnvironment;
            }
        }

        [HideInInspector]
        public string T3DAzureFunctionURL
        {
            get
            {
                if (string.IsNullOrEmpty(_T3DAzureFunctionURL))
                {
                    var url = Application.absoluteURL;
                    _T3DAzureFunctionURL = "https://t3d-o-functions.azurewebsites.net/"; //default

                    if (string.IsNullOrEmpty(url)) return _T3DAzureFunctionURL;
                    if (url.StartsWith("http://localhost")) return _T3DAzureFunctionURL;
                    if (url.StartsWith("https://t3dstorage.")) return _T3DAzureFunctionURL;

                    MatchCollection matches = regex_url.Matches(url);
                    if (matches.Count == 0) throw new Exception("Kan omgeving niet detecteren");

                    var urlstart = matches[0].Value;
                    _T3DAzureFunctionURL = $"{urlstart}-functions.azurewebsites.net/";
                }

                return _T3DAzureFunctionURL;
            }
        }

        public string CityJSONUploadEndoint
        {
            get
            {
                return @"https://voorportaal.azurewebsites.net/api/uploadcityjson";
            }
        }

        public string CityJSONUploadEndpointToken
        {
            get
            {
                return "qCUevbaM8BFtkT32TyLjjNsm6Mr7Rfty6KL8kPSQ";
            }
        }
    }
}