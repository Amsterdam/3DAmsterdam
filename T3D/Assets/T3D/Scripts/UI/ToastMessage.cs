using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToastMessage : MonoBehaviour
{
    [SerializeField]
    private float duration = 5f;

    private void OnEnable()
    {
        StartCoroutine(DisappearPopup(duration));
    }

    private IEnumerator DisappearPopup(float duration)
    {
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
    }
}
