using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileDownloadOption : ScriptableObject
{
    [System.Serializable]
    public class ProfileDownloadEntry
    {
        public string name;
        public TriggerEvent trigger;
    }
}
