using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Annotation : WorldPointFollower
{
    [SerializeField]
    private Image balloon;

    [SerializeField]
    private Text balloonText;

    [SerializeField]
    private Input editInputField;

    void Start()
    {
        MoveToWorldPosition(Vector3.zero);
    }
}
