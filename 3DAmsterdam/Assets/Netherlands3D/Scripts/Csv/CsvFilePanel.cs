using ConvertCoordinates;
using Netherlands3D;
using Netherlands3D.Events;
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


    private Dictionary<string,bool> selectedColumnsToDisplay = new System.Collections.Generic.Dictionary<string,bool>();

    private CsvGeoLocation csvGeoLocation;

    private GameObject LocationMarkersParent;

    private ActionDropDown currentFilterDropdown;

    private void Awake()
    {
        ToggleActiveEvent.Subscribe(OnToggleActive);
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

        ServiceLocator.GetService<PropertiesPanel>().SetDynamicFieldsTargetContainer(GeneratedFieldsContainer);

        ServiceLocator.GetService<PropertiesPanel>().ClearGeneratedFields(UIClearIgnoreObject);

        csvGeoLocation = new CsvGeoLocation(csv);

        if (csvGeoLocation.Status != CsvGeoLocation.CsvGeoLocationStatus.Success)
        {
            ServiceLocator.GetService<PropertiesPanel>().AddSpacer(20);

            foreach (var line in csvGeoLocation.StatusMessageLines)
            {
                ServiceLocator.GetService<PropertiesPanel>().AddTextfieldColor(line, Color.red, FontStyle.Normal);
            }

            return;
        }
        
               
        ServiceLocator.GetService<PropertiesPanel>().AddLabel("Label");
        ServiceLocator.GetService<PropertiesPanel>().AddActionDropdown(csvGeoLocation.ColumnsExceptCoordinates, (action) =>
        {            
            csvGeoLocation.LabelColumnName = action;
            csvGeoLocation.SetlabelIndex(action);

        }, "");

        ServiceLocator.GetService<PropertiesPanel>().AddSpacer(10);
        ServiceLocator.GetService<PropertiesPanel>().AddLabel("Welke informatie wilt u zichtbaar maken als er op een label geklikt wordt?");
        ServiceLocator.GetService<PropertiesPanel>().AddSpacer(10);

        foreach (var column in csvGeoLocation.Columns)
        {
            if (csvGeoLocation.CoordinateColumns.Contains(column)) continue;

            selectedColumnsToDisplay.Add(column, true);
            ServiceLocator.GetService<PropertiesPanel>().AddActionCheckbox(column, true, (action) =>
            {
                selectedColumnsToDisplay[column] = action;
            });
        }

        ServiceLocator.GetService<PropertiesPanel>().AddActionButtonBig("Toon data", (action) =>
        {
            MapAndShow();               
        });

    }

    void MapAndShow()
    {
        ShowAll();

        ServiceLocator.GetService<PropertiesPanel>().ClearGeneratedFields(UIClearIgnoreObject);
        
        ServiceLocator.GetService<PropertiesPanel>().AddLabel($"CSV file geladen met {csvGeoLocation.Rows.Count} rijen");
        ServiceLocator.GetService<PropertiesPanel>().AddLabel("Klik op een icoon voor details");

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
                pos = ConvertCoordinates.CoordConvert.RDtoUnity(new Vector3RD(x, y, 7));
            }
            else
            {
                pos = ConvertCoordinates.CoordConvert.WGS84toUnity(new Vector3WGS(y, x, 7));
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
        ServiceLocator.GetService<PropertiesPanel>().ClearGeneratedFields(UIClearIgnoreObject);

        //dropdown to select the label of the pointer
        ServiceLocator.GetService<PropertiesPanel>().AddLabel("Label");
        ServiceLocator.GetService<PropertiesPanel>().AddActionDropdown(csvGeoLocation.ColumnsExceptCoordinates, (action) =>
        {
            Debug.Log($"label: {action}");
            csvGeoLocation.LabelColumnName = action;
            csvGeoLocation.SetlabelIndex(action);

            UpdateLabels();
            currentFilterDropdown.UpdateOptions(csvGeoLocation.GetFiltersByColumn());

        }, csvGeoLocation.LabelColumnName);

        ServiceLocator.GetService<PropertiesPanel>().AddLabel("Filters");

        string[] filters = csvGeoLocation.GetFiltersByColumn();        

        //filter dropdown
        currentFilterDropdown = ServiceLocator.GetService<PropertiesPanel>().AddActionDropdown(filters, (action) =>
        {
            Debug.Log($"label: {action}");
            foreach (Transform marker in LocationMarkersParent.transform)
            {
                var billboard = marker.GetComponent<Billboard>();

                if(action == "Toon alles") billboard.Show();
                else billboard.FilterOn(csvGeoLocation.LabelColumnIndex, action);
            }

        }, "");

        ServiceLocator.GetService<PropertiesPanel>().AddSpacer(20);

        var row = csvGeoLocation.Rows[index];

        for (int i = 0; i < row.Length; i++)
        {
            var column = csvGeoLocation.Columns[i];
            var text = row[i];

            if (selectedColumnsToDisplay.ContainsKey(column) &&  selectedColumnsToDisplay[column])
            {
                ServiceLocator.GetService<PropertiesPanel>().AddLabelColor(column, new Color(0,0.2705f,0.6f,1), FontStyle.Bold);

                if (text.StartsWith("http"))
                {
                    ServiceLocator.GetService<PropertiesPanel>().AddLink(text, text);
                }
                else
                {                    
                    ServiceLocator.GetService<PropertiesPanel>().AddTextfieldColor(text,Color.black, FontStyle.Italic);
                }
                ServiceLocator.GetService<PropertiesPanel>().AddSeperatorLine();
            }
        }
    }

    public void LoadCSVFromJavascript()
    {        
        var csv = JavascriptMethodCaller.FetchOBJDataAsString();     
        ParseCsv(csv);
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
