
using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Netherlands3D.JavascriptConnection;
using UnityEngine.Events;

namespace Netherlands3D.Traffic.VISSIM 
{ 

    public class VissimStringLoader : MonoBehaviour
    {
        [SerializeField] private LoadingScreen loadingObjScreen = default;
        [SerializeField] private Playback playback = default;
        [SerializeField] private ConvertFZP converter = default;
        [SerializeField] private GameObject vissimLayerObject = default;
        [SerializeField] private Transform vissimScriptObject = default;

        private void Start()
        {
            vissimLayerObject.SetActive(false);
            //loadingObjScreen = FindObjectOfType<LoadingScreen>();
            //playback = FindObjectOfType<VissimPlayback>();
            //converter = FindObjectOfType<ConvertFZP>();
        }
        public void LoadVissimFromJavascript()
        {
            StartCoroutine(LoadingProgress());
            //Debug.Log(JavascriptMethodCaller.FetchVissimDataAsString());
        }

        IEnumerator LoadingProgress()
        {
            // starts loading and sets to 50% by default
            vissimLayerObject.SetActive(true);
            loadingObjScreen.ProgressBar.SetMessage("50%");
            loadingObjScreen.ProgressBar.Percentage(.5f);
            loadingObjScreen.ShowMessage("FZP wordt geladen: Vissim");
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            // calls vissim data
            string temp = JavascriptMethodCaller.FetchVissimDataAsString();
            converter.ReadFileFZP(temp);
            yield return new WaitForSeconds(1f);
            // loading "done"
            loadingObjScreen.ProgressBar.SetMessage("100%");
            loadingObjScreen.ProgressBar.Percentage(1f);
            yield return new WaitForSeconds(0.5f);
            loadingObjScreen.Hide();

        }

        public void DestroyVissim()
        {

            converter.allVissimData.Clear(); // cleans all old files
            converter.finishedLoadingData = false;
            playback.vehicles.Clear(); //  clears all old data
            foreach (Transform child in vissimScriptObject.transform)
            {
                // disables all old cars
                child.gameObject.SetActive(false);
            }

            vissimLayerObject.SetActive(false); // disables layer
        }
    }
}
