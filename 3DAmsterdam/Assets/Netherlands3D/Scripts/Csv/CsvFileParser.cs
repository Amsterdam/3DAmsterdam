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
using Netherlands3D.Core.Colors;

public class CsvFileParser : MonoBehaviour
{
    [SerializeField] private GameObject marker;
    [SerializeField] private Transform GeneratedFieldsContainer;
    [SerializeField] private GameObject UIClearIgnoreObject;

    private Dictionary<string,bool> selectedColumnsToDisplay = new Dictionary<string,bool>();

    private CsvColorsFinder csvColorsFinder;
    private CsvNumbersFinder csvNumbersFinder;
    private CsvGeoLocationFinder csvGeoLocationFinder;

    private GameObject LocationMarkersParent;

    private ActionDropDown currentFilterDropdown;

    [Header("Listeners")]
    [SerializeField] private IntEvent onSelectCSVContentType;
    [SerializeField] private TriggerEvent onClearData;

    [SerializeField]
    private StringEvent onFilesImported;

    [Header("Triggers")]
    [SerializeField] private ObjectEvent showColorsBasedOnIds;
    [SerializeField] private ObjectEvent showColorGradientBasedOnIds;
    [SerializeField] private FloatEvent setGradientValueMin;
    [SerializeField] private FloatEvent setGradientValueMax;
    [SerializeField] private GradientContainerEvent setGradient;


    [SerializeField] private FloatEvent setProgressBarPercentage;
    [SerializeField] private StringEvent setProgressBarMessage;
    [SerializeField] private StringEvent setProgressBarDetailedMessage;
    [SerializeField] private BoolEvent setEnableDrawingColors;
    [SerializeField] private BoolEvent activateCSVLayer;

    public string[] Columns;
    public List<string[]> Rows;

    private string columnWithNumbers = "";

    [SerializeField]
    private GradientsGroup gradientsGroup;

    public enum CSVContentFinderType
    {
        AUTODETECT,
        LOCATIONS,
        COLORS,
        NUMBERS
    }
    public CSVContentFinderType currentContentFinderType = CSVContentFinderType.AUTODETECT;

    private void Awake()
	{
		onFilesImported.AddListenerStarted(LoadCsvFromFile);

        onSelectCSVContentType.AddListenerStarted(SelectCSVContentType);
        
        onClearData.AddListenerStarted(Restart);
        activateCSVLayer.AddListenerStarted(ShowVisuals);
    }

	private void ShowVisuals(bool showData)
	{
        setEnableDrawingColors.InvokeStarted(showData);
        foreach (Transform marker in LocationMarkersParent.transform)
        {
            marker.gameObject.SetActive(showData);
        }
    }

	public void SelectCSVContentType(int type)
	{
        currentContentFinderType = (CSVContentFinderType)type;
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

        setProgressBarPercentage.InvokeStarted(0);
        setProgressBarMessage.InvokeStarted("CSV wordt uitgelezen..");
        setProgressBarDetailedMessage.InvokeStarted("");

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

        setProgressBarPercentage.InvokeStarted(100);

        // Seperate first line containing columns
        Columns = Rows[0];
        Rows.RemoveAt(0);

        //Clear UI and start finder type
        PropertiesPanel.Instance.SetDynamicFieldsTargetContainer(GeneratedFieldsContainer);
        PropertiesPanel.Instance.ClearGeneratedFields(UIClearIgnoreObject);
        UIClearIgnoreObject.gameObject.SetActive(false);
        
        StartFinderType();
    }

    private void Restart()
    {
        CleanUp();

        //Stop drawing colors
        setEnableDrawingColors.InvokeStarted(false);

        //Clear finders from memory
        csvColorsFinder = null;
        csvGeoLocationFinder = null;
        csvNumbersFinder = null;

        UIClearIgnoreObject.gameObject.SetActive(true);
        PropertiesPanel.Instance.ClearGeneratedFields(UIClearIgnoreObject);
    }

    void StartFinderType()
    {
		switch (currentContentFinderType)
		{
			case CSVContentFinderType.AUTODETECT:
                AutoDetectCSVContent();
                break;
			case CSVContentFinderType.LOCATIONS:
                FindLocationBasedContent();
                DrawStatusMessages();
                break;
			case CSVContentFinderType.COLORS:
                FindColorBasedContent();
                DrawStatusMessages();
                break;
			case CSVContentFinderType.NUMBERS:
                FindNumbersBasedContent();
                DrawStatusMessages();
                break;
			default:
				break;
		}
	}

    private void ReportProgress(int lineNr, int ofTotal)
    {
        setProgressBarPercentage.InvokeStarted(((float)lineNr / (float)ofTotal) * 100.0f);
        setProgressBarDetailedMessage.InvokeStarted($"Regels gelezen: {lineNr}/{ofTotal}");
    }

    public void AutoDetectCSVContent()
	{
        //First check for geolocation based content
        if (FindLocationBasedContent()) return;

        //Second, check for data with explicity defined hex colors
        if (FindColorBasedContent()) return;

        //Last, check for numbers we might be able to map to a gradient
        if (FindNumbersBasedContent()) return;

        //In case of failure, show all messages
        DrawStatusMessages();
	}

    private bool FindColorBasedContent()
    {
        csvColorsFinder = new CsvColorsFinder(Columns, Rows);
        if (csvColorsFinder.Status == CsvContentFinder.CsvContentFinderStatus.Success)
        {
            ShowColorToIDMappingOptions();
            return true;
        }
        return false;
    }

    private bool FindNumbersBasedContent()
    {
        csvNumbersFinder = new CsvNumbersFinder(Columns, Rows);
        if (csvNumbersFinder.Status == CsvContentFinder.CsvContentFinderStatus.Success)
        {
            ShowGradientToIDMappingOptions();
            return true;
        }
        return false;
    }

    private bool FindLocationBasedContent()
    {
        csvGeoLocationFinder = new CsvGeoLocationFinder(Columns, Rows);
        if (csvGeoLocationFinder.Status == CsvContentFinder.CsvContentFinderStatus.Success)
        {
            ShowLocationBasedOptions();
            return true;
        }
        return false;
    }


    private void DrawStatusMessages()
	{
        var showTryAgainButton = false;

        PropertiesPanel.Instance.AddSpacer(20);
        if(csvGeoLocationFinder != null){ 
		    foreach (var line in csvGeoLocationFinder.StatusMessageLines)
		    {
			    PropertiesPanel.Instance.AddTextfield(line);
                showTryAgainButton = true;
            }
        }
        PropertiesPanel.Instance.AddSpacer(20);
        if (csvColorsFinder != null)
        {
            foreach (var line in csvColorsFinder.StatusMessageLines)
            {
                PropertiesPanel.Instance.AddTextfield(line);
                showTryAgainButton = true;
            }
        }
        PropertiesPanel.Instance.AddSpacer(20);
        if (csvNumbersFinder != null)
        {
            foreach (var line in csvNumbersFinder.StatusMessageLines)
            {
                PropertiesPanel.Instance.AddTextfield(line);
                showTryAgainButton = true;
            }
        }

        if(showTryAgainButton)
        {
            PropertiesPanel.Instance.AddActionButtonBig("Opnieuw proberen", (action) =>
            {
                Restart();
            });
        }
    }

	private void ShowColorToIDMappingOptions()
    {
        activateCSVLayer.InvokeStarted(true);

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

        PropertiesPanel.Instance.AddActionButtonBig("Toepassen", (action) =>
        {
            ShowColors();
        });
    }

    private void ShowGradientToIDMappingOptions()
    {
        activateCSVLayer.InvokeStarted(true);

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

        //Dropdown with the column to use.
        if (columnWithNumbers == "")
        {
            columnWithNumbers = csvNumbersFinder.GetSecondaryNumberColumn();
        }
        csvNumbersFinder.SetNumberColumn(columnWithNumbers);
        PropertiesPanel.Instance.AddActionDropdown(columnsWithNumbers.ToArray(), (action) =>
        {
            columnWithNumbers = action;
            csvNumbersFinder.SetNumberColumn(action);
            //Redraw UI to update number fields
            PropertiesPanel.Instance.ClearGeneratedFields(UIClearIgnoreObject);
            ShowGradientToIDMappingOptions();

        }, columnWithNumbers);

        //Choose ranges
        PropertiesPanel.Instance.AddLabel("Bereik:");
        var inputFieldMin = PropertiesPanel.Instance.AddNumberInput("Minimaal:", csvNumbersFinder.GetMinNumberValue());
        var inputFieldMax = PropertiesPanel.Instance.AddNumberInput("Maximaal:", csvNumbersFinder.GetMaxNumberValue());

        //Choose type of gradient
        PropertiesPanel.Instance.AddLabel("Pas kleurverloop toe:");
        foreach (var gradientContainer in gradientsGroup.gradientContainers)
        {
            Button gradientButton = PropertiesPanel.Instance.AddGradientButton(gradientContainer.name, gradientContainer);
            gradientButton.onClick.AddListener(() => {
                setGradient.InvokeStarted(gradientContainer);

                double.TryParse(inputFieldMin.text, out double min);
                double.TryParse(inputFieldMax.text, out double max);
                
                ShowGradientColors(min, max);
            });
        }


    }

    private void ShowLocationBasedOptions()
    {
        activateCSVLayer.InvokeStarted(true);

        PropertiesPanel.Instance.AddLabel("Label");
        PropertiesPanel.Instance.AddActionDropdown(csvGeoLocationFinder.ColumnsExceptCoordinates, (action) =>
        {
            csvGeoLocationFinder.LabelColumnName = action;
            csvGeoLocationFinder.SetlabelIndex(action);
        }, "");

        PropertiesPanel.Instance.AddSpacer(10);
        PropertiesPanel.Instance.AddLabel("Welke informatie wilt u zichtbaar maken als er op een label geklikt wordt?");
        PropertiesPanel.Instance.AddSpacer(10);

        foreach (var column in csvGeoLocationFinder.Columns)
        {
            if (csvGeoLocationFinder.CoordinateColumns.Contains(column)) continue;

            selectedColumnsToDisplay.Add(column, true);
            PropertiesPanel.Instance.AddActionCheckbox(column, true, (action) =>
            {
                selectedColumnsToDisplay[column] = action;
            });
        }

        PropertiesPanel.Instance.AddActionButtonBig("Toon data", (action) =>
        {
            MapAndShowLocations();
        });
    }

    private void ShowColors()
    {
        var colorsAndIDs = csvColorsFinder.GetColorsAndIDs();
        showColorsBasedOnIds.InvokeStarted(colorsAndIDs);
    }

    private void ShowGradientColors(double min, double max)
    {
        var colorsAndNumbers = csvNumbersFinder.GetNumbersAndIDs();

        setGradientValueMin.InvokeStarted((float)min);
        setGradientValueMax.InvokeStarted((float)max);

        showColorGradientBasedOnIds.InvokeStarted(colorsAndNumbers);
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
			ResetSelections();
		}
	}

	void MapAndShowLocations()
    {
        ShowAllLocations();

        PropertiesPanel.Instance.ClearGeneratedFields(UIClearIgnoreObject);
        PropertiesPanel.Instance.AddActionButtonText("<Terug", (action) =>
        {
            CleanUp();
            PropertiesPanel.Instance.ClearGeneratedFields(UIClearIgnoreObject);
            ShowLocationBasedOptions();
        });
        PropertiesPanel.Instance.AddLabel($"CSV file geladen met {csvGeoLocationFinder.Rows.Count} rijen");
        PropertiesPanel.Instance.AddLabel("Klik op een icoon voor details");
    }

    private void ResetSelections()
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

    void ShowAllLocations()
    {
        var firstrow = csvGeoLocationFinder.Rows[0];
        double firstrow_x = double.Parse(firstrow[csvGeoLocationFinder.XColumnIndex]);
        bool isRd = csvGeoLocationFinder.IsRd(firstrow_x);

        for (int rowindex = 0; rowindex < csvGeoLocationFinder.Rows.Count; rowindex++)
		{
			var row = csvGeoLocationFinder.Rows[rowindex];
			AddMarker(isRd, rowindex, row);
		}
	}

	private void AddMarker(bool isRd, int rowindex, string[] row)
	{
		var locationMarker = Instantiate(marker, LocationMarkersParent.transform);
		var billboard = locationMarker.GetComponent<Billboard>();
		var textmesh = locationMarker.GetComponentInChildren<TextMesh>();
		textmesh.text = row[csvGeoLocationFinder.LabelColumnIndex];
		labels.Add(textmesh);

		billboard.Index = rowindex;
		billboard.Row = row;
		billboard.ClickAction = (action =>
		{
			ShowPositionDetails(action);
		});

		double.TryParse(row[csvGeoLocationFinder.XColumnIndex], out double x);
		double.TryParse(row[csvGeoLocationFinder.YColumnIndex], out double y);

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

	private void UpdateLabels()
    {
        for (int i = 0; i < csvGeoLocationFinder.Rows.Count; i++) 
        {
            var row = csvGeoLocationFinder.Rows[i];
            labels[i].text = row[csvGeoLocationFinder.LabelColumnIndex];
        }
     }

    void ShowPositionDetails(int index)
    {
        PropertiesPanel.Instance.ClearGeneratedFields(UIClearIgnoreObject);
        PropertiesPanel.Instance.AddActionButtonText("<Terug", (action) =>
        {
            CleanUp();
            PropertiesPanel.Instance.ClearGeneratedFields(UIClearIgnoreObject);
            ShowLocationBasedOptions();
        });

        //dropdown to select the label of the pointer
        PropertiesPanel.Instance.AddLabel("Label");
        PropertiesPanel.Instance.AddActionDropdown(csvGeoLocationFinder.ColumnsExceptCoordinates, (action) =>
        {
            Debug.Log($"label: {action}");
            csvGeoLocationFinder.LabelColumnName = action;
            csvGeoLocationFinder.SetlabelIndex(action);

            UpdateLabels();
            currentFilterDropdown.UpdateOptions(csvGeoLocationFinder.GetFiltersByColumn());

        }, csvGeoLocationFinder.LabelColumnName);

        PropertiesPanel.Instance.AddLabel("Filters");

        string[] filters = csvGeoLocationFinder.GetFiltersByColumn();        

        //filter dropdown
        currentFilterDropdown = PropertiesPanel.Instance.AddActionDropdown(filters, (action) =>
        {
            Debug.Log($"label: {action}");
            foreach (Transform marker in LocationMarkersParent.transform)
            {
                var billboard = marker.GetComponent<Billboard>();

                if(action == "Toon alles") billboard.Show();
                else billboard.FilterOn(csvGeoLocationFinder.LabelColumnIndex, action);
            }

        }, "");

        PropertiesPanel.Instance.AddSpacer(20);
        var row = csvGeoLocationFinder.Rows[index];

        for (int i = 0; i < row.Length; i++)
        {
            var column = csvGeoLocationFinder.Columns[i];
            var text = row[i];

            if (selectedColumnsToDisplay.ContainsKey(column) &&  selectedColumnsToDisplay[column])
            {
                PropertiesPanel.Instance.AddLabelColor(column, new Color(0,0.2705f,0.6f,1), TMPro.FontStyles.Bold);

                if (text.StartsWith("http"))
                {
                    PropertiesPanel.Instance.AddLink(text, text);
                }
                else
                {                    
                    PropertiesPanel.Instance.AddTextfieldColor(text,Color.black, TMPro.FontStyles.Italic);
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
