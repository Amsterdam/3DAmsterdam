using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

namespace Netherlands3D.T3D
{
    public enum EditMode
    {
        None = -1,
        Resize = 0,
        Reposition = 1,
    }

    public abstract class DistanceMeasurement : MonoBehaviour
    {
        [SerializeField]
        protected EditMode mode = EditMode.Reposition;
        public EditMode Mode => mode;

        [SerializeField]
        private GameObject measurementLine;
        [SerializeField]
        protected int numberOfLines = 2;

        protected BuildingMeasuring[] lines;

        public bool DrawDistanceActive { get; set; } = false;

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

        private void OnDestroy()
        {
            if (lines != null) //sometimes a nullreference is thrown without this for some reason
            {
                foreach (var line in lines)
                {
                    if (line)
                        Destroy(line.gameObject);
                }
            }
        }
    }
}
