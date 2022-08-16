using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ErrorService
{
    private const string errorSceneName = "ErrorScene";
    public static string ErrorMessage { get; private set; }
    public static Scene ErrorScene => SceneManager.GetSceneByName(errorSceneName);

    public static void GoToErrorPage(string errorMessage)
    {
        Debug.LogError("Error found, going to error page: " + errorMessage);
        var monoBehaviours = Object.FindObjectsOfType<MonoBehaviour>();
        foreach (var m in monoBehaviours)
        {
            m.StopAllCoroutines();
        }

        SceneManager.LoadScene(errorSceneName);
        ErrorMessage = errorMessage;
    }
}
