using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Netherlands3D.Interface
{
    public class TabPanel : MonoBehaviour
    {
        [SerializeField]
        private Transform fieldsContainer;
		public Transform FieldsContainer { get => fieldsContainer; }

		public void Open(bool open = true)
        {
            this.gameObject.SetActive(open);
		}
    }
}