using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Netherlands3D.Cameras;

namespace Netherlands3D.Interfac
{
    public class NAPHeight : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI heightText;

        [SerializeField]
        private string textSuffix = "m";

        [SerializeField]
        private float multiplier = 5;
        [SerializeField]
        private float minimumHeight = 2;


        void Start()
        {
            SetText();
        }

        void LateUpdate()
        {
            SetText();
        }

        private void SetText()
        {
            float heightInNAP = Mathf.Round(CameraModeChanger.Instance.ActiveCamera.transform.position.y - Config.activeConfiguration.zeroGroundLevelY);
            heightText.text = heightInNAP + textSuffix;
        }

        public void ChangeHeight(bool isIncrease)
        {
            if (Camera.main.transform.position.y <= minimumHeight)
                return;

            Camera.main.transform.Translate((isIncrease ? Vector3.up : Vector3.down) * (Camera.main.transform.position.y / 100) * multiplier, Space.World);
            SetText();
        }
    }

}