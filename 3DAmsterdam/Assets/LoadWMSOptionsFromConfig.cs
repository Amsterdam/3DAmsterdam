using Netherlands3D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadWMSOptionsFromConfig : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    private Toggle toggleTemplate;
    [SerializeField] private ConfigurationFile configurationFile;

    private void Awake()
    {
        toggleTemplate = transform.GetChild(0).GetComponent<Toggle>();
        toggleTemplate.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        StartCoroutine(LoadWMSConfig());
    }

    IEnumerator LoadWMSConfig()
    {

        yield return null;
    }

}
