using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeMaskSphere : MonoBehaviour
{
    [SerializeField]
    private Material groundMaterial;

    void Update()
    {
        groundMaterial.SetVector("_ClippingMaskDome", new Vector4(this.transform.position.x, this.transform.position.y, this.transform.position.z, this.transform.localScale.x-1));
    }
}
