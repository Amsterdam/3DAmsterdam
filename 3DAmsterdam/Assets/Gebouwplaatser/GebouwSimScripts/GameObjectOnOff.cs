using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectOnOff : MonoBehaviour
{
    //public GameObject ObjectToImport;

   public void ObjectOn(GameObject ObjectToOn)
    {
        ObjectToOn.SetActive(true);
    }
}
