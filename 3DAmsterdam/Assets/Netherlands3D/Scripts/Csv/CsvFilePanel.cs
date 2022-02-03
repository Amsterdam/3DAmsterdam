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

    private Dictionary<string,bool> selectedColumnsToDisplay = new Dictionary<string,bool>();

    private CsvColorsFinder csvColorsFinder;
    private CsvNumbersFinder csvNumbersFinder;
    private CsvGeoLocationFinder csvGeoLocation;

    private GameObject LocationMarkersParent;

    private ActionDropDown currentFilterDropdown;

    [Header("Listeners")]
    [SerializeField]
    private BoolEvent onToggleBetaFeatures;

    [SerializeField]
    private StringEvent onFilesImported;

    [Header("Triggers")]
    [SerializeField]
    private ObjectEvent showColorsBasedOnIds;
    [SerializeField]
    private ObjectEvent showColorGradientBasedOnIds;
    [SerializeField]
    private FloatEvent setGradientValueMin;
    [SerializeField]
    private FloatEvent setGradientValueMax;

    [SerializeField]
    private FloatEvent setProgressBarPercentage;
    [SerializeField]
    private StringEvent setProgressBarMessage;
    [SerializeField]
    private StringEvent setProgressBarDetailedMessage;

    public string[] Columns;
    public List<string[]> Rows;

    private void Awake()
    {
        onToggleBetaFeatures.started.AddListener(Show);
        onFilesImported.started.AddListener(LoadCsvFromFile);
    }

    public void LoadCsvFromFile(string filename)
    {
        if (!filename.EndsWith(".csv") && !filename.EndsWith(".CSV"))
            return;

        string filePath;
        if (Path.IsPathRooted(filename))
        {
            filePath = filename;
        }
        else{
            filePath = Application.persistentDataPath + "/" + filename;
        }

        CleanUp();

        setProgressBarPercentage.started.Invoke(0);
        setProgressBarMessage.started.Invoke("CSV wordt uitgelezen..");
        setProgressBarDetailedMessage.started.Invoke("");

        StartCoroutine(ReadColumnsAndRows(filePath));
    }

    private IEnumerator ReadColumnsAndRows(string filePath)
    {
        yield return new WaitForEndOfFrame();

        var lines = 0;
        int maxLinesPerFrame = 10000;
        yield return CsvParser.StreamReadLines(
            filePath, 0, maxLinesPerFrame,
            (totalLines) => { lines = totalLines; },
            (currentLine) => { ReportProgress(currentLine, lines); },
            (rows) => { Rows = rows; }
        );

        setProgressBarPercentage.started.Invoke(100);

        // Seperate first line containing columns
        Columns = Rows[0];
        Rows.RemoveAt(0);

        //Not determine what kind of content we are dealing with
        DetermineCSVContent();
    }

    private void ReportProgress(int lineNr, int ofTotal)
    {
        setProgressBarPercentage.started.Invoke(((float)lineNr / (float)ofTotal) * 100.0f);
        setProgressBarDetailedMessage.started.Invoke($"Regels gelezen: {lineNr}/{ofTotal}");
    }

    private void Show(bool active)
    {
        gameObject.SetActive(active);
    }

    public void DetermineCSVContent()
	{
		PropertiesPanel.Instance.SetDynamicFieldsTargetContainer(GeneratedFieldsContainer);
		PropertiesPanel.Instance.ClearGeneratedFields(UIClearIgnoreObject);

		//First check for geolocation based content
		csvGeoLocation = new CsvGeoLocationFinder(Columns, Rows);
		if (csvGeoLocation.Status == CsvContentFinder.CsvContentFinderStatus.Success)
		{
			ShowLocationBasedOptions();
			return;
		}

		//Second, check for data with explicity defined hex colors
		csvColorsFinder = new CsvColorsFinder(Columns, Rows);
		if (csvColorsFinder.Status == CsvContentFinder.CsvContentFinderStatus.Success)
		{
			ShowColorToIDMappingOptions();
			return;
		}

		//Last, check for numbers we might be able to map to a gradient
		csvNumbersFinder = new CsvNumbersFinder(Columns, Rows);
		if (csvNumbersFinder.Status == CsvContentFinder.CsvContentFinderStatus.Success)
		{
			ShowGradientToIDMappingOptions();
			return;
		}

		//In case of failure, show all messages
		DrawStatusMessages();
	}

	private void DrawStatusMessages()
	{
		PropertiesPanel.Instance.AddSpacer(20);
		foreach (var line in csvGeoLocation.StatusMessageLines)
		{
			PropertiesPanel.Instance.AddTextfieldColor(line, Color.red, FontStyle.Normal);
		}
		PropertiesPanel.Instance.AddSpacer(20);
		foreach (var line in csvColorsFinder.StatusMessageLines)
		{
			PropertiesPanel.Instance.AddTextfieldColor(line, Color.red, FontStyle.Normal);
		}
	}

	private void ShowColorToIDMappingOptions()
    {
        PropertiesPanel.Instance.AddLabel("BAG ID kolom:");
        List<string> columnsWithIDs = new List<string>();
        for (int i = 0; i < csvColorsFinder.IDColumnIndices.Count; i++)
        {
            var index = csvColorsFinder.IDColumnIndices[i];
            columnsWithIDs.Add(Columns[index]);
        }
        csvColorsFinder.SetIDColumn(columnsWithIDs[0]);
        PropertiesPanel.Instance.AddActionDropdown(columnsWithIDs.ToArray(), (action) =>
        {
            csvColorsFinder.SetIDColumn(columnsWithIDs[0]);
        }, columnsWithIDs[0]);

        PropertiesPanel.Instance.AddLabel("Kleuren kolom:");
        List<string> columnsWithColors = new List<string>();
		for (int i = 0; i < csvColorsFinder.ColorColumnIndices.Count; i++)
		{
            var index = csvColorsFinder.ColorColumnIndices[i];
            columnsWithColors.Add(Columns[index]);
        }
        csvColorsFinder.SetColorColumn(columnsWithColors[0]);
        PropertiesPanel.Instance.AddActionDropdown(columnsWithColors.ToArray(), (action) =>
        {
            csvColorsFinder.SetColorColumn(action);
        }, columnsWithColors[0]);

        PropertiesPanel.Instance.AddActionButtonBig("Toon data", (action) =>
        {
            ShowColors();
        });
    }

    private void ShowGradientToIDMappingOptions()
    {
        PropertiesPanel.Instance.AddLabel("BAG ID kolom:");
        List<string> columnsWithIDs = new List<string>();
        for (int i = 0; i < csvNumbersFinder.IDColumnIndices.Count; i++)
        {
            var index = csvNumbersFinder.IDColumnIndices[i];
            columnsWithIDs.Add(Columns[index]);
        }
        csvNumbersFinder.SetIDColumn(columnsWithIDs[0]);

        PropertiesPanel.Instance.AddActionDropdown(columnsWithIDs.ToArray(), (action) =>
        {
            csvNumbersFinder.SetIDColumn(columnsWithIDs[0]);
        }, columnsWithIDs[0]);

        PropertiesPanel.Instance.AddLabel("Kleurbepalende waarde:");
        List<string> columnsWithNumbers = new List<string>();
        for (int i = 0; i < csvNumbersFinder.NumberColumnIndices.Count; i++)
        {
            var index = csvNumbersFinder.NumberColumnIndices[i];
            columnsWithNumbers.Add(Columns[index]);
        }
        csvNumbersFinder.SetNumberColumn(columnsWithNumbers[0]);

        PropertiesPanel.Instance.AddActionDropdown(columnsWithNumbers.ToArray(), (action) =>
        {
            csvNumbersFinder.SetNumberColumn(action);
        }, columnsWithNumbers[0]);

        //Choose ranges
        PropertiesPanel.Instance.AddLabel("Bereik:");
        var inputFieldMin = PropertiesPanel.Instance.AddNumberInput("Minimaal:", 0);
        var inputFieldMax = PropertiesPanel.Instance.AddNumberInput("Maximaal:", 10);

        PropertiesPanel.Instance.AddActionButtonBig("Toon data", (action) =>
        {
            double.TryParse(inputFieldMin.text, out double min);
            double.TryParse(inputFieldMax.text, out double max);
            ShowGradientColors(min, max);
        });
    }

    private void ShowColors()
    {
        var colorsAndIDs = csvColorsFinder.GetColorsAndIDs();
        showColorsBasedOnIds.started.Invoke(colorsAndIDs);
    }

    private void ShowGradientColors(double min, double max)
    {
        var colorsAndNumbers = csvNumbersFinder.GetNumbersAndIDs();

        setGradientValueMin.started.Invoke((float)min);
        setGradientValueMax.started.Invoke((float)max);

        showColorGradientBasedOnIds.started.Invoke(colorsAndNumbers);
    }

    private void ShowLocationBasedOptions()
	{
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

	private void CleanUp()
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
            StartCoroutine(ReadColumnsAndRows(path));
        }
    }

#endif
}
