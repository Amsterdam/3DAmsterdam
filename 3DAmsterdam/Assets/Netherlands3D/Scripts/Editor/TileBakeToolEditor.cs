using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;

public class TileBakeToolEditor : EditorWindow
{
	private Vector2 scrollPosition;
	private static string mainSourcePath = "C:/CityJSONS/";
    private static string outputPath = "C:/CityJSONS/BinaryOutput/";

	private static string sourcePath = "";

    private static string overridesPath = "";

    private static string identifier = "identificatie";
    private static string filterType = "Buildings";
    private static string lod = "2.2";
    private static string removeFromID = "NL.IMBAG.Pand.";

	private static bool replace = true;
	private static bool brotli = true;

    private static string relativeToolPath = "/../../TileBakeTool/TileBakeTool/bin/Release/net5.0/TileBakeTool.exe";

    private static bool baking = false;

    private static string windowTitle = "Tile Bake Tool";

    //[MenuItem("Netherlands3D/Tile Bake Tool")]
    static void Init()
    {
        TileBakeToolEditor window = (TileBakeToolEditor)EditorWindow.GetWindow(typeof(TileBakeToolEditor), false, windowTitle);

        mainSourcePath = PlayerPrefs.GetString($"{windowTitle}sourcePath");
        outputPath = PlayerPrefs.GetString($"{windowTitle}outputPath");
        overridesPath = PlayerPrefs.GetString($"{windowTitle}overridesPath");
        identifier = PlayerPrefs.GetString($"{windowTitle}identifier");
        filterType = PlayerPrefs.GetString($"{windowTitle}filterType");
        lod = PlayerPrefs.GetString($"{windowTitle}lod");
        removeFromID = PlayerPrefs.GetString($"{windowTitle}removeFromID");

        replace = PlayerPrefs.GetInt($"{windowTitle}replace") != 0;
        brotli = PlayerPrefs.GetInt($"{windowTitle}brotli") != 0;

        baking = false;

        window.Show();
    }

    private void SavePreferences(){
        PlayerPrefs.SetString($"{windowTitle}sourcePath", mainSourcePath);
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

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        if (!mainSourcePath.EndsWith("/") || mainSourcePath.EndsWith("\\")) mainSourcePath += "/";
        if (!outputPath.EndsWith("/") || outputPath.EndsWith("\\")) outputPath += "/";
        if (!overridesPath.EndsWith("/") || overridesPath.EndsWith("\\")) overridesPath += "/";

        GUILayout.Label("Input folder with CityJSON files", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        mainSourcePath = EditorGUILayout.TextField(mainSourcePath);
        if (GUILayout.Button("Browse", GUILayout.Width(100)))
        {
            GUI.FocusControl(null);
            mainSourcePath = EditorUtility.OpenFolderPanel("Select the folder containing the main CityJSON (.json) files", mainSourcePath, "");
            if (outputPath == "") outputPath = mainSourcePath + "/BinaryTiles/";
        }
        GUILayout.EndHorizontal();
 
        EditorGUILayout.Space();

        GUILayout.Label("Output folder for generated binary tile files", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(100)))
        {
            GUI.FocusControl(null);
            outputPath = EditorUtility.OpenFolderPanel("Select the folder where you want to store the binary tile output files", outputPath, "");
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUILayout.Label("(Optional) folder with CityJSON override files", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        overridesPath = EditorGUILayout.TextField(overridesPath);
        if (GUILayout.Button("Browse", GUILayout.Width(100)))
        {
            GUI.FocusControl(null);
            overridesPath = EditorUtility.OpenFolderPanel("Select a folder containing CityJSON override files", overridesPath, "");
        }
        GUILayout.EndHorizontal();


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
        if (baking)
        {
            GUI.enabled = false;
            GUILayout.Button("Baking...", GUILayout.Height(100));            
            GUI.enabled = true;
        }
        else
        {
            if (GUILayout.Button("Bake", GUILayout.Height(100)))
            {
                Debug.Log("Bake!");
                baking = true;
                bool overrides = (overridesPath.Length > 3);
                StartBakeTool(mainSourcePath, overrides);
            }
        }
    }

    private void StartBakeTool(string source, bool checkOverrides = false)
    {
        sourcePath = source;

        Debug.Log("<color=#00FF00>Starting bake tool with the following parameters:</color>");
        Debug.Log("<color=#00FF00>" + Path.GetFullPath(Application.dataPath + relativeToolPath) + "</color><color=#00FFFF> " + DrawArguments() + "</color>");
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
        startInfo.FileName = Path.GetFullPath(Application.dataPath + relativeToolPath);
        startInfo.Arguments = DrawArguments();
        process.StartInfo = startInfo;
        process.EnableRaisingEvents = true;
        process.Start();
        baking = true;
        process.WaitForExit();

        EndedBaking(checkOverrides);
    }

    private void EndedBaking(bool checkOverrides = false)
    {
        if (checkOverrides)
        {
            Debug.Log("Baking override files..");     
            StartBakeTool(overridesPath);
        }
        else
        {
            Debug.Log("Bake Tool has ended.");
            baking = false;
            Application.OpenURL("file://" + outputPath); //Open explorer with output path
        }
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