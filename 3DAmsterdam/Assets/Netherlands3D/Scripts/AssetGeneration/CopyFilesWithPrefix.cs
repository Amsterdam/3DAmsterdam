using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CopyFilesWithPrefix : MonoBehaviour
{
    [SerializeField]
    private string sourceTilesAllowedPrefixesPath = "C:/Users/Sam/Desktop/Python/ids_file.txt";

    [SerializeField]
    private List<string> prefixes;

    [SerializeField]
    private string folder = "C:/Users/Sam/Desktop/lod12/";
    [SerializeField]
    private string targetCopyFolder = "C:/Users/Sam/Desktop/lod12/utrecht/";


    void Start()
    {
        Directory.CreateDirectory(targetCopyFolder);

        if (sourceTilesAllowedPrefixesPath != "" && File.Exists(sourceTilesAllowedPrefixesPath))
        {
            prefixes = File.ReadAllLines(sourceTilesAllowedPrefixesPath).ToList<string>();
        }

		for (int i = 0; i < prefixes.Count; i++)
		{
            var prefix = prefixes[i].Trim();
            if (prefix == "") continue;

            var filter = $"*_{prefix}.obj";
            Debug.Log($"Search: {filter}");

            string[] copyFiles = Directory.GetFiles(folder, filter);
            foreach (var filePath in copyFiles)
            {
                Debug.Log($"Copying {filePath}");

                File.Copy(filePath, filePath.Replace(folder, targetCopyFolder));
            }
        }

    }
}
