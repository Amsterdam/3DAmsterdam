using Netherlands3D.Core;
using Netherlands3D;
using Netherlands3D.Events;
using Netherlands3D.Interface;
using Netherlands3D.Interface.Search;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.JavascriptConnection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CsvFilePanel : MonoBehaviour
{
    [SerializeField]
    private GameObject marker;

    [SerializeField]
    private Transform GeneratedFieldsContainer;

    [SerializeField]
    private GameObject UIClearIgnoreObject;

    [SerializeField]
    private LoadingScreen loadingObjScreen;


    private Dictionary<string,bool> selectedColumnsToDisplay = new System.Collections.Generic.Dictionary<string,bool>();

    private CsvGeoLocation csvGeoLocation;

    private GameObject LocationMarkersParent;

    private ActionDropDown currentFilterDropdown;

    [SerializeField]
    private StringEvent filesImportedEvent;

    private void Awake()
    {
        ToggleActiveEvent.Subscribe(OnToggleActive);

		filesImportedEvent.started.AddListener(LoadCsvFromFile);
    }
    public void LoadCsvFromFile(string filename)
    {
        if (!filename.EndsWith(".csv") && !filename.EndsWith(".CSV"))
            return;

        var csv = File.ReadAllText(Application.persistentDataPath + "/" + filename);
        File.Delete(filename);
        ParseCsv(csv);
        loadingObjScreen.Hide();
    }

    private void OnToggleActive(object sender, ToggleActiveEvent.Args e)
    {
        var toggle = (bool)sender;
        gameObject.SetActive(toggle);
    }

    public void ParseCsv(string csv)
    {
        if (LocationMarkersParent == null)
        {
            LocationMarkersParent = new GameObject("LocationMarkers");
        }
        else
        {
            ClearLocationMarkers();
            Reset();
        }

        PropertiesPanel.Instance.SetDynamicFieldsTargetContainer(GeneratedFieldsContainer);
        PropertiesPanel.Instance.ClearGeneratedFields(UIClearIgnoreObject);

        csvGeoLocation = new CsvGeoLocation(csv);

        if (csvGeoLocation.Status != CsvGeoLocation.CsvGeoLocationStatus.Success)
        {
            PropertiesPanel.Instance.AddSpacer(20);

            foreach (var line in csvGeoLocation.StatusMessageLines)
            {
                PropertiesPanel.Instance.AddTextfieldColor(line, Color.red, FontStyle.Normal);
            }

            return;
        }
               
        PropertiesPanel.Instance.AddLabel("Label");
        PropertiesPanel.Instance.AddActionDropdown(csvGeoLocation.ColumnsExceptCoordinates, (action) =>
        {            
            csvGeoLocation.LabelColumnName = action;
            csvGeoLocation.SetlabelIndex(action);

        }, "");

        PropertiesPanel.Instance.AddSpacer(10);
        PropertiesPanel.Instance.AddLabel("Welke informatie wilt u zichtbaar maken als er op een label geklikt wordt?");
        PropertiesPanel.Instance.AddSpacer(10);

        foreach (var column in csvGeoLocation.Columns)
        {
            if (csvGeoLocation.CoordinateColumns.Contains(column)) continue;

            selectedColumnsToDisplay.Add(column, true);
            PropertiesPanel.Instance.AddActionCheckbox(column, true, (action) =>
            {
                selectedColumnsToDisplay[column] = action;
            });
        }

        PropertiesPanel.Instance.AddActionButtonBig("Toon data", (action) =>
        {
            MapAndShow();               
        });

    }

    void MapAndShow()
    {
        ShowAll();

        PropertiesPanel.Instance.ClearGeneratedFields(UIClearIgnoreObject);
        
        PropertiesPanel.Instance.AddLabel($"CSV file geladen met {csvGeoLocation.Rows.Count} rijen");
        PropertiesPanel.Instance.AddLabel("Klik op een icoon voor details");

    }

    private void Reset()
    {
        selectedColumnsToDisplay.Clear();
        labels.Clear();
    }

    public void ClearLocationMarkers()
    {
        foreach (Transform marker in LocationMarkersParent.transform)
        {
            Destroy(marker.gameObject);
        }
    }

    private List<TextMesh> labels = new List<TextMesh>();

    void ShowAll()
    {
        var firstrow = csvGeoLocation.Rows[0];
        double firstrow_x = double.Parse(firstrow[csvGeoLocation.XColumnIndex]);
        bool isRd = csvGeoLocation.IsRd(firstrow_x);

        for (int rowindex = 0; rowindex < csvGeoLocation.Rows.Count; rowindex++)
        {
            var row = csvGeoLocation.Rows[rowindex];

            var locationMarker = Instantiate(marker, LocationMarkersParent.transform);

            var billboard = locationMarker.GetComponent<Billboard>();
            var textmesh = locationMarker.GetComponentInChildren<TextMesh>();
            textmesh.text = row[csvGeoLocation.LabelColumnIndex];

            labels.Add(textmesh);

            billboard.Index = rowindex;
            billboard.Row = row;
            billboard.ClickAction = (action =>
            {
                Show(action);
            });

            double x = double.Parse(row[csvGeoLocation.XColumnIndex]);
            double y = double.Parse(row[csvGeoLocation.YColumnIndex]);

            Vector3 pos;

            if (isRd)
            {
                pos = CoordConvert.RDtoUnity(new Vector3RD(x, y, 7));
            }
            else
            {
                pos = CoordConvert.WGS84toUnity(new Vector3WGS(y, x, 7));
            }

            locationMarker.transform.position = pos;
        }
    }

    private void UpdateLabels()
    {
        for (int i = 0; i < csvGeoLocation.Rows.Count; i++) 
        {
            var row = csvGeoLocation.Rows[i];
            labels[i].text = row[csvGeoLocation.LabelColumnIndex];
        }
     }

    void Show(int index)
    {
        PropertiesPanel.Instance.ClearGeneratedFields(UIClearIgnoreObject);

        //dropdown to select the label of the pointer
        PropertiesPanel.Instance.AddLabel("Label");
        PropertiesPanel.Instance.AddActionDropdown(csvGeoLocation.ColumnsExceptCoordinates, (action) =>
        {
            Debug.Log($"label: {action}");
            csvGeoLocation.LabelColumnName = action;
            csvGeoLocation.SetlabelIndex(action);

            UpdateLabels();
            currentFilterDropdown.UpdateOptions(csvGeoLocation.GetFiltersByColumn());

        }, csvGeoLocation.LabelColumnName);

        PropertiesPanel.Instance.AddLabel("Filters");

        string[] filters = csvGeoLocation.GetFiltersByColumn();        

        //filter dropdown
        currentFilterDropdown = PropertiesPanel.Instance.AddActionDropdown(filters, (action) =>
        {
            Debug.Log($"label: {action}");
            foreach (Transform marker in LocationMarkersParent.transform)
            {
                var billboard = marker.GetComponent<Billboard>();

                if(action == "Toon alles") billboard.Show();
                else billboard.FilterOn(csvGeoLocation.LabelColumnIndex, action);
            }

        }, "");

        PropertiesPanel.Instance.AddSpacer(20);

        var row = csvGeoLocation.Rows[index];

        for (int i = 0; i < row.Length; i++)
        {
            var column = csvGeoLocation.Columns[i];
            var text = row[i];

            if (selectedColumnsToDisplay.ContainsKey(column) &&  selectedColumnsToDisplay[column])
            {
                PropertiesPanel.Instance.AddLabelColor(column, new Color(0,0.2705f,0.6f,1), FontStyle.Bold);

                if (text.StartsWith("http"))
                {
                    PropertiesPanel.Instance.AddLink(text, text);
                }
                else
                {                    
                    PropertiesPanel.Instance.AddTextfieldColor(text,Color.black, FontStyle.Italic);
                }
                PropertiesPanel.Instance.AddSeperatorLine();
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// For Editor testing only.
    /// This method loads a csv file.
    /// </summary>
    [ContextMenu("Open csv file selection dialog")]
    public void LoadCsvViaEditor()
    {
        if (!Application.isPlaying) return;
        
        string path = EditorUtility.OpenFilePanel("Selecteer .csv", "", "csv");
        if (path.Length != 0)
        {
            var csv = File.ReadAllText(path);
            ParseCsv(csv);
        }
    }

#endif
}
