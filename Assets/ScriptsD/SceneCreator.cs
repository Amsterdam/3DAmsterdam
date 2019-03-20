using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCreator : MonoBehaviour
{
    public GameObject parent;
    public GameObject ground;
    public int numOfObjects;

    private GameObject cube;

    private void Start()
    {
        for (int i = 0; i < numOfObjects; i++)
        {
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(Random.Range(1, 4), Random.Range(1, 4), Random.Range(1, 4));
            cube.AddComponent<Rigidbody>();

            cube.transform.parent = parent.transform;
            cube.transform.position = new Vector3(Random.Range(-ground.transform.localScale.x / 2, ground.transform.localScale.x / 2) * 10f, 1,
                                                  Random.Range(-ground.transform.localScale.z / 2, ground.transform.localScale.z / 2) * 10f);
        }
    }
}
