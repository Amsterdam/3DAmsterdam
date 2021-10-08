/*
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
using System.Linq;
using UnityEngine;

public class MapsDataLoader : MonoBehaviour
{
    [SerializeField]
    private StringEvent tableNameReceiveEvent;

    [SerializeField]
    private List<MapsDataTable> mapsDataTable;

    public enum SymbolShape
    {
        Circle,
        Square,
        Triangle
    }

    [System.Serializable]
    public struct MapsDataTable
    {
        public string name;
        public string geoJsonURL;
        public Color color;
        public SymbolShape shape;
    }

    void Start()
    {
        tableNameReceiveEvent.stringEvent.AddListener(LoadTable);
    }

    void LoadTable(string tableName)
    {
        try{
            var targetTable = mapsDataTable.First((item) => item.name == tableName);
            StartCoroutine(LoadGeoJSON(targetTable.geoJsonURL));
		}
        catch{
            Debug.Log($"Could not find table item {tableName}");
        }
    }

    private IEnumerator LoadGeoJSON(string geoJsonURL)
    {
        
        yield return new WaitForEndOfFrame();
	}
}