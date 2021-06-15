
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

        [SerializeField]
        private UnityEvent doneLoadingModel;
        [SerializeField] private LoadingScreen loadingObjScreen = default;
        [SerializeField] private Playback playback = default;
        [SerializeField] private ConvertFZP converter = default;
        [SerializeField] private Transform vissimScriptObject = default;

#if UNITY_EDITOR
        /// <summary>
        /// For Editor testing only.
        /// This method loads a obj and a mtl file.
        /// </summary>
        [ContextMenu("Open selection dialog")]
        public void LoadVissimViaEditor()
        {
            if (!Application.isPlaying) return;

            string vissimData = UnityEditor.EditorUtility.OpenFilePanel("Open FZP", "", "fzp");

            if(vissimData != "")
            {
                StartCoroutine(LoadingProgress(File.ReadAllText(vissimData)));
            }
        }
#endif

        public void LoadVissimFromJavascript()
        {
            StartCoroutine(LoadingProgress(JavascriptMethodCaller.FetchVissimDataAsString()));
        }

        IEnumerator LoadingProgress(string vissimData)
        {
            // starts loading and sets to 50% by default
            loadingObjScreen.ProgressBar.SetMessage("50%");
            loadingObjScreen.ProgressBar.Percentage(.5f);
            loadingObjScreen.ShowMessage("FZP wordt geladen: Vissim");
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            // calls vissim data
            converter.ReadFileFZP(vissimData);
            yield return new WaitForSeconds(1f);
            // loading "done"
            loadingObjScreen.ProgressBar.SetMessage("100%");
            loadingObjScreen.ProgressBar.Percentage(1f);
            yield return new WaitForSeconds(0.5f);
            loadingObjScreen.Hide();
            doneLoadingModel.Invoke();
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
        }
    }
}
