using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalingKeepScale : MonoBehaviour
{
    private float StdScale = 4;
    private float OscaleX, ScaleX, ScaleY, ScaleZ;

    // Start is called before the first frame update
    void Start()
    {
        OscaleX = gameObject.transform.parent.localScale.x;
        
    }

    // Update is called once per frame
    void Update()
    {
        ScaleX = (gameObject.transform.parent.localScale.x / (gameObject.transform.parent.localScale.x / OscaleX)) * StdScale;
        ScaleY = (gameObject.transform.parent.localScale.y / gameObject.transform.localScale.y) * StdScale;
        ScaleZ = (gameObject.transform.parent.localScale.z / gameObject.transform.localScale.z) * StdScale;

        Debug.Log(ScaleX + " - "+ gameObject.transform.parent.localScale.y + " - "+ gameObject.transform.parent.localScale.z);

        transform.localScale = new Vector3(ScaleX,ScaleY,ScaleZ);
    }
}
