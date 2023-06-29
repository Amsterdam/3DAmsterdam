using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CurrentCoords : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textPositionX;
    [SerializeField] private TextMeshProUGUI textPositionY;
    [SerializeField] private TextMeshProUGUI textPositionZ;

    [SerializeField] private TextMeshProUGUI textRotationX;
    [SerializeField] private TextMeshProUGUI textRotationY;
    [SerializeField] private TextMeshProUGUI textRotationZ;

    [SerializeField] private bool metric;

    [SerializeField] private bool isUpdating = true;

    public double PositionX { get; private set; }
    public double PositionY { get; private set; }
    public double PositionZ { get; private set; }
    public double RotationX { get; private set; }
    public double RotationY { get; private set; }
    public double RotationZ { get; private set; }

    // Update is called once per frame
    void Update()
    {
        if (!isUpdating) return;
        SetCoords();
    }

    private void SetCoords()
    {
        var currentPosition = CoordConvert.UnitytoRD(Camera.main.transform.position);
        PositionX = currentPosition.x;
        PositionY = currentPosition.y;
        PositionZ = currentPosition.z;

        var currentRotation = Camera.main.transform.rotation;
        RotationX = currentRotation.x;
        RotationY = currentRotation.y;
        RotationZ = currentRotation.z;

        //Setting the fields
        SetTextField(textPositionX, PositionX, metric ? "x" : null);
        SetTextField(textPositionY, PositionY, metric ? "y" : null);
        SetTextField(textPositionZ, PositionZ, metric ? "z" : null);

        SetTextField(textRotationX, RotationX, metric ? "x" : null);
        SetTextField(textRotationY, RotationY, metric ? "y" : null);
        SetTextField(textRotationZ, RotationZ, metric ? "z" : null);
    }

    private void SetTextField(TextMeshProUGUI textField, double value, string metric = "")
    {
        if (textField) textField.text = $"{value.ToString("00")}{metric}";
    }
}
