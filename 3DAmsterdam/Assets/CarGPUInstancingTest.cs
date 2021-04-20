using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarGPUInstancingTest : MonoBehaviour
{
    [SerializeField]
    private Material carShader;

    [SerializeField]
    private Material wheelShader;

    private bool instancing = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            instancing = !instancing;

            carShader.enableInstancing = instancing;
            wheelShader.enableInstancing = instancing;
            Debug.Log("Instancing enabled: " + instancing);
        }
    }
}
