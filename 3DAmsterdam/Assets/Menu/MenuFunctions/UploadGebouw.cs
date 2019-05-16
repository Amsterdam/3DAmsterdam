using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using SimpleFileBrowser;

public class UploadGebouw : MonoBehaviour
{

    public void StartFileBrowser()
    {
        StartCoroutine(WaitForLoadDialog());
    }

    public static IEnumerator WaitForLoadDialog(bool folderMode = false, string initialPath = null, string title = "Load", string loadButtonText = "Select")
    {
        yield return FileBrowser.WaitForLoadDialog(false, null, "Load File", "Load");
    }
}
