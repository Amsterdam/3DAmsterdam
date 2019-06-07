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
        lineRenderer.enabled = false;
        lineRenderer.Points = null;
    }

    void Update()
    {
        switch ( state)
        {
            case State.Idle:
                if (Input.GetMouseButtonDown(0))
                {
                    StartDrawingLine();
                    state = State.Selecting;
                }
                break;

            case State.Selecting:
                PreviewLine();
                if (Input.GetMouseButtonUp(0))
                {
                    EndDrawingLine();
                    Rect rc = new Rect(startPosition, Input.mousePosition);
                    if (rc.size.magnitude > 5)
                    {
                        var mfs = SelectTerrain();
                        UploadTerrain(mfs);
                        state = State.Uploading;
                    }
                    else
                    {
                        state = State.Idle;
                    }
                }
                break;

            case State.Uploading:
                // Wait..
                break;
        }
    }

    MeshFilter[] SelectTerrain()
    {
        List<MeshFilter> selected = new List<MeshFilter>();
        var ray1 = Camera.main.ScreenPointToRay(startPosition);
        var ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit1, hit2;
        if (Physics.Raycast(ray1, out hit1, 10000, LayerMask.GetMask("Terrein")) &&
            Physics.Raycast(ray2, out hit2, 10000, LayerMask.GetMask("Terrein")))
        {
            Bounds bb = new Bounds();

            Vector3 min = Vector3.Min(hit1.point, hit2.point);
            Vector3 max = Vector3.Max(hit1.point, hit2.point);

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
        return selected.ToArray();
    }

    private void UploadTerrain(MeshFilter [] mfs)
    {
        var oriMfs = Highlighter.SetColor(mfs, Color.red);
        SelectionSaver.SaveMeshFilters(mfs, (bool succes) =>
        {
            StartCoroutine(ResetColor(oriMfs));
        });
    }

    IEnumerator ResetColor(OriginalMeshfilter[] oriMfs)
    {
        yield return new WaitForSeconds(3);
        Highlighter.ResetMaterials(oriMfs);
        state = State.Idle;
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
