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
    bool startedSelecting;

    private void Start()
    {
        lineRenderer = GetComponent<UILineRenderer>();
        
    }

    Vector3 ToWorldPosition(Vector3 scrp)
    {
        Vector3 positionToReturn = Camera.main.ScreenToWorldPoint(scrp);
        return positionToReturn;
    }

    void StartDrawingLine()
    {
        startPosition = Input.mousePosition;
        lineRenderer.Points = null;
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
    }

    void Update()
    {
        if (!startedSelecting && Input.GetMouseButtonDown(0))
        {
            StartDrawingLine();
            startedSelecting = true;
        }
        else if (startedSelecting && Input.GetMouseButton(0))
        {
            PreviewLine();
        }
        else if (startedSelecting && Input.GetMouseButtonUp(0))
        {
            startedSelecting = false;
            EndDrawingLine();
            var mfs = SelectTerrain();
            UploadTerrain(mfs);
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
            foreach( var c in cc)
            {
                var mr = c.GetComponent<MeshRenderer>();
                var mf = c.GetComponent<MeshFilter>();
                if (mr != null && mf != null)
                {
                    if (bb.Intersects(mr.bounds))
                    {
                        selected.Add(mf);
                    }
                }
            }
        }
        return selected.ToArray();
    }

    private void UploadTerrain(MeshFilter [] mfs)
    {
        var oriMfs = Highlighter.SetColor(mfs, Color.red);

        TileSaver.SaveMeshFilters(mfs, (bool succes) =>
        {
          //   MarkColor(panden, succes ? Color.green : Color.red);
             StartCoroutine(ResetColor(oriMfs));
        });
    }

    IEnumerator ResetColor(OriginalMeshfilter[] oriMfs)
    {
        yield return new WaitForSeconds(3);
        Highlighter.ResetMaterials(oriMfs);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(lastBounds.center, lastBounds.size);
    }
}
