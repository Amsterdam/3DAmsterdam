using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public abstract class DistanceMeasurement : MonoBehaviour
{
    [SerializeField]
    private GameObject measurementLine;
    protected BuildingMeasuring[] lines = new BuildingMeasuring[2];

    public bool DrawDistanceActive { get; set; } = true;

    protected void Start()
    {
        DrawNewMeasurement(0);
        DrawNewMeasurement(1);
    }

    protected void DrawNewMeasurement(int index)
    {
        var lineObject = Instantiate(measurementLine);
        var measuring = lineObject.GetComponent<BuildingMeasuring>();
        measuring.DistanceInputOverride += Measuring_DistanceInputOverride;
        lines[index] = measuring;
    }

    protected abstract void Measuring_DistanceInputOverride(BuildingMeasuring source, Vector3 direction, float delta);

    protected void Update()
    {
        UpdateMeasurementLines();
    }

    protected void UpdateMeasurementLines()
    {
        lines[0].gameObject.SetActive(DrawDistanceActive);
        lines[1].gameObject.SetActive(DrawDistanceActive);
        if (DrawDistanceActive)
        {
            DrawLines();
        }
    }

    protected abstract void DrawLines();

    protected void DrawLine(int lineIndex, Vector3 start, Vector3 end)
    {
        lines[lineIndex].SetLinePosition(start, end);
    }
}
