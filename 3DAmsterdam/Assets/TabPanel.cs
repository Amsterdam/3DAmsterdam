using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Netherlands3D.Interface
{
    public class TabPanel : MonoBehaviour
    {
        public void Open(bool open = true)
        {
            this.gameObject.SetActive(open);
		}
    }
}