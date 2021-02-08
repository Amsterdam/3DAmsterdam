
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class NameAndURL : OpenURL
    {
        [SerializeField]
        private Text titleText;

        void Awake()
        {
            titleText.GetComponent<Text>();
        }

        public void SetURL(string urlName, string urlPath)
        {
            titleText.name = urlName;
            gameObject.name = urlPath;
        }
	}
}