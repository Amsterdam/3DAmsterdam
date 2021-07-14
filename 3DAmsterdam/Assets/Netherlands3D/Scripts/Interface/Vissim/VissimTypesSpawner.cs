using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Netherlands3D.Traffic.VISSIM
{
    public class VissimTypesSpawner : MonoBehaviour
    {
        public GameObject vissimButton;
        public List<VissimType> allVissimTypes;
        // Start is called before the first frame update
        void Start()
        {
            foreach (VissimType type in allVissimTypes)
            {
                // creates the  Vissim type button
                GameObject temp = Instantiate(vissimButton, transform.position, transform.rotation);
                temp.transform.SetParent(transform);

                //VissimTypeButton tempButton = GetComponent<VissimTypeButton>();
                temp.GetComponent<VissimTypeButton>().thisType = type;
            }
        }
    }
}