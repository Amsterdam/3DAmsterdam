using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Interface.Redesign
{
    public class OpenURL : MonoBehaviour
    {
        [SerializeField] private string URL;

        public void SetUrl(string url)
        {
            this.URL = url;
        }

        public void Open()
        {
            Debug.Log("Trying to open URL: " + URL);
            Application.OpenURL(URL);
        }
    }
}
