﻿using Netherlands3D.JavascriptConnection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Cameras;

namespace Netherlands3D.Interface
{
    public class CanvasSettings : MonoBehaviour
    {
        [SerializeField]
        private CanvasScaler canvasScaler;
        private string canvasScaleFactorKey = "canvasScaleFactor";

        //static scale we can request through class
        public static float canvasScale = 1.0f;

        private float maxAutoWidth = 1.5f;

        private float referenceWidth = 1920.0f;

        [SerializeField]
        private GameObject mainMenu;

        [SerializeField]
        private GameObject interfaceLayers;

        void Start()
        {
            CameraModeChanger.Instance.OnFirstPersonModeEvent += DisableMainCanvasItems;
        }

        void DisableMainCanvasItems()
        {
            interfaceLayers.SetActive(false);
            mainMenu.SetActive(false);
        }

        [ContextMenu("Clear the stored Canvas settings PlayerPrefs")]
        public void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteKey(canvasScaleFactorKey);
        }

        /// <summary>
        /// Scale up the canvas when we have a screen width high DPI like a 4k monitor or ultrawide.
        /// Do not go all the way up to 2x, but it is allowed through the settings menu.
        /// </summary>
        public float DetectPreferedCanvasScale()
        {
            canvasScale = Mathf.Clamp(Screen.width / referenceWidth, 1.0f, maxAutoWidth);
            canvasScaler.scaleFactor = canvasScale;
            JavascriptMethodCaller.SetInterfaceScale(canvasScale);
            return canvasScale;
        }

        /// <summary>
        /// Change the canvas scale and store the value in the persistent PlayerPrefs
        /// </summary>
        /// <param name="scaleFactor"></param>
        public void ChangeCanvasScale(float scaleFactor)
        {
            canvasScale = scaleFactor;
            canvasScaler.scaleFactor = canvasScale;
            PlayerPrefs.SetFloat(canvasScaleFactorKey, canvasScale);
            JavascriptMethodCaller.SetInterfaceScale(canvasScale);
        }
    }
}