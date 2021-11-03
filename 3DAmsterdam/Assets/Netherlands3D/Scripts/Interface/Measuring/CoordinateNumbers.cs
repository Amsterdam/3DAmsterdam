using Netherlands3D.Interface;
using Netherlands3D.T3D.Uitbouw.BoundaryFeatures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    /// <summary>
    /// Add 2D numbers on the canvas and return its objects.
    /// </summary>
    public class CoordinateNumbers : MonoBehaviour
    {
        [SerializeField]
        private Coordinate coordinatePrefab;

        [SerializeField]
        private Distance distancePrefab;

        [SerializeField]
        private NumberInputField numberInputFieldPrefab;

        [SerializeField]
        private EditUI editUIPrefab;

        public static CoordinateNumbers Instance;

        void Awake()
        {
            Instance = this;
        }

        public Coordinate CreateCoordinateNumber()
        {
            return Instantiate(coordinatePrefab,this.transform);
        }

        public Distance CreateDistanceNumber()
        {
            return Instantiate(distancePrefab, this.transform);
        }

        public NumberInputField CreateNumberInputField()
        {
            return Instantiate(numberInputFieldPrefab, this.transform);
        }

        public EditUI CreateEditUI(BoundaryFeature feature)
        {
            var ui = Instantiate(editUIPrefab, this.transform);
            ui.SetFeature(feature);
            return ui;
        }
    }
}