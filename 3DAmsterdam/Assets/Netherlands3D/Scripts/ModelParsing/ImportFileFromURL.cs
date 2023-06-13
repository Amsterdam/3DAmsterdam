using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public static class ImportFileFromURL
{

    [DllImport("__Internal")]
    private static extern void ImportFromURL(string url, string filename);

    public static void Import(string url)
    {
        var filenameWithExtention = Path.GetFileName(url);
        ImportFromURL(url, filenameWithExtention);
    }
}
