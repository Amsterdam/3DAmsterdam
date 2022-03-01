using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAtStart : MonoBehaviour
{
    /// <summary>
    /// Disable object as late as possible on first frame
    /// and remove this script when it has ran once.
    /// </summary>
    void LateUpdate()
    {
        Destroy(this);
        this.gameObject.SetActive(false);
    }
}
