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

    //public void SaveSessionAndReloadAfterSuccess()
    //{
    //    SessionSaver.ExportSavedData();
    //    SessionSaver.Saver.SavingCompleted += Saver_SavingCompleted;
    //}

    //private void Saver_SavingCompleted(bool saveSucceeded)
    //{
    //    SessionSaver.Saver.SavingCompleted -= Saver_SavingCompleted;

    //    if (saveSucceeded)
    //    {
    //        RestartScene();
    //    }
    //    else
    //    {
    //        Debug.Log("saving failed, trying again");
    //        SaveSessionAndReloadAfterSuccess();
    //    }
    //}

    private void RestartScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}
