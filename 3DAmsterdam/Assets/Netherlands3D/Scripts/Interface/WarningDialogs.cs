using Netherlands3D.Help;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    public class WarningDialogs : MonoBehaviour
    {
        public static WarningDialogs Instance;

        [SerializeField] private Warning warningPrefab;
        [SerializeField] private bool useLightWarnings = true;
        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Spawns a new warning prefab with a message. Leave blank for default server warning.
        /// </summary>
        /// <param name="message">Optional custom message</param>
        /// <param name="clearOldWarnings">Deletes all existing warnings</param>
        public void ShowNewDialog(string message = "", bool clearOldWarnings = true)
        {
            if(useLightWarnings)
            {
                HelpMessage.Show(message);
                return;
            }


            if (clearOldWarnings)
            {
                foreach (Transform child in transform)
                    Destroy(child.gameObject);
            }

            var newWarning = Instantiate(warningPrefab, this.transform);
            newWarning.SetMessage(message);
            this.transform.SetAsLastSibling();
        }
    }
}