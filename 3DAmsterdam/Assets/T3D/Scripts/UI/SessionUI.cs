using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Sharing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionUI : MonoBehaviour
{
    public void StartNewSession()
    {
        SessionSaver.LoadPreviousSession = false;
        SessionSaver.ClearAllSaveData();
        RestartScene();
    }

    public void LoadSavedSession()
    {
        SessionSaver.LoadPreviousSession = true;
        RestartScene(); 
    }

    private void RestartScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}