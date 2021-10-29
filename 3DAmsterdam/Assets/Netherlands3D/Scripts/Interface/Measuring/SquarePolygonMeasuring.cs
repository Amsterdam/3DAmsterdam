using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquarePolygonMeasuring : MonoBehaviour
{
    [SerializeField]
    private GameObject measurementLine;

    private SquarePolygon square;
    private BuildingMeasuring[] lines = new BuildingMeasuring[2];

    public bool DrawDistanceActive { get; set; } = true;

    private void Start()
    {
        square = GetComponent<SquarePolygon>();

        DrawNewMeasurement(0);
        DrawNewMeasurement(1);
    }

    void DrawNewMeasurement(int index)
    {
        var lineObject = Instantiate(measurementLine);
        var measuring = lineObject.GetComponent<BuildingMeasuring>();
        measuring.DistanceInputOverride += Measuring_DistanceInputOverride;
        lines[index] = measuring;
    }

    private void Measuring_DistanceInputOverride(BuildingMeasuring source, Vector3 direction, float delta)
    {
        //var axis = (source.LinePoints[1].transform.position - source.LinePoints[0].transform.position).normalized;
        //uitbouw.transform.position += direction * delta;

        var deltaVector = Quaternion.Inverse(transform.rotation) * direction * delta;
        var newSize = square.Size - (Vector2)deltaVector;

        square.SetSize(newSize);
    }

    private void Update()
    {
        lines[0].gameObject.SetActive(DrawDistanceActive);
        lines[1].gameObject.SetActive(DrawDistanceActive);
        if (DrawDistanceActive)
        {
            var corners = square.Polygon;

            DrawLine(0, corners[0], corners[1]); //direction matters for resize
            DrawLine(1, corners[2], corners[1]); //direction matters for resize
        }
    }

    private void DrawLine(int lineIndex, Vector3 start, Vector3 end)
    {
        lines[lineIndex].SetLinePosition(start, end);
    }
}
