using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public abstract class DistanceMeasurement : MonoBehaviour
{
    [SerializeField]
    private GameObject measurementLine;
    [SerializeField]
    protected int numberOfLines = 2;

    protected BuildingMeasuring[] lines;

    public bool DrawDistanceActive { get; set; } = true;

    protected void Start()
    {
        lines = new BuildingMeasuring[numberOfLines];

        for (int i = 0; i < numberOfLines; i++)
        {
            DrawNewMeasurement(i);
        }
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
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].gameObject.SetActive(DrawDistanceActive);
        }

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
