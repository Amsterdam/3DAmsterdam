using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using SimpleFileBrowser;

public class DownloadGebouw : MonoBehaviour
{
    public void StartDownloadBrowser()
    {
        StartCoroutine(WaitForSaveDialog());
    }

    public static IEnumerator WaitForSaveDialog(bool folderMode = false, string initialPath = null, string title = "Save", string saveButtonText = "Save")
    {
        yield return FileBrowser.WaitForSaveDialog(false, null, "Save", "Save");
    }
}
