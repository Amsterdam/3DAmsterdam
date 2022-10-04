using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnEnableOrDisable : MonoBehaviour
{
    [SerializeField] private UnityEvent onEnable;
    [SerializeField] private UnityEvent onDisable;

    private void OnEnable()
    {
        onEnable.Invoke();
    }

    private void OnDisable()
    {
        onDisable.Invoke();
    }
}
