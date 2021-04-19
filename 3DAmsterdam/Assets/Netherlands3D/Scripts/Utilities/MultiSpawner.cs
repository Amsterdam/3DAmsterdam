using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiSpawner : MonoBehaviour
{
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private int copies = 100;

    void Start()
    {
        var copySource = transform.GetChild(0);

        for (int i = 1; i <= copies; i++)
		{
            GameObject copy = Instantiate(copySource.gameObject, copySource.transform.parent, true);
            copy.transform.position = copySource.transform.position + offset*i;
        }
    }
}
