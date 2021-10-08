using Netherlands3D.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
}