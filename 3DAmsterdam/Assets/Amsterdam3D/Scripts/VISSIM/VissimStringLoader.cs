using Amsterdam3D.CameraMotion;
using Amsterdam3D.Interface;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Amsterdam3D.JavascriptConnection;
using UnityEngine.Events;

namespace Amsterdam3D.Parsing
{

    public class VissimStringLoader : MonoBehaviour
    {
        [SerializeField] private LoadingScreen loadingObjScreen = default;
        [SerializeField] private VissimPlayback playback = default;
        [SerializeField] private ConvertFZP converter = default;
        [SerializeField] private GameObject vissimLayerObject = default;
        [SerializeField] private Transform vissimScriptObject = default;

        private void Start()
        {
            vissimLayerObject.SetActive(false);
        }
        public void LoadVissimFromJavascript()
        {
            StartCoroutine(LoadingProgress());
            //Debug.Log(JavascriptMethodCaller.FetchVissimDataAsString());
        }

        IEnumerator LoadingProgress()
        {
            vissimLayerObject.SetActive(true);
            loadingObjScreen.ProgressBar.SetMessage("50%");
            loadingObjScreen.ProgressBar.Percentage(.5f);
            loadingObjScreen.ShowMessage("FZP wordt geladen: Vissim");
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            string temp = JavascriptMethodCaller.FetchVissimDataAsString();
            converter.ReadFileFZP(temp);
            yield return new WaitForSeconds(1f);
            loadingObjScreen.ProgressBar.SetMessage("100%");
            loadingObjScreen.ProgressBar.Percentage(1f);
            yield return new WaitForSeconds(0.5f);
            loadingObjScreen.Hide();

            //Debug.Log(JavascriptMethodCaller.FetchVissimDataAsString());
            // stuur deze static string door naar de file loader
            //zet t scherm op 100 en haal scherm weg na t laden, 
        }

        public void DestroyVissim()
        {
            foreach (Transform child in vissimScriptObject.transform)
            {
                child.gameObject.SetActive(false);
                //Destroy(child.gameObject); // deletes all child cars
            }

            converter.allVissimData.Clear(); // cleans all old files
            playback.vehicles.Clear(); //  clears all old data

            vissimLayerObject.SetActive(false); // disables layer
        }
    }
}
