using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using SimpleFileBrowser;

public class DownloadGebied : MonoBehaviour
{
    bool isActive = false;

    public void StartFileBrowser()
    {
        StartCoroutine(WaitForLoadDialog());
    }

    public static IEnumerator WaitForLoadDialog(bool folderMode = false, string initialPath = null, string title = "Load", string loadButtonText = "Select")
    {
        yield return FileBrowser.WaitForLoadDialog(false, null, "Load File", "Load");
    }

    public void ToggleActivation()
    {
        isActive = !isActive;
    }

    private void Update()
    {
        if (!isActive)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, 10000, LayerMask.GetMask("Panden")))
            return;

        var tileIdStr = hit.collider.gameObject.name.Split('_');
        if (tileIdStr.Length != 3)
            return;

        int x, y, z;
        if (!int.TryParse(tileIdStr[0], out x)) return;
        if (!int.TryParse(tileIdStr[1], out y)) return;
        if (!int.TryParse(tileIdStr[2], out z)) return;

        List<Vector3> tileIds = new List<Vector3>();
        tileIds.Add(new Vector3(x, y, z));
        TileSaver.Save2(tileIds.ToArray());
        
    }
}
