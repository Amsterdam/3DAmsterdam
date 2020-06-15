using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class DownloadGebied : MonoBehaviour
{
    Vector3 startPosition;
    UILineRenderer lineRenderer;
    Bounds lastBounds;
    public CameraControls camcontroller;
    

    enum State
    {
        Idle,
        Selecting,
        Uploading
    }
    State state = State.Idle;


    private void Start()
    {
        
        lineRenderer = GetComponent<UILineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<UILineRenderer>();
        
    }

    void StartDrawingLine()
    {
        startPosition = Input.mousePosition;
        lineRenderer.enabled = true;
    }

    void PreviewLine()
    {
        UILineRenderer lr = lineRenderer;
        Vector2 upLeft, rightUp, rightDown, leftDown;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(lr.rectTransform, startPosition, lr.canvas.worldCamera, out upLeft);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(lr.rectTransform, new Vector2(Input.mousePosition.x, startPosition.y), lr.canvas.worldCamera, out rightUp);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(lr.rectTransform, new Vector2(startPosition.x, Input.mousePosition.y), lr.canvas.worldCamera, out leftDown);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(lr.rectTransform, Input.mousePosition, lr.canvas.worldCamera, out rightDown);
        lr.Points = new Vector2[] { upLeft, rightUp, rightDown, leftDown, upLeft };
    }

    void EndDrawingLine()
    {
        lineRenderer.Points = null;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        switch ( state)
        {
            case State.Idle:
                if (Input.GetMouseButtonDown(0))
                {
                    camcontroller.LockFunctions = true;
                    StartDrawingLine();
                    PreviewLine();
                    state = State.Selecting;
                }
                break;

            case State.Selecting:
                PreviewLine();
                if (Input.GetMouseButtonUp(0))
                {
                    EndDrawingLine();
                    Rect rc = new Rect(startPosition, Input.mousePosition - startPosition);
                    if (rc.size.magnitude > 25)
                    {
                        var mfs = SelectTerrain();
                        UploadTerrain(mfs);
                        state = State.Uploading;
                    }
                    else
                    {
                        state = State.Idle;
                    }
                    camcontroller.LockFunctions = false;
                }
                break;

            case State.Uploading:
                // Wait..
                
                break;
        }
    }

    MeshFilter[] SelectTerrain()
    {
        // Temporarilly disable pijlenPrefab as we do not want to include in the selection.
        var pijlenPrefab = GameObject.FindGameObjectsWithTag("PijlenPrefab");
        if (pijlenPrefab != null)
        {
            foreach (var pb in pijlenPrefab)
                pb.SetActive(false);
        }

        // Disable Sphere (1) if is required to be in scene, to avoid occluding the export
        var sphere = GameObject.Find("Sphere (1)");
        bool sphereWasActive = false;
        if (sphere != null) sphereWasActive = sphere.activeSelf;
        if (sphere != null) sphere.SetActive(false);

        // Attempt to include trees
        GameObject bomen = GameObject.Find("Bomen");
        //if (bomen != null)
        //{
        //    for (int i = 0; i < bomen.transform.childCount; i++)
        //    {
        //        bomen.transform.GetChild(i).gameObject.AddComponent<MeshCollider>();
        //    }
        //}

        
        List<MeshFilter> selected = new List<MeshFilter>();
        var ray1 = Camera.main.ScreenPointToRay(startPosition);
        var ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
        var ray3 = Camera.main.ScreenPointToRay(new Vector2(Input.mousePosition.x, startPosition.y));
        var ray4 = Camera.main.ScreenPointToRay(new Vector2(startPosition.x, Input.mousePosition.y));
        RaycastHit hit1, hit2, hit3, hit4;
        
        if (Physics.Raycast(ray1, out hit1, 10000, LayerMask.GetMask("Terrein", "Terrain")) &&
            Physics.Raycast(ray2, out hit2, 10000, LayerMask.GetMask("Terrein", "Terrain")) &&
            Physics.Raycast(ray3, out hit3, 10000, LayerMask.GetMask("Terrein", "Terrain")) &&
            Physics.Raycast(ray4, out hit4, 10000, LayerMask.GetMask("Terrein", "Terrain")))
        {
            Bounds bb = new Bounds();

           // Debug.DrawRay(hit1.point, Vector3.up * 100, Color.yellow, 20, false);
           // Debug.DrawRay(hit2.point, Vector3.up * 100, Color.yellow, 20, false);
            Vector3 min = Vector3.Min(hit4.point, Vector3.Min(hit3.point, Vector3.Min(hit1.point, hit2.point)));
            Vector3 max = Vector3.Max(hit4.point, Vector3.Max(hit3.point, Vector3.Max(hit1.point, hit2.point)));

            bb.min = min + Vector3.down * 10;
            bb.max = max + Vector3.up * 100;
            lastBounds = bb;

            var cc = Physics.OverlapBox(bb.center, bb.extents);
            foreach( var c in cc )
            {
                var mrs = c.GetComponentsInChildren<MeshRenderer>();
                foreach( var mr in mrs )
                {
                    var mf = mr.GetComponent<MeshFilter>();
                    if (mr != null && mf != null)
                    {
                        if (bb.Intersects(mr.bounds))
                        {
                            if(!selected.Contains(mf))
                                selected.Add(mf);
                        }
                    }
                }
            }
        }

        // Re-enable pijlenPrefab
        if (pijlenPrefab != null)
        {
            foreach (var pb in pijlenPrefab)
                pb.SetActive(true);
        }
        if (sphere != null) sphere.SetActive(sphereWasActive);

        // Undo collider on trees
        if (bomen != null)
        {
            for (int i = 0; i < bomen.transform.childCount; i++)
            {
                Destroy(bomen.transform.GetChild(i).gameObject.GetComponent<MeshCollider>());
            }
        }
        return selected.ToArray();
    }

    private void UploadTerrain(MeshFilter [] mfs)
    {
        var oriMfs = Highlighter.SetColor(mfs, Color.red);
        StartCoroutine(ResetColorAndSaveArea(mfs, oriMfs));
    }

    IEnumerator ResetColorAndSaveArea(MeshFilter [] mfs, OriginalMeshfilter[] oriMfs)
    {
        yield return new WaitForSeconds(1);
        Highlighter.ResetMaterials(oriMfs);
        SelectionSaver.SaveMeshFilters(mfs, (bool succes) =>
        {
        });
        state = State.Idle;
        transform.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        state = State.Idle;
        EndDrawingLine();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(lastBounds.center, lastBounds.size);
    }
}
