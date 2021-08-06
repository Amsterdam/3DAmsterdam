﻿/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
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
