using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class AutoRun
{
    /// <summary>
    /// Creating a static method like this allows you to run the method via batchmode
    /// </summary>
    public static void StartThisScene(){
        EditorSceneManager.OpenScene("Assets/3DAmsterdam/Scenes/DataGeneration/GenerateBuildingTiles.unity");
        EditorApplication.isPlaying = true;
    }
}
