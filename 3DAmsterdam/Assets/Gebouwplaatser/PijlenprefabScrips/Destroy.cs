using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour {

    private void OnMouseDown()
    {
        Destroy(transform.parent.parent.gameObject);
    }


    public void Remove()
    {
        Destroy(transform.parent.parent.parent.gameObject);
    }
}
