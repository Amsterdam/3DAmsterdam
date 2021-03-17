using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.Style
{
    public class Logo : MonoBehaviour
    {
        [SerializeField]
        private Image image;
        void Start()
        {
            image.sprite = Config.activeConfiguration.logo;
        }
    }
}
