using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface.Sharing
{
    public class ShareDialog : MonoBehaviour
    {
        public enum SharingState
        {
            SHARING_OPTIONS,
            SHARING_SCENE,
            SHOW_URL
        }


        [SerializeField]
        private RectTransform shareOptions;

        [SerializeField]
        private RectTransform progressFeedback;
        [SerializeField]
        private ProgressBar progressBar;

        [SerializeField]
        private RectTransform generatedURL;
        private InputField generatedURLInputField;
        private string generatedURLAddress = "";


        private SharingState state = SharingState.SHARING_OPTIONS;

        void OnEnable()
        {
            ChangeState(SharingState.SHARING_OPTIONS);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public void GenerateURL()
        {
            //Start uploading scene settings, and models to unique URL/session ID
            StartCoroutine(Share());
        }

        private IEnumerator Share()
        {
            //TODO real server upload/feedback
            ChangeState(SharingState.SHARING_SCENE);
            progressBar.SetMessage("Instellingen opslaan..");
            progressBar.Percentage(0.2f);

            yield return new WaitForSeconds(1.0f);
            progressBar.SetMessage("Object A wordt geupload..");
            progressBar.Percentage(0.5f);

            yield return new WaitForSeconds(2.0f);
            progressBar.SetMessage("Object B wordt geupload..");
            progressBar.Percentage(0.8f);

            //Temp fake random URL
            generatedURLAddress = "https://3d.amsterdam.nl/?view=" + Path.GetRandomFileName().Split('.')[0];
            generatedURLInputField.text = generatedURLAddress;

            ChangeState(SharingState.SHOW_URL);
            yield return null;
        }

        public void ChangeState(SharingState newState)
        {
            switch (newState)
            {
                case SharingState.SHARING_OPTIONS:
                    shareOptions.gameObject.SetActive(true);
                    break;
                case SharingState.SHARING_SCENE:
                    shareOptions.gameObject.SetActive(false);
                    progressFeedback.gameObject.SetActive(true);
                    break;
                case SharingState.SHOW_URL:
                    shareOptions.gameObject.SetActive(false);
                    progressFeedback.gameObject.SetActive(false);
                    generatedURL.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }
    }
}