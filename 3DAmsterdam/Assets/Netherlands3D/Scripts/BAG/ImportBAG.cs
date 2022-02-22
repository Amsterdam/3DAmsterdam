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
        public static IEnumerator GetBuildingData(string bagId, Action<BagData.Rootobject> callback)
        {
            // adds data id and url in one string
            string url = Config.activeConfiguration.buildingUrl + bagId + "/?format=" + format;

            // send http request
            var request = UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                WarningDialogs.Instance.ShowNewDialog(requestFailureMessage);
            }
            else
            {
                callback?.Invoke(JsonUtility.FromJson<BagData.Rootobject>(request.downloadHandler.text));
            }
        }

        /// <summary>
        /// Returns a building data object
        /// </summary>
        /// <param name="bagId">The unique building BAG id</param>
        /// <param name="callback">The callback action containing the building data object</param>
        public static IEnumerator GetBuildingDataKadasterViewer(string bagId, Action<BagDataKadasterViewer> callback)
        {
            // adds data id and url in one string
            string url = Config.activeConfiguration.buildingUrl + bagId;

            // send http request
            var request = UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                WarningDialogs.Instance.ShowNewDialog(requestFailureMessage);
            }
            else
            {
                callback?.Invoke(JsonUtility.FromJson<BagDataKadasterViewer>(request.downloadHandler.text));
            }
        }

        /// <summary>
        /// Returns a list of addresses tied to a building Bag ID
        /// </summary>
        /// <param name="bagId">The building Bag ID</param>
        /// <param name="callback">The callback action containing the building adresses data object</param>
        public static IEnumerator GetBuildingAdresses(string bagId, Action<BagData.Rootobject> callback)
        {
            // adds data id and url in one string
            string url = Config.activeConfiguration.numberIndicatorURL + bagId + "&format=" + format;

            // send http request
            var request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                WarningDialogs.Instance.ShowNewDialog(requestFailureMessage);
            }
            else
            {
                callback?.Invoke(JsonUtility.FromJson<BagData.Rootobject>(request.downloadHandler.text));
            }
        }

        /// <summary>
        /// Get all the data tied to a address Bag ID
        /// </summary>
        /// <param name="bagId">The address Bag ID</param>
        /// <param name="callback">The callback action containing the adresses data object</param>
        /// <returns></returns>
        public static IEnumerator GetAddressData(string bagId, Action<BagData.AddressInstance> callback)
        {
            // adds data id and url in one string
            string url = Config.activeConfiguration.numberIndicatorInstanceURL + bagId + "/?format=" + format;

            // send http request
            var request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                WarningDialogs.Instance.ShowNewDialog(requestFailureMessage);
            }
            else
            {
                callback?.Invoke(JsonUtility.FromJson<BagData.AddressInstance>(request.downloadHandler.text));
            }
        }
    }
}