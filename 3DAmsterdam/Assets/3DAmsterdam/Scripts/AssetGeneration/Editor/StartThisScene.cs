using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class AutoRun
{
    [MenuItem("3D Amsterdam/Tools/Bake")]
    public static void StartThisScene(){
        EditorSceneManager.OpenScene("Assets/3DAmsterdam/Scenes/DataGeneration/GenerateBuildingTilesFromGeoJSON.unity");
        EditorApplication.isPlaying = true;
    }
}
