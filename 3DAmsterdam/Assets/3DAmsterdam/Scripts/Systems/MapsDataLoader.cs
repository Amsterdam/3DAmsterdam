using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapsDataLoader : MonoBehaviour
{
    [SerializeField]
    private StringEvent tableNameStringEvent;

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
        tableNameStringEvent.stringEvent.AddListener(LoadTable);
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
