using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Sharing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionUI : MonoBehaviour
{
    public void StartNewSession()
    {
        SessionSaver.ClearAllSaveData();
        SessionSaver.LoadPreviousSession = false;
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void LoadSavedSession()
    {
        SessionSaver.LoadPreviousSession = true;
        SessionSaver.LoadSaveData();
        //SceneSerializer.Instance.LoadBuilding();
    }

    public void SaveSession()
    {
        SessionSaver.ExportSavedData();
    }
}
