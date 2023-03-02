/* Copyright(C)  X Gemeente
                 X Amsterdam
                 X Economic Services Departments
Licensed under the EUPL, Version 1.2 or later (the "License");
You may not use this work except in compliance with the License. You may obtain a copy of the License at:
https://joinup.ec.europa.eu/software/page/eupl
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied. See the License for the specific language governing permissions and limitations under the License.
*/

using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Netherlands3D.JavascriptConnection;
using UnityEngine.Events;
using Netherlands3D.Events;

namespace Netherlands3D.Traffic.VISSIM 
{ 
    public class VissimStringLoader : MonoBehaviour
    {
        [SerializeField]
        private StringEvent filesImportedEvent;

        [SerializeField]
        private BoolEvent clearDataBaseEvent;

        private string fileExtention = ".fzp";

        [SerializeField] private Playback playback = default;
        [SerializeField] private ConvertFZP converter = default;

		private void Awake()
		{
            filesImportedEvent.AddListenerStarted(FileImported);
        }

        private void FileImported(string files)
        {
            string[] importedFiles = files.Split(',');
            foreach(var file in importedFiles)
            { 
                if(file.EndsWith(fileExtention)){
                    LoadVissimFromFile(file);
                    return;
                }    
            }
        }

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

        private void LoadVissimFromFile(string filepath)
        {
            string contents = File.ReadAllText(Application.persistentDataPath + "/" + filepath);
            File.Delete(filepath);
            StartCoroutine(LoadingProgress(contents));
        }

        IEnumerator LoadingProgress(string vissimData)
        {
            // starts loading and sets to 50% by default
            LoadingScreen.Instance.ProgressBar.SetMessage("50%");
            LoadingScreen.Instance.ProgressBar.Percentage(.5f);
            LoadingScreen.Instance.ShowMessage("FZP wordt geladen: Vissim");
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            // calls vissim data
            converter.ReadFileFZP(vissimData);
            yield return new WaitForSeconds(1f);
            // loading "done"
            LoadingScreen.Instance.ProgressBar.SetMessage("100%");
            LoadingScreen.Instance.ProgressBar.Percentage(1f);
            yield return new WaitForSeconds(0.5f);

            clearDataBaseEvent.InvokeStarted(true);
            LoadingScreen.Instance.Hide();
        }

        public void DestroyVissim()
        {
            converter.allVissimData.Clear(); // cleans all old files
            converter.finishedLoadingData = false;
            playback.vehicles.Clear(); //  clears all old data
            foreach (Transform child in transform)
            {
                // disables all old cars
                child.gameObject.SetActive(false);
            }
        }
    }
}
