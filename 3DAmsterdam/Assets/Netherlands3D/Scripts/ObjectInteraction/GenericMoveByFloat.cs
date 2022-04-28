using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericMoveByFloat : MonoBehaviour
{
    [SerializeField]
    private Vector3 direction;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = this.transform.position;
    }
  
    public void MoveObject(float value)
    {
        this.transform.position = startPosition + direction * value;
    }
}
