using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.Interface.SidePanel
{
    public class ShowExternalData : MonoBehaviour
    {
        [SerializeField]
        private string metaDataPath = "/metadata.xml";

        public void ShowMetaData()
        {
            PropertiesPanel.Instance.OpenPanel();
            PropertiesPanel.Instance.ClearGeneratedFields();
            PropertiesPanel.Instance.AddLoadingSpinner();
            StartCoroutine(GetText());
        }

        private IEnumerator GetText()
        {
            UnityWebRequest www = UnityWebRequest.Get(metaDataPath + metaDataPath);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                Debug.Log(www.downloadHandler.text);

                // Or retrieve results as binary data
                byte[] results = www.downloadHandler.data;
            }
        }
    }
}