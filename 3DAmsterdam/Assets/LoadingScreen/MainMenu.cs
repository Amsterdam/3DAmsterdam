using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Image loadBar;

    void Start()
    {
        StartCoroutine(LoadScene());
    }

    public IEnumerator LoadScene()
    {
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(1);

        while(loadScene.progress < 1)
        {
            loadBar.fillAmount = loadScene.progress;

            yield return new WaitForEndOfFrame();
        }       
    }
        
}
