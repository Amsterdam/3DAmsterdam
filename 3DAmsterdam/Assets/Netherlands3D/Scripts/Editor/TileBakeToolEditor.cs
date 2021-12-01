using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;

public class TileBakeToolEditor : EditorWindow
{
	private Vector2 scrollPosition;
	private static string sourcePath = "C:/CityJSONS/";
    private static string outputPath = "C:/CityJSONS/BinaryOutput/";

    private static string overridesPath = "";

    private static string identifier = "identificatie";
    private static string filterType = "Buildings";
    private static string lod = "2.2";
    private static string removeFromID = "NL.IMBAG.Pand.";

	private static bool replace = true;
	private static bool brotli = true;

    private static string relativeToolPath = "/../../TileBakeTool/TileBakeTool/bin/Release/net5.0/TileBakeTool.exe";

    private static string windowTitle = "Tile Bake Tool";

    [MenuItem("Netherlands 3D/Tile Bake Tool")]
    static void Init()
    {
        TileBakeToolEditor window = (TileBakeToolEditor)EditorWindow.GetWindow(typeof(TileBakeToolEditor), false, windowTitle);

        sourcePath = PlayerPrefs.GetString($"{windowTitle}sourcePath");
        outputPath = PlayerPrefs.GetString($"{windowTitle}outputPath");
        overridesPath = PlayerPrefs.GetString($"{windowTitle}overridesPath");
        identifier = PlayerPrefs.GetString($"{windowTitle}identifier");
        filterType = PlayerPrefs.GetString($"{windowTitle}filterType");
        lod = PlayerPrefs.GetString($"{windowTitle}lod");
        removeFromID = PlayerPrefs.GetString($"{windowTitle}removeFromID");

        replace = PlayerPrefs.GetInt($"{windowTitle}replace") != 0;
        brotli = PlayerPrefs.GetInt($"{windowTitle}brotli") != 0;

        window.Show();
    }

    private void SavePreferences(){
        PlayerPrefs.SetString($"{windowTitle}sourcePath", sourcePath);
        PlayerPrefs.SetString($"{windowTitle}outputPath", outputPath);
        PlayerPrefs.SetString($"{windowTitle}overridesPath", overridesPath);
        PlayerPrefs.SetString($"{windowTitle}identifier", identifier);
        PlayerPrefs.SetString($"{windowTitle}filterType", filterType);
        PlayerPrefs.SetString($"{windowTitle}lod", lod);
        PlayerPrefs.SetString($"{windowTitle}removeFromID", removeFromID);
        
        PlayerPrefs.SetInt($"{windowTitle}replace", (replace) ? 1 : 0);
        PlayerPrefs.SetInt($"{windowTitle}brotli", (brotli) ? 1 : 0);
    }

    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        GUILayout.Label("Input/Output", EditorStyles.boldLabel);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        sourcePath = EditorGUILayout.TextField("Source CityJSONs input path", sourcePath);
        if (GUILayout.Button("Select source folder"))
        {
            GUI.FocusControl(null);
            sourcePath = EditorUtility.OpenFolderPanel("Select the folder containing the main CityJSON (.json) files", "", "") + "/";
            if (outputPath == "") outputPath = sourcePath + "/BinaryTiles/";
        }
        EditorGUILayout.Space();

        outputPath = EditorGUILayout.TextField("Output path", outputPath);
        if (GUILayout.Button("Select output folder"))
        {
            GUI.FocusControl(null);
            outputPath = EditorUtility.OpenFolderPanel("Select the folder where the binary tile (.bin) files should be written", "", "") + "/";
        }
        EditorGUILayout.Space();

        overridesPath = EditorGUILayout.TextField("(Optional) Overrides path", overridesPath);
        if (GUILayout.Button("Select optional overrides folder"))
        {
            GUI.FocusControl(null);
            overridesPath = EditorUtility.OpenFolderPanel("Select a folder containing CityJSON override files", "", "") + "/";
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.Label("Filtering", EditorStyles.boldLabel);
        identifier = EditorGUILayout.TextField("Identifier", identifier);
        lod = EditorGUILayout.TextField("LOD filter", lod);
        filterType = EditorGUILayout.TextField("Filter type", filterType);
        removeFromID = EditorGUILayout.TextField("Remove from ID", removeFromID);
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.Label("Options", EditorStyles.boldLabel);
        replace = EditorGUILayout.Toggle("Replace same ID's", replace);
        brotli = EditorGUILayout.Toggle("Brotli compress", brotli);

        if (EditorGUI.EndChangeCheck())
        {
            SavePreferences();
        }

        EditorGUILayout.Space();

        GUILayout.EndScrollView();

        GUILayout.FlexibleSpace();
        GUILayout.Label(Path.GetFullPath(Application.dataPath + relativeToolPath), EditorStyles.boldLabel);
        if (GUILayout.Button("Bake",GUILayout.Height(100)))
		{
            Debug.Log("Bake!");
            StartBakeTool();
        }
    }

    private void StartBakeTool()
    {
        Debug.Log("<color=#00FF00>Starting bake tool with the following parameters:</color>");
        Debug.Log("<color=#00FF00>" + Path.GetFullPath(Application.dataPath + relativeToolPath) + "</color><color=#00FFFF> " + DrawArguments() + "</color>");
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
        startInfo.FileName = Path.GetFullPath(Application.dataPath + relativeToolPath);
        startInfo.Arguments = DrawArguments();
        process.StartInfo = startInfo;
        process.Start();
    }

    private string DrawArguments()
    {
        StringBuilder argumentsBuilder = new StringBuilder();
        argumentsBuilder.Append($"--source \"{sourcePath}\" ");
        argumentsBuilder.Append($"--output \"{outputPath}\" ");
        argumentsBuilder.Append($"--id \"{identifier}\" ");
        argumentsBuilder.Append($"--lod \"{lod}\" ");
        argumentsBuilder.Append($"--filter \"{filterType}\" ");
        argumentsBuilder.Append($"--id-remove \"{removeFromID}\" ");

        if(replace) argumentsBuilder.Append($"--replace ");
        if(brotli) argumentsBuilder.Append($"--brotli ");

        return argumentsBuilder.ToString();
    }
}