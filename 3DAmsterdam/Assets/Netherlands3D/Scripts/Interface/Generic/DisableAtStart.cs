using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAtStart : MonoBehaviour
{
    void Start()
    {
        this.gameObject.SetActive(false);
    }
}
