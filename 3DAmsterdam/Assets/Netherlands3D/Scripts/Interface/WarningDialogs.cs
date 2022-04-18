using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    public class WarningDialogs : MonoBehaviour, IUniqueService
    {
        [SerializeField]
        private Warning warningPrefab;

        /// <summary>
        /// Spawns a new warning prefab with a message. Leave blank for default server warning.
        /// </summary>
        /// <param name="message">Optional custom message</param>
        /// <param name="clearOldWarnings">Deletes all existing warnings</param>
        public void ShowNewDialog(string message = "", bool clearOldWarnings = true)
        {
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