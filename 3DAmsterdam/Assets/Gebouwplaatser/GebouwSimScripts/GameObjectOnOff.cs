using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectOnOff : MonoBehaviour
{
   public void ObjectOn(GameObject ObjectToOn)
    {
        ObjectToOn.SetActive(true);
    }
}
