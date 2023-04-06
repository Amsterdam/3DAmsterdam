using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Netherlands3D.Interface;

namespace Netherlands3D.BAG
{
    public class ImportBAG
    {
        public const string format = "json";
        public const string requestFailureMessage = "Sorry, er is geen data gevonden voor dit pand.";

        /// <summary>
        /// Returns a building data object
        /// </summary>
        /// <param name="bagId">The unique building BAG id</param>
        /// <param name="callback">The callback action containing the building data object</param>
        public static IEnumerator GetBuildingData(string bagId, string key, Action<BagDataKadasterBuilding.Rootobject> callback)
        {
            // adds data id and url in one string
            string url = Config.activeConfiguration.kadasterBuildingURL.Replace("{bagid}", bagId);
            Debug.Log("Kadaster request: " + url);
            // send http request
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("X-Api-Key", key);
            request.SetRequestHeader("Accept-Crs", "epsg:28992");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                WarningDialogs.Instance.ShowNewDialog(requestFailureMessage);
            }
            else
            {
                //TODO: Change entire BAG logic to use SimpleJSON to avoid this unsupported nesting problems by JsonUtility
                var jsonWithFlattenedGeometryArray = request.downloadHandler.text.
                    Replace("[", "").Replace("]", ""). //Clear all double brackets
                    Replace("coordinates\":", "coordinates\":["). //Restore single opening bracket
                    Replace("},\"oorspronkelijkBouwjaar", "]},\"oorspronkelijkBouwjaar"); //Restore single closing bracket

                callback?.Invoke(JsonUtility.FromJson<BagDataKadasterBuilding.Rootobject>(jsonWithFlattenedGeometryArray));
            }
        }

        /// <summary>
        /// Returns a list of addresses tied to a building Bag ID
        /// </summary>
        /// <param name="bagId">The building Bag ID</param>
        /// <param name="callback">The callback action containing the building adresses data object</param>
        public static IEnumerator GetBuildingAdresses(string bagId, string key , Action<BagDataKadasterBuildingAdresses.Rootobject> callback)
        {
            // adds data id and url in one string
            string url = Config.activeConfiguration.kadasterBuildingAdressesURL.Replace("{bagid}", bagId);
            Debug.Log("Kadaster request: " + url);
            // send http request
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("X-Api-Key", key);
            request.SetRequestHeader("Accept-Crs", "epsg:28992");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                WarningDialogs.Instance.ShowNewDialog(requestFailureMessage);
            }
            else
            {
                callback?.Invoke(JsonUtility.FromJson<BagDataKadasterBuildingAdresses.Rootobject>(request.downloadHandler.text));
            }
        }

        /// <summary>
        /// Returns a building data object
        /// </summary>
        /// <param name="bagId">The unique building BAG id</param>
        /// <param name="callback">The callback action containing the building data object</param>
        public static IEnumerator GetBuildingDataAmsterdam(string bagId, Action<BagDataAmsterdam.Rootobject> callback)
        {
            // adds data id and url in one string
            string url = Config.activeConfiguration.buildingUrl.Replace("{bagid}", bagId);

            // send http request
            var request = UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                WarningDialogs.Instance.ShowNewDialog(requestFailureMessage);
            }
            else
            {
                callback?.Invoke(JsonUtility.FromJson<BagDataAmsterdam.Rootobject>(request.downloadHandler.text));
            }
        }

        /// <summary>
        /// Returns a list of addresses tied to a building Bag ID
        /// </summary>
        /// <param name="bagId">The building Bag ID</param>
        /// <param name="callback">The callback action containing the building adresses data object</param>
        public static IEnumerator GetBuildingAdressesAmsterdam(string bagId, Action<BagDataAmsterdam.Rootobject> callback)
        {
            // adds data id and url in one string
            string url = Config.activeConfiguration.numberIndicatorURL.Replace("{bagid}", bagId);
            Debug.Log($"Adress requests: {url}");
            // send http request
            var request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                WarningDialogs.Instance.ShowNewDialog(requestFailureMessage);
            }
            else
            {
                callback?.Invoke(JsonUtility.FromJson<BagDataAmsterdam.Rootobject>(request.downloadHandler.text));
            }
        }

        /// <summary>
        /// Get all the data tied to a address Bag ID
        /// </summary>
        /// <param name="bagId">The address Bag ID</param>
        /// <param name="callback">The callback action containing the adresses data object</param>
        /// <returns></returns>
        public static IEnumerator GetAddressData(string bagId, Action<BagDataAmsterdam.AddressInstance> callback)
        {
            // adds data id and url in one string
            string url = Config.activeConfiguration.numberIndicatorInstanceURL.Replace("{bagid}", bagId);

            // send http request
            var request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                WarningDialogs.Instance.ShowNewDialog(requestFailureMessage);
            }
            else
            {
                callback?.Invoke(JsonUtility.FromJson<BagDataAmsterdam.AddressInstance>(request.downloadHandler.text));
            }
        }
    }
}