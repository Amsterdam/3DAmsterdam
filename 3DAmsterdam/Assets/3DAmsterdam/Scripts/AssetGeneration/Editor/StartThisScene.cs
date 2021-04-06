using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class AutoRun
{
    public static void StartThisScene(){
        EditorSceneManager.OpenScene("Assets/3DAmsterdam/Scenes/DataGeneration/GenerateBuildingTilesFromGeoJSON.unity");
        EditorApplication.isPlaying = true;
    }
}
